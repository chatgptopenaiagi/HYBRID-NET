using System.Text.Json.Nodes;
using Hybrid.Core;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var values=new Dictionary<string,string>(StringComparer.Ordinal); var positional=new List<string>();
            var allowed=new HashSet<string> {"workspace","python","verifier-dir","libreoffice","request","source","output","operation-id","criteria","timeout","cancel-after-ms"};
            for(var i=0;i<args.Length;i++)
            {
                if(!args[i].StartsWith("--",StringComparison.Ordinal)) { positional.Add(args[i]); continue; }
                var key=args[i][2..]; if(!allowed.Contains(key)||values.ContainsKey(key)||i+1>=args.Length) throw new ContractException("INVALID_CLI_OPTION"); values.Add(key,args[++i]);
            }
            var command=positional.FirstOrDefault() ?? "help";
            var arity=command switch {"inspect" or "capability" or "receipt" or "verify"=>2,"reproduce"=>3,"execute"=>values.ContainsKey("request")?1:2,_=>1};
            if(positional.Count>arity || (positional.Count<arity && args.Length>0)) throw new ContractException("INVALID_POSITIONAL_ARGUMENTS");
            var executionOptions=new[]{"request","source","output","operation-id","criteria","timeout","cancel-after-ms"};
            if(command!="execute" && values.Keys.Any(k=>executionOptions.Contains(k))) throw new ContractException("UNSUPPORTED_OPTION_FOR_COMMAND");
            if(values.ContainsKey("request") && values.Keys.Any(k=>new[]{"source","output","criteria","timeout","operation-id"}.Contains(k))) throw new ContractException("CONFLICTING_REQUEST_OPTIONS");
            string Option(string key,string fallback)=>values.GetValueOrDefault(key,fallback);
            if(command=="version") return Print(new JsonObject { ["product"]="HYBRID.NET",["version"]="0.1.0",["milestone"]="Genesis 2",["protocol"]="HMCP 0.1",["runtime"]=System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,["architecture"]=System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString() });
            if(command=="help") return Print(new JsonObject { ["commands"]=new JsonArray("version","doctor","discover","applications","capabilities","inspect LibreOffice","capability document.export_pdf","execute <capability> --source <absolute> --output <absolute>","execute --request <json>","receipt <id>","history","verify <id>","reproduce <first-id> <second-id>"),["global_options"]=new JsonArray("--workspace <absolute directory>","--python <absolute python.exe>","--verifier-dir <directory>","--libreoffice <soffice.com>"),["execution_options"]=new JsonArray("--operation-id <id>","--criteria <absolute json>","--timeout <seconds 1..300>","--cancel-after-ms <milliseconds>"),["exit_codes"]=new JsonObject { ["0"]="query succeeded or VERIFIED",["2"]="FAILED",["3"]="INCONCLUSIVE",["4"]="invalid command or infrastructure error",["130"]="CANCELLED" } });
            var workspace=Option("workspace",Environment.GetEnvironmentVariable("HYBRID_WORKSPACE") ?? Path.Combine(AppContext.BaseDirectory,"state"));
            var python=Option("python",Environment.GetEnvironmentVariable("HYBRID_PYTHON") ?? Path.Combine(AppContext.BaseDirectory,"python","python.exe"));
            var verifier=Option("verifier-dir",Path.Combine(AppContext.BaseDirectory,"verification"));
            var libreOffice=Option("libreoffice",Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"LibreOffice","program","soffice.com"));
            using var cancellation=new CancellationTokenSource();
            Console.CancelKeyPress+=(_,e)=>{e.Cancel=true;cancellation.Cancel();};
            if(values.TryGetValue("cancel-after-ms",out var delay)) { if(!int.TryParse(delay,out var ms)||ms<1||ms>300000) throw new ContractException("INVALID_CANCELLATION_DELAY"); cancellation.CancelAfter(ms); }
            var runtime=new HybridRuntime(workspace,python,verifier,libreOffice);
            string Id(int index=1)=>positional.Count>index ? positional[index] : throw new ContractException("MISSING_OPERATION_ID");
            switch(command)
            {
                case "discover": case "capabilities": case "applications": return Print(await runtime.DiscoverAsync(cancellation.Token));
                case "inspect": case "capability":
                    var registry=await runtime.DiscoverAsync(cancellation.Token);
                    return Print(registry.FirstOrDefault(n=>n?[command=="inspect"?"application":"capability"]?.GetValue<string>()==Id()) ?? throw new ContractException("CAPABILITY_NOT_FOUND"));
                case "history": return Print(runtime.Store.History());
                case "receipt": return Print(runtime.Store.GetReceipt(Id()) ?? throw new ContractException("OPERATION_NOT_FOUND"));
                case "verify": return Print(await runtime.ReverifyAsync(Id(),cancellation.Token),true);
                case "reproduce": return Print(runtime.Reproduce(Id(),Id(2)));
                case "doctor":
                    var probe=await ProcessRunner.RunAsync(python,["--version"],runtime.Workspace,TimeSpan.FromSeconds(10));
                    return Print(new JsonObject { ["workspace"]=runtime.Workspace,["database"]=runtime.Store.Path,["runtime"]=System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,["python"]=probe,["verifier_exists"]=File.Exists(Path.Combine(verifier,"verify.py")),["capabilities"]=await runtime.DiscoverAsync(cancellation.Token),["network_endpoint"]=null,["server_required"]=false });
                case "execute":
                    JsonObject request;
                    if(values.TryGetValue("request",out var requestFile))
                    { if(new FileInfo(requestFile).Length>65536) throw new ContractException("REQUEST_TOO_LARGE"); request=Contracts.Parse(File.ReadAllText(requestFile)); }
                    else
                    {
                        var capability=Id(); var options=new JsonObject();
                        if(values.TryGetValue("timeout",out var seconds)) { if(!int.TryParse(seconds,out var timeout)) throw new ContractException("INVALID_TIMEOUT"); options["timeout_seconds"]=timeout; }
                        if(values.TryGetValue("criteria",out var criteria)) options["criteria_path"]=criteria;
                        request=new JsonObject { ["protocol"]="HMCP",["version"]="0.1",["operation_id"]=Option("operation-id","op-"+Guid.NewGuid().ToString("N")),["capability"]=capability,["application"]=capability=="document.export_pdf"?"LibreOffice":"Hybrid.Text",["parameters"]=new JsonObject { ["source"]=Option("source",""),["destination"]=Option("output","") },["options"]=options };
                    }
                    return Print(await runtime.ExecuteAsync(request,cancellation.Token),true);
                default: throw new ContractException("UNKNOWN_COMMAND");
            }
        }
        catch(Exception e)
        {
            var code=e is ContractException ? e.Message : e is InvalidOperationException && e.Message=="OPERATION_ID_CONFLICT" ? e.Message : e.GetType().Name;
            Console.WriteLine(JsonData.Text(new JsonObject { ["error"]=new JsonObject { ["code"]=code,["category"]="REQUEST_OR_INFRASTRUCTURE_ERROR" },["verification"]=new JsonObject { ["outcome"]="INCONCLUSIVE",["level"]="V0" } })); return 4;
        }
    }
    private static int Print(JsonNode value,bool result=false)
    {
        Console.WriteLine(JsonData.Text(value)); if(!result)return 0;
        if(value["state"]?.GetValue<string>()=="CANCELLED")return 130;
        return value["verification"]?["outcome"]?.GetValue<string>() switch {"VERIFIED"=>0,"FAILED"=>2,_=>value["state"]?.GetValue<string>()=="FAILED"?2:3};
    }
}
