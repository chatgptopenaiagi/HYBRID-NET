using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Hybrid.Core;

public sealed class ContractException(string code) : Exception(code);

public static class JsonData
{
    public static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    public static string Text(JsonNode? value) => value?.ToJsonString(Options) ?? "null";
    public static JsonObject Read(string path) => JsonNode.Parse(File.ReadAllText(path)) as JsonObject ?? throw new ContractException("INVALID_JSON_OBJECT");
    public static string HashFile(string path) { using var file = File.OpenRead(path); return Convert.ToHexStringLower(SHA256.HashData(file)); }
    public static string Hash(string text) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    public static JsonObject Clone(JsonObject value) => (JsonObject)value.DeepClone();
    public static void WriteNew(string path, JsonObject value) { using var file = new FileStream(path, FileMode.CreateNew); JsonSerializer.Serialize(file, value, Options); file.Flush(true); }
}

public sealed record ValidatedRequest(string Id, string Capability, string Source, string Destination, string CriteriaPath, int TimeoutSeconds, string Hash, JsonObject Json);

public static class Contracts
{
    public const string ProtocolVersion = "0.1";
    private static readonly HashSet<string> RootFields = ["protocol", "version", "operation_id", "capability", "application", "parameters", "options"];
    public static JsonObject Parse(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 16 });
            CheckDuplicates(doc.RootElement);
            return JsonNode.Parse(json) as JsonObject ?? throw new ContractException("INVALID_JSON_OBJECT");
        }
        catch (JsonException) { throw new ContractException("MALFORMED_JSON"); }
    }
    private static void CheckDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in value.EnumerateObject()) { if (!names.Add(property.Name)) throw new ContractException("DUPLICATE_JSON_FIELD"); CheckDuplicates(property.Value); }
        }
        else if (value.ValueKind == JsonValueKind.Array) foreach (var item in value.EnumerateArray()) CheckDuplicates(item);
    }
    private static string Required(JsonObject o, string key)
    {
        if (o[key] is not JsonValue v || !v.TryGetValue<string>(out var s) || string.IsNullOrWhiteSpace(s)) throw new ContractException("INVALID_" + key.ToUpperInvariant());
        return s;
    }
    private static void Allowed(JsonObject obj, IEnumerable<string> fields)
    {
        var allow = fields.ToHashSet(StringComparer.Ordinal);
        if (obj.Any(p => !allow.Contains(p.Key))) throw new ContractException("UNSUPPORTED_FIELD");
    }
    public static string SafePath(string path, string workspace)
    {
        if (!Path.IsPathFullyQualified(path) || path.StartsWith(@"\\", StringComparison.Ordinal) || path.IndexOfAny(Path.GetInvalidPathChars()) >= 0) throw new ContractException("INVALID_PATH");
        foreach(var segment in path[(Path.GetPathRoot(path)?.Length ?? 0)..].Split([Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar]))
            if(segment.IndexOfAny(Path.GetInvalidFileNameChars())>=0 || (segment is not ("." or "..") && (segment.EndsWith('.')||segment.EndsWith(' '))) || Regex.IsMatch(segment,@"\A(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\.|\z)",RegexOptions.IgnoreCase)) throw new ContractException("INVALID_PATH");
        string full;
        try { full = Path.GetFullPath(path); } catch (Exception e) when(e is ArgumentException or NotSupportedException or PathTooLongException) { throw new ContractException("INVALID_PATH"); }
        var root = Path.GetFullPath(workspace).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new ContractException("PATH_OUTSIDE_WORKSPACE");
        var relative = Path.GetRelativePath(root, full);
        foreach (var part in relative.Split(Path.DirectorySeparatorChar))
        {
            if (part.Contains(':') || part.EndsWith('.') || part.EndsWith(' ') || Regex.IsMatch(part, @"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\.|$)", RegexOptions.IgnoreCase)) throw new ContractException("INVALID_PATH");
        }
        for (string? current = full; current != null && current.Length >= root.TrimEnd(Path.DirectorySeparatorChar).Length; current = Path.GetDirectoryName(current))
            if ((File.Exists(current) || Directory.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) throw new ContractException("REPARSE_PATH_REJECTED");
        return full;
    }
    public static ValidatedRequest Validate(JsonObject request, string workspace, string defaultCriteria)
    {
        Allowed(request, RootFields);
        if (Required(request,"protocol") != "HMCP" || Required(request,"version") != ProtocolVersion) throw new ContractException("UNSUPPORTED_PROTOCOL_VERSION");
        var id=Required(request,"operation_id");
        if (!Regex.IsMatch(id,@"\A[A-Za-z0-9][A-Za-z0-9_-]{0,79}\z")) throw new ContractException("INVALID_OPERATION_ID");
        var capability=Required(request,"capability");
        if (capability is not ("document.export_pdf" or "text.normalize")) throw new ContractException("UNSUPPORTED_CAPABILITY");
        if (Required(request,"application") != (capability=="document.export_pdf" ? "LibreOffice" : "Hybrid.Text")) throw new ContractException("APPLICATION_MISMATCH");
        var parameters=request["parameters"] as JsonObject ?? throw new ContractException("INVALID_PARAMETERS");
        Allowed(parameters,["source","destination"]);
        var source=SafePath(Required(parameters,"source"),workspace);
        var destination=SafePath(Required(parameters,"destination"),workspace);
        if (string.Equals(source,destination,StringComparison.OrdinalIgnoreCase)) throw new ContractException("SOURCE_DESTINATION_CONFLICT");
        var expectedExtension=capability=="document.export_pdf" ? ".pdf" : ".txt";
        if (!string.Equals(Path.GetExtension(destination),expectedExtension,StringComparison.OrdinalIgnoreCase)) throw new ContractException("INVALID_OUTPUT_EXTENSION");
        if (capability=="document.export_pdf" && !string.Equals(Path.GetExtension(source),".docx",StringComparison.OrdinalIgnoreCase)) throw new ContractException("INVALID_INPUT_EXTENSION");
        var timeout=60; var criteria=defaultCriteria;
        if (request.ContainsKey("options"))
        {
            var options=request["options"] as JsonObject ?? throw new ContractException("INVALID_OPTIONS");
            Allowed(options,["timeout_seconds","criteria_path"]);
            if (options.ContainsKey("timeout_seconds") && (options["timeout_seconds"] is not JsonValue value || !value.TryGetValue<int>(out timeout) || timeout<1 || timeout>300)) throw new ContractException("INVALID_TIMEOUT");
            if(options.ContainsKey("criteria_path")) criteria=SafePath(Required(options,"criteria_path"),workspace);
        }
        var normalized=JsonData.Clone(request);
        normalized["parameters"]=new JsonObject { ["source"]=source,["destination"]=destination };
        normalized["options"]=new JsonObject { ["timeout_seconds"]=timeout,["criteria_path"]=criteria };
        // Canonical property ordering makes an identical request independent of JSON key order.
        var canonical = Canonical(normalized);
        return new(id,capability,source,destination,criteria,timeout,JsonData.Hash(canonical.ToJsonString()),normalized);
    }
    private static JsonNode Canonical(JsonNode node) => node is JsonObject o ? new JsonObject(o.OrderBy(p=>p.Key,StringComparer.Ordinal).Select(p=>new KeyValuePair<string,JsonNode?>(p.Key,p.Value is null ? null : Canonical(p.Value)))) : node.DeepClone();
}

public static class Lifecycle
{
    private static readonly Dictionary<string,string[]> Legal = new()
    {
        ["CREATED"]=["VALIDATED","FAILED","INCONCLUSIVE"], ["VALIDATED"]=["PREPARING","FAILED","INCONCLUSIVE","CANCELLED"],
        ["PREPARING"]=["RUNNING","FAILED","INCONCLUSIVE","CANCELLED"], ["RUNNING"]=["EXECUTED","FAILED","INCONCLUSIVE","CANCELLED"],
        ["EXECUTED"]=["VERIFYING","FAILED","INCONCLUSIVE"], ["VERIFYING"]=["VERIFIED","FAILED","INCONCLUSIVE","CANCELLED"]
    };
    public static void Check(string before,string after)
    { if(!Legal.TryGetValue(before,out var options)||!options.Contains(after)) throw new InvalidOperationException($"ILLEGAL_TRANSITION:{before}:{after}"); }
}
