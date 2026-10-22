using System.Diagnostics;
using System.Text.Json.Nodes;

namespace Hybrid.Core;

public sealed class HybridRuntime
{
    public string Workspace { get; }
    public EvidenceStore Store { get; }
    private readonly string python;
    private readonly string verifierDirectory;
    private readonly Dictionary<string,ICapabilityAdapter> adapters;
    public HybridRuntime(string workspace,string pythonPath,string verifierPath,string libreOfficePath)
    {
        if(!Path.IsPathFullyQualified(workspace)||workspace.StartsWith(@"\\",StringComparison.Ordinal)) throw new ContractException("INVALID_WORKSPACE");
        Workspace=Path.GetFullPath(workspace);
        for(string? ancestor=Workspace;ancestor is not null;ancestor=Path.GetDirectoryName(ancestor))
            if((Directory.Exists(ancestor)||File.Exists(ancestor))&&(File.GetAttributes(ancestor)&FileAttributes.ReparsePoint)!=0) throw new ContractException("REPARSE_WORKSPACE_REJECTED");
        Directory.CreateDirectory(Workspace);
        python=Path.GetFullPath(pythonPath); verifierDirectory=Path.GetFullPath(verifierPath);
        Store=new EvidenceStore(Contracts.SafePath(Path.Combine(Workspace,"engine","evidence.sqlite"),Workspace));
        adapters=new ICapabilityAdapter[] {new LibreOfficeAdapter(libreOfficePath,Contracts.SafePath(Path.Combine(Workspace,"engine","discovery"),Workspace)),new TextNormalizeAdapter()}.ToDictionary(a=>a.Capability);
        Store.RecoverInterrupted();
    }
    public async Task<JsonArray> DiscoverAsync(CancellationToken token=default)
    {
        var result=new JsonArray();
        foreach(var adapter in adapters.Values)
        {
            var capability=await adapter.DiscoverAsync(token);
            var last=Store.LastVerified(adapter.Capability,capability["environment_key"]?.GetValue<string>() ?? "");
            bool intact=false;
            if(last?["artifacts"] is JsonArray artifacts && artifacts.Count>0)
            {
                try { intact=artifacts.All(a=>a?["path"] is not null && File.Exists(a["path"]!.GetValue<string>()) && JsonData.HashFile(a["path"]!.GetValue<string>())==a["sha256"]?.GetValue<string>()); } catch(IOException) { }
            }
            capability["capability_verified"]=last is not null && intact;
            capability["last_successful_verification"]=last is null ? null : new JsonObject { ["operation_id"]=last["operation_id"]?.DeepClone(),["evidence_reference"]=Store.Path,["artifact_integrity_current"]=intact };
            capability["availability"]=capability["application_installed"]?.GetValue<bool>()!=true || capability["availability"]?.GetValue<string>()=="unavailable" ? "unavailable" : intact ? "verified_for_recorded_case" : "registered_unverified";
            result.Add(capability);
        }
        Store.RecordRegistry(result); return result;
    }
    private string CriteriaFor(string capability) => Path.Combine(verifierDirectory,capability=="document.export_pdf" ? "genesis2-criteria.json" : "text-criteria.json");
    private static string[] CriterionIds(string capability) => capability=="document.export_pdf" ? Enumerable.Range(1,9).Select(n=>$"AC-{n:00}").ToArray() : Enumerable.Range(1,5).Select(n=>$"TC-{n:00}").ToArray();
    private static JsonObject UnavailableVerification(string capability,string reason) => new()
    {
        ["schema_version"]="1.0",["outcome"]="INCONCLUSIVE",["observed"]=false,["error_code"]=reason,
        ["verifier"]=new JsonObject { ["id"]="independent-verifier-unavailable" },
        ["criteria"]=new JsonArray(CriterionIds(capability).Select(id=>(JsonNode)new JsonObject { ["id"]=id,["expected"]="Predetermined criterion must be established",["observed"]=reason,["outcome"]="INCONCLUSIVE",["evidence_reference"]="execution diagnostics",["verifier_id"]="runtime-unavailability-recorder" }).ToArray())
    };
    public async Task<JsonObject> ExecuteAsync(JsonObject raw,CancellationToken token=default)
    {
        var timer=Stopwatch.StartNew();
        ValidatedRequest request;
        try { request=Contracts.Validate(raw,Workspace,CriteriaFor(raw["capability"]?.GetValue<string>() ?? "")); }
        catch(Exception e) when(e is ContractException or InvalidOperationException or FormatException)
        { return Reject(raw,e is ContractException ? e.Message : "INVALID_FIELD_TYPE"); }
        // Prefix avoids Windows device names; hash disambiguates case-sensitive IDs on NTFS.
        var run=Contracts.SafePath(Path.Combine(Workspace,"runs","op-"+request.Id+"-"+JsonData.Hash(request.Id)[..12]),Workspace);
        if(!Store.Reserve(request.Id,request.Hash,request.Json,run))
        {
            var existing=Store.GetReceipt(request.Id)!; existing["duplicate_request"]=true; existing["replayed"]=false; return existing;
        }
        var state="CREATED"; var level="V0";
        void Transition(string next,string evidenceLevel,JsonObject? detail=null) { Lifecycle.Check(state,next); state=next; level=evidenceLevel; Store.AppendEvent(request.Id,state,level,detail ?? new JsonObject()); }
        var receipt=new JsonObject { ["protocol"]="HMCP",["version"]="0.1",["operation_id"]=request.Id,["capability"]=request.Capability,["request"]=request.Json.DeepClone(),["started_at"]=DateTimeOffset.UtcNow.ToString("O"),["run_directory"]=run };
        JsonObject execution=new(),discovery=new(),verification=UnavailableVerification(request.Capability,"NOT_EXECUTED");
        string? sourceHash=null,criteriaHash=null,snapshot=null;
        try
        {
            Transition("VALIDATED","V1"); Directory.CreateDirectory(run);
            JsonData.WriteNew(Path.Combine(run,"request.json"),request.Json);
            if(!File.Exists(request.Source)) throw new ContractException("SOURCE_NOT_FOUND");
            if(new FileInfo(request.Source).Length>32*1024*1024) throw new ContractException("SOURCE_TOO_LARGE");
            if(File.Exists(request.Destination)||Directory.Exists(request.Destination)) throw new ContractException("DESTINATION_EXISTS");
            if(!Directory.Exists(Path.GetDirectoryName(request.Destination))) throw new ContractException("DESTINATION_PARENT_MISSING");
            if(!File.Exists(request.CriteriaPath)) throw new ContractException("CRITERIA_NOT_FOUND");
            sourceHash=JsonData.HashFile(request.Source); snapshot=Path.Combine(run,"criteria.json");
            File.Copy(request.CriteriaPath,snapshot,false); criteriaHash=JsonData.HashFile(snapshot);
            discovery=await adapters[request.Capability].DiscoverAsync(token);
            receipt["discovery"]=discovery.DeepClone();
            Transition("PREPARING","V1"); token.ThrowIfCancellationRequested();
            if(discovery["application_installed"]?.GetValue<bool>()!=true) throw new ContractException("APPLICATION_UNAVAILABLE");
            Transition("RUNNING","V1");
            execution=await adapters[request.Capability].ExecuteAsync(request.Source,request.Destination,run,TimeSpan.FromSeconds(request.TimeoutSeconds),token);
            var executionPath=Path.Combine(run,"execution.json"); JsonData.WriteNew(executionPath,execution);
            if(execution["cancelled"]?.GetValue<bool>()==true) { verification=UnavailableVerification(request.Capability,"EXECUTION_CANCELLED"); Transition("CANCELLED","V1"); }
            else if(execution["timed_out"]?.GetValue<bool>()==true) { verification=UnavailableVerification(request.Capability,"EXECUTION_TIMEOUT"); Transition("INCONCLUSIVE","V1"); }
            else
            {
                Transition("EXECUTED","V2"); Transition("VERIFYING","V2");
                verification=await VerifyFiles(request.Capability,request.Source,request.Destination,sourceHash,snapshot,executionPath,run,token);
                if(verification["observed"]?.GetValue<bool>()==true) { level="V3"; Store.AppendEvent(request.Id,state,level,new JsonObject { ["event"]="INDEPENDENT_OBSERVATION" }); }
                if(execution["exit_code"]?.GetValue<int>()!=0 && verification["outcome"]?.GetValue<string>()=="VERIFIED") verification["outcome"]="FAILED";
            }
        }
        catch(OperationCanceledException) { verification=UnavailableVerification(request.Capability,"EXECUTION_CANCELLED"); if(state is not ("CANCELLED" or "INCONCLUSIVE")) Transition("CANCELLED",level); }
        catch(Exception e) when(e is ContractException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            receipt["error"]=new JsonObject { ["code"]=e is ContractException ? e.Message : e.GetType().Name,["category"]=e is ContractException ? "REQUEST_OR_APPLICATION_FAILURE" : "INFRASTRUCTURE_FAILURE" };
            verification=UnavailableVerification(request.Capability,receipt["error"]!["code"]!.GetValue<string>());
            if(state is not ("FAILED" or "CANCELLED" or "INCONCLUSIVE")) Transition(e is ContractException ? "FAILED" : "INCONCLUSIVE",level);
        }
        var artifacts=new JsonArray();
        if(File.Exists(request.Destination)) artifacts.Add(new JsonObject { ["path"]=request.Destination,["sha256"]=JsonData.HashFile(request.Destination),["size"]=new FileInfo(request.Destination).Length,["type"]=request.Capability=="document.export_pdf" ? "pdf" : "text" });
        receipt["source_sha256"]=sourceHash; receipt["criteria_sha256"]=criteriaHash; receipt["criteria_path"]=snapshot;
        receipt["environment_key"]=discovery["environment_key"]?.DeepClone(); receipt["execution"]=execution.DeepClone(); receipt["discovery"]=discovery.DeepClone(); receipt["artifacts"]=artifacts;
        var storageTimer=Stopwatch.StartNew();
        try
        {
            var provisional=JsonData.Clone(receipt); provisional["verification"]=verification.DeepClone();
            var verificationId=Store.SaveVerification(request.Id,provisional,"provisional");
            var readback=await Readback(request.Id,verificationId,run);
            var criterion=new JsonObject { ["id"]=request.Capability=="document.export_pdf" ? "AC-10" : "TC-06",["expected"]="Committed execution and verification evidence survives a new SQLite connection",["observed"]=readback.DeepClone(),["outcome"]=readback["outcome"]?.DeepClone() ?? JsonValue.Create("INCONCLUSIVE"),["evidence_reference"]=Store.Path,["verifier_id"]="hybrid.sqlite.independent-readback" };
            ((JsonArray)verification["criteria"]!).Add(criterion);
            CheckIdentityAtGate(verification,request.Capability,request.Source,request.Destination,sourceHash,snapshot,criteriaHash);
            var expectedIds=CriterionIds(request.Capability).Append(criterion["id"]!.GetValue<string>()).ToArray();
            var criteria=((JsonArray)verification["criteria"]!).OfType<JsonObject>().ToArray();
            if(criteria.Length!=expectedIds.Length || !criteria.Select(c=>c["id"]?.GetValue<string>()).Order().SequenceEqual(expectedIds.Order())) verification["outcome"]="INCONCLUSIVE";
            else if(criteria.Any(c=>c["outcome"]?.GetValue<string>()=="FAIL")) verification["outcome"]="FAILED";
            else if(criteria.Any(c=>c["outcome"]?.GetValue<string>()!="PASS")) verification["outcome"]="INCONCLUSIVE";
            else if(state=="VERIFYING" && verification["observed"]?.GetValue<bool>()==true && execution["exit_code"]?.GetValue<int>()==0 && execution["error_code"] is null && execution["timed_out"]?.GetValue<bool>()!=true && execution["cancelled"]?.GetValue<bool>()!=true) verification["outcome"]="VERIFIED";
            else if(state=="VERIFYING") verification["outcome"]="INCONCLUSIVE";
            // Missing execution/verification infrastructure is uncertainty about correctness.
            if(verification["error_code"] is not null) verification["outcome"]="INCONCLUSIVE";
            if(state=="VERIFYING")
            {
                var terminal=verification["outcome"]!.GetValue<string>(); Lifecycle.Check(state,terminal);
                // Receipt and terminal event are committed together by EvidenceStore.Complete.
                state=terminal; if(terminal=="VERIFIED") level="V4";
            }
            verification["level"]=level;
            verification["verification_id"]=Store.SaveVerification(request.Id,verification,"initial");
        }
        catch(Exception e) when(e is IOException or InvalidOperationException or Microsoft.Data.Sqlite.SqliteException)
        { verification["outcome"]="INCONCLUSIVE"; receipt["persistence_error"]=e.GetType().Name; state="INCONCLUSIVE"; if(level=="V4") level="V3"; verification["level"]=level; }
        receipt["state"]=state; receipt["verification"]=verification; receipt["finished_at"]=DateTimeOffset.UtcNow.ToString("O");
        receipt["metrics"]=new JsonObject { ["total_wall_ms"]=timer.Elapsed.TotalMilliseconds,["execution_ms"]=execution["duration_ms"]?.DeepClone(),["verification_ms"]=verification["duration_ms"]?.DeepClone(),["persistence_and_readback_ms"]=storageTimer.Elapsed.TotalMilliseconds,["retry_count"]=0,["human_time_recovered"]="NOT_ESTABLISHED" };
        Store.Complete(request.Id,receipt);
        if(Directory.Exists(run)) JsonData.WriteNew(Path.Combine(run,"receipt.json"),receipt);
        return receipt;
    }
    private JsonObject Reject(JsonObject raw,string code)
    {
        var id="rejected-"+Guid.NewGuid().ToString("N"); var run=Path.Combine(Workspace,"runs",id);
        Store.Reserve(id,JsonData.Hash(raw.ToJsonString()),raw,run);
        Store.AppendEvent(id,"FAILED","V0",new JsonObject { ["code"]=code });
        var receipt=new JsonObject { ["protocol"]="HMCP",["version"]="0.1",["operation_id"]=id,["requested_operation_id"]=raw["operation_id"]?.DeepClone(),["state"]="FAILED",["error"]=new JsonObject { ["code"]=code,["category"]="CONTRACT_VALIDATION" },["verification"]=new JsonObject { ["outcome"]="INCONCLUSIVE",["level"]="V0" } };
        Store.Complete(id,receipt); return receipt;
    }
    private async Task<JsonObject> VerifyFiles(string capability,string source,string output,string sourceHash,string criteriaPath,string executionPath,string run,CancellationToken token)
    {
        var file=Path.Combine(run,"verify-request-"+Guid.NewGuid().ToString("N")+".json");
        JsonData.WriteNew(file,new JsonObject { ["capability"]=capability,["source"]=source,["output"]=output,["source_sha256"]=sourceHash,["criteria_path"]=criteriaPath,["execution_path"]=executionPath });
        var process=await ProcessRunner.RunAsync(python,[Path.Combine(verifierDirectory,"verify.py"),"--request",file],run,TimeSpan.FromSeconds(30),token);
        JsonData.WriteNew(file.Replace(".json","-process.json"),process);
        if(process["exit_code"]?.GetValue<int>()!=0 || process["timed_out"]?.GetValue<bool>()==true || process["output_truncated"]?.GetValue<bool>()==true) return UnavailableVerification(capability,"VERIFIER_UNAVAILABLE");
        try { return JsonNode.Parse(process["stdout"]!.GetValue<string>()) as JsonObject ?? UnavailableVerification(capability,"VERIFIER_INVALID_RESPONSE"); }
        catch(System.Text.Json.JsonException) { return UnavailableVerification(capability,"VERIFIER_INVALID_RESPONSE"); }
    }
    private async Task<JsonObject> Readback(string id,string verificationId,string run)
    {
        Directory.CreateDirectory(run);
        var process=await ProcessRunner.RunAsync(python,[Path.Combine(verifierDirectory,"store_readback.py"),"--database",Store.Path,"--operation-id",id,"--verification-id",verificationId],run,TimeSpan.FromSeconds(15));
        JsonData.WriteNew(Path.Combine(run,"readback-"+Guid.NewGuid().ToString("N")+".json"),process);
        try { if(process["exit_code"]?.GetValue<int>()==0) return JsonNode.Parse(process["stdout"]!.GetValue<string>()) as JsonObject ?? new JsonObject { ["outcome"]="INCONCLUSIVE" }; }
        catch(System.Text.Json.JsonException) { }
        return new JsonObject { ["outcome"]="INCONCLUSIVE",["error_code"]="STORAGE_READBACK_UNAVAILABLE" };
    }
    private static void CheckIdentityAtGate(JsonObject verification,string capability,string source,string destination,string? sourceHash,string? criteriaPath,string? criteriaHash)
    {
        if(verification["outcome"]?.GetValue<string>()!="VERIFIED") return;
        bool stable;
        try
        {
            stable=sourceHash is not null && File.Exists(source) && JsonData.HashFile(source)==sourceHash
                && verification["source_sha256_after"]?.GetValue<string>()==sourceHash
                && File.Exists(destination) && JsonData.HashFile(destination)==verification["output_sha256"]?.GetValue<string>()
                && criteriaPath is not null && File.Exists(criteriaPath) && JsonData.HashFile(criteriaPath)==criteriaHash
                && verification["criteria_sha256"]?.GetValue<string>()==criteriaHash;
        }
        catch(IOException) { stable=false; }
        if(stable) return;
        var criterion=((JsonArray)verification["criteria"]!).OfType<JsonObject>().FirstOrDefault(c=>c["id"]?.GetValue<string>()==(capability=="document.export_pdf" ? "AC-07" : "TC-04"));
        if(criterion is not null) { criterion["outcome"]="FAIL"; criterion["promotion_check"]="INPUT_OUTPUT_OR_CRITERIA_IDENTITY_CHANGED"; }
        verification["outcome"]="FAILED";
    }
    public async Task<JsonObject> ReverifyAsync(string id,CancellationToken token=default)
    {
        var original=Store.GetReceipt(id) ?? throw new ContractException("OPERATION_NOT_FOUND");
        var request=original["request"] as JsonObject ?? throw new ContractException("NO_EXECUTABLE_REQUEST");
        var parameters=(JsonObject)request["parameters"]!;
        var run=original["run_directory"]!.GetValue<string>();
        var capability=original["capability"]!.GetValue<string>(); var source=parameters["source"]!.GetValue<string>(); var output=parameters["destination"]!.GetValue<string>();
        if(original["source_sha256"] is null || original["criteria_path"] is null) throw new ContractException("NO_VERIFIABLE_EXECUTION");
        var sourceHash=original["source_sha256"]!.GetValue<string>(); var criteriaPath=original["criteria_path"]!.GetValue<string>(); var criteriaHash=original["criteria_sha256"]!.GetValue<string>();
        var verification=await VerifyFiles(capability,source,output,sourceHash,criteriaPath,Path.Combine(run,"execution.json"),run,token);
        var bundle=JsonData.Clone(original); bundle.Remove("events");bundle.Remove("verification_history");bundle["verification"]=verification.DeepClone();
        bundle["artifacts"]=new JsonArray();
        if(File.Exists(output)) ((JsonArray)bundle["artifacts"]!).Add(new JsonObject { ["path"]=output,["sha256"]=JsonData.HashFile(output),["size"]=new FileInfo(output).Length,["type"]=capability=="document.export_pdf"?"pdf":"text" });
        var provisionalId=Store.SaveVerification(id,bundle,"reverification_provisional"); var readback=await Readback(id,provisionalId,run);
        ((JsonArray)verification["criteria"]!).Add(new JsonObject { ["id"]=capability=="document.export_pdf"?"AC-10":"TC-06",["expected"]="New verification evidence committed and independently read back",["observed"]=readback.DeepClone(),["outcome"]=readback["outcome"]?.DeepClone() ?? JsonValue.Create("INCONCLUSIVE"),["evidence_reference"]=Store.Path,["verifier_id"]="hybrid.independent-sqlite-readback" });
        CheckIdentityAtGate(verification,capability,source,output,sourceHash,criteriaPath,criteriaHash);
        var rows=((JsonArray)verification["criteria"]!).OfType<JsonObject>().ToArray();
        var ids=CriterionIds(capability).Append(capability=="document.export_pdf"?"AC-10":"TC-06");
        var execution=(JsonObject)original["execution"]!;
        if(!rows.Select(c=>c["id"]?.GetValue<string>()).Order().SequenceEqual(ids.Order())) verification["outcome"]="INCONCLUSIVE";
        else if(rows.Any(c=>c["outcome"]?.GetValue<string>()=="FAIL")) verification["outcome"]="FAILED";
        else if(rows.Any(c=>c["outcome"]?.GetValue<string>()!="PASS") || verification["observed"]?.GetValue<bool>()!=true || execution["error_code"] is not null || execution["exit_code"]?.GetValue<int>()!=0) verification["outcome"]="INCONCLUSIVE";
        if(verification["error_code"] is not null) verification["outcome"]="INCONCLUSIVE";
        verification["level"]=verification["outcome"]?.GetValue<string>()=="VERIFIED"?"V4":verification["observed"]?.GetValue<bool>()==true?"V3":"V2";
        var idNew=Store.SaveVerification(id,verification,"reverification");
        return new JsonObject { ["operation_id"]=id,["verification_id"]=idNew,["original_receipt_unchanged"]=true,["verification"]=verification };
    }
    public JsonObject Reproduce(string first,string second)
    {
        if(first==second) throw new ContractException("REPRODUCTION_REQUIRES_SEPARATE_OPERATION");
        var a=Store.GetReceipt(first) ?? throw new ContractException("OPERATION_NOT_FOUND"); var b=Store.GetReceipt(second) ?? throw new ContractException("OPERATION_NOT_FOUND");
        foreach(var r in new[]{a,b})
        {
            if(r["verification"]?["outcome"]?.GetValue<string>()!="VERIFIED" || r["verification"]?["level"]?.GetValue<string>()!="V4") throw new ContractException("REPRODUCTION_REQUIRES_V4");
            var latest=(r["verification_history"] as JsonArray)?.OfType<JsonObject>().LastOrDefault(v=>v["kind"]?.GetValue<string>()=="reverification");
            if(latest is not null && latest["verification"]?["outcome"]?.GetValue<string>()!="VERIFIED") throw new ContractException("REPRODUCTION_HAS_LATER_FAILED_VERIFICATION");
            foreach(var artifact in (JsonArray)r["artifacts"]!) if(JsonData.HashFile(artifact!["path"]!.GetValue<string>())!=artifact["sha256"]!.GetValue<string>()) throw new ContractException("ARTIFACT_CHANGED");
            if(JsonData.HashFile(r["criteria_path"]!.GetValue<string>())!=r["criteria_sha256"]!.GetValue<string>() || JsonData.HashFile(r["request"]!["parameters"]!["source"]!.GetValue<string>())!=r["source_sha256"]!.GetValue<string>()) throw new ContractException("REPRODUCTION_INPUT_OR_CRITERIA_CHANGED");
        }
        foreach(var key in new[]{"capability","source_sha256","criteria_sha256","environment_key"}) if(a[key]?.ToJsonString()!=b[key]?.ToJsonString()) throw new ContractException("REPRODUCTION_CONDITIONS_MISMATCH");
        if(a["verification"]?["verifier"]?.ToJsonString()!=b["verification"]?["verifier"]?.ToJsonString()) throw new ContractException("REPRODUCTION_VERIFIER_MISMATCH");
        var evidence=new JsonObject { ["first_operation_id"]=first,["second_operation_id"]=second,["level"]="V5",["criteria_sha256"]=a["criteria_sha256"]?.DeepClone(),["environment_key"]=a["environment_key"]?.DeepClone(),["byte_identical"]=a["artifacts"]![0]!["sha256"]!.GetValue<string>()==b["artifacts"]![0]!["sha256"]!.GetValue<string>(),["original_receipts_unchanged"]=true };
        evidence["reproducibility_id"]=Store.RecordReproducibility(first,second,evidence); return evidence;
    }
}
