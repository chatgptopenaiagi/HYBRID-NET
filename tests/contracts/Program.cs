using System.Text.Json.Nodes;
using Hybrid.Core;

var root=Path.GetFullPath(args.Length>0 ? args[0] : Path.Combine(AppContext.BaseDirectory,"test-state"));
Directory.CreateDirectory(root);
var results=new List<object>();
void Test(string name,Action body) {try{body();results.Add(new{name,passed=true});}catch(Exception e){results.Add(new{name,passed=false,error=e.GetType().Name,detail=e.Message});}}
void Assert(bool value){if(!value)throw new Exception("Assertion failed");}
JsonObject Request()=>new(){["protocol"]="HMCP",["version"]="0.1",["operation_id"]="test-1",["capability"]="document.export_pdf",["application"]="LibreOffice",["parameters"]=new JsonObject{["source"]=Path.Combine(root,"source.docx"),["destination"]=Path.Combine(root,"output.pdf")}};
ValidatedRequest Validate(JsonObject r)=>Contracts.Validate(r,root,Path.Combine(root,"criteria.json"));
void Rejected(JsonObject r,string code){try{Validate(r);throw new Exception("Accepted bad contract");}catch(ContractException e){Assert(e.Message==code);}}
Test("valid request normalized",()=>Assert(Validate(Request()).TimeoutSeconds==60));
Test("missing protocol",()=>{var r=Request();r.Remove("protocol");Rejected(r,"INVALID_PROTOCOL");});
Test("unsupported protocol",()=>{var r=Request();r["version"]="99";Rejected(r,"UNSUPPORTED_PROTOCOL_VERSION");});
Test("unsupported capability",()=>{var r=Request();r["capability"]="shell.execute";Rejected(r,"UNSUPPORTED_CAPABILITY");});
Test("application mismatch",()=>{var r=Request();r["application"]="Other";Rejected(r,"APPLICATION_MISMATCH");});
Test("unknown root argument",()=>{var r=Request();r["shell"]="cmd";Rejected(r,"UNSUPPORTED_FIELD");});
Test("unknown parameter",()=>{var r=Request();r["parameters"]!["overwrite"]=true;Rejected(r,"UNSUPPORTED_FIELD");});
Test("invalid operation traversal",()=>{var r=Request();r["operation_id"]="../../x";Rejected(r,"INVALID_OPERATION_ID");});
Test("operation id length bound",()=>{var r=Request();r["operation_id"]=new string('x',81);Rejected(r,"INVALID_OPERATION_ID");});
Test("operation id final newline rejected",()=>{var r=Request();r["operation_id"]="test\n";Rejected(r,"INVALID_OPERATION_ID");});
Test("source outside workspace",()=>{var r=Request();r["parameters"]!["source"]=Path.GetFullPath(Path.Combine(root,"..","outside.docx"));Rejected(r,"PATH_OUTSIDE_WORKSPACE");});
Test("destination traversal outside workspace rejected",()=>{var r=Request();r["parameters"]!["destination"]=Path.Combine(root,"..","outside.pdf");Rejected(r,"PATH_OUTSIDE_WORKSPACE");});
Test("workspace prefix lookalike rejected",()=>{var r=Request();r["parameters"]!["destination"]=Path.Combine(root+"-lookalike","output.pdf");Rejected(r,"PATH_OUTSIDE_WORKSPACE");});
Test("relative source rejected",()=>{var r=Request();r["parameters"]!["source"]="source.docx";Rejected(r,"INVALID_PATH");});
Test("UNC source rejected",()=>{var r=Request();r["parameters"]!["source"]=@"\\server\share\x.docx";Rejected(r,"INVALID_PATH");});
Test("reserved device rejected",()=>{var r=Request();r["parameters"]!["source"]=Path.Combine(root,"NUL.docx");Rejected(r,"INVALID_PATH");});
Test("alternate stream rejected",()=>{var r=Request();r["parameters"]!["source"]=Path.Combine(root,"a:stream.docx");Rejected(r,"INVALID_PATH");});
Test("wildcard destination rejected",()=>{var r=Request();r["parameters"]!["destination"]=Path.Combine(root,"out*.pdf");Rejected(r,"INVALID_PATH");});
Test("question mark destination rejected",()=>{var r=Request();r["parameters"]!["destination"]=Path.Combine(root,"out?.pdf");Rejected(r,"INVALID_PATH");});
Test("quote destination rejected",()=>{var r=Request();r["parameters"]!["destination"]=Path.Combine(root,"out\".pdf");Rejected(r,"INVALID_PATH");});
Test("angle bracket destination rejected",()=>{var r=Request();r["parameters"]!["destination"]=Path.Combine(root,"out>.pdf");Rejected(r,"INVALID_PATH");});
Test("trailing dot directory rejected",()=>{var r=Request();r["parameters"]!["destination"]=Path.Combine(root,"alias.","output.pdf");Rejected(r,"INVALID_PATH");});
Test("trailing space directory rejected",()=>{var r=Request();r["parameters"]!["destination"]=Path.Combine(root,"alias ","output.pdf");Rejected(r,"INVALID_PATH");});
Test("same input output rejected",()=>{var r=Request();r["parameters"]!["destination"]=r["parameters"]!["source"]!.DeepClone();Rejected(r,"SOURCE_DESTINATION_CONFLICT");});
Test("case insensitive source output alias rejected",()=>{var r=Request();r["parameters"]!["destination"]=Path.Combine(root,"SOURCE.DOCX");Rejected(r,"SOURCE_DESTINATION_CONFLICT");});
Test("wrong input extension",()=>{var r=Request();r["parameters"]!["source"]=Path.Combine(root,"a.exe");Rejected(r,"INVALID_INPUT_EXTENSION");});
Test("wrong output extension",()=>{var r=Request();r["parameters"]!["destination"]=Path.Combine(root,"a.exe");Rejected(r,"INVALID_OUTPUT_EXTENSION");});
Test("timeout bounds",()=>{var r=Request();r["options"]=new JsonObject{["timeout_seconds"]=301};Rejected(r,"INVALID_TIMEOUT");});
Test("timeout type",()=>{var r=Request();r["options"]=new JsonObject{["timeout_seconds"]="60"};Rejected(r,"INVALID_TIMEOUT");});
Test("null options",()=>{var r=Request();r["options"]=null;Rejected(r,"INVALID_OPTIONS");});
Test("unknown options",()=>{var r=Request();r["options"]=new JsonObject{["unbounded"]=true};Rejected(r,"UNSUPPORTED_FIELD");});
Test("criteria outside workspace",()=>{var r=Request();r["options"]=new JsonObject{["criteria_path"]=Path.GetFullPath(Path.Combine(root,"..","c.json"))};Rejected(r,"PATH_OUTSIDE_WORKSPACE");});
Test("paths with spaces",()=>{var r=Request();r["parameters"]!["source"]=Path.Combine(root,"input folder","my file.docx");Assert(Validate(r).Source.Contains("my file.docx"));});
Test("existing directory symlink rejected before output write",()=>{
    var target=Path.Combine(root,"symlink-target-"+Guid.NewGuid().ToString("N"));
    var link=Path.Combine(root,"symlink-"+Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(target);
    Directory.CreateSymbolicLink(link,target);
    var r=Request();r["parameters"]!["destination"]=Path.Combine(link,"output.pdf");
    Rejected(r,"REPARSE_PATH_REJECTED");
    Assert(!File.Exists(Path.Combine(target,"output.pdf")));
});
Test("relative runtime workspace rejected before writes",()=>{try{new HybridRuntime("relative-workspace","python.exe",root,"missing.exe");throw new Exception("Accepted relative workspace");}catch(ContractException e){Assert(e.Message=="INVALID_WORKSPACE");}});
Test("runtime workspace symlink rejected before database writes",()=>{
    var target=Path.Combine(root,"workspace-target-"+Guid.NewGuid().ToString("N"));var link=Path.Combine(root,"workspace-link-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(target);Directory.CreateSymbolicLink(link,target);
    try{new HybridRuntime(link,Path.Combine(root,"missing-python.exe"),root,Path.Combine(root,"missing-lo.exe"));throw new Exception("Accepted symlink workspace");}catch(ContractException e){Assert(e.Message=="REPARSE_WORKSPACE_REJECTED");}
    Assert(!Directory.Exists(Path.Combine(target,"engine")));
});
Test("internal evidence symlink rejected before database writes",()=>{
    var workspace=Path.Combine(root,"internal-link-"+Guid.NewGuid().ToString("N"));var target=Path.Combine(root,"internal-target-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(workspace);Directory.CreateDirectory(target);Directory.CreateSymbolicLink(Path.Combine(workspace,"engine"),target);
    try{new HybridRuntime(workspace,Path.Combine(root,"missing-python.exe"),root,Path.Combine(root,"missing-lo.exe"));throw new Exception("Accepted evidence symlink");}catch(ContractException e){Assert(e.Message=="REPARSE_PATH_REJECTED");}
    Assert(!File.Exists(Path.Combine(target,"evidence.sqlite")));
});
Test("canonical identity independent of property ordering",()=>{var r=Request();var reversed=new JsonObject(r.Reverse().Select(p=>new KeyValuePair<string,JsonNode?>(p.Key,p.Value?.DeepClone())));Assert(Validate(r).Hash==Validate(reversed).Hash);});
Test("duplicate json key rejected",()=>{try{Contracts.Parse("{\"protocol\":\"HMCP\",\"protocol\":\"Other\"}");throw new Exception();}catch(ContractException e){Assert(e.Message=="DUPLICATE_JSON_FIELD");}});
Test("malformed JSON rejected",()=>{try{Contracts.Parse("{");throw new Exception();}catch(ContractException e){Assert(e.Message=="MALFORMED_JSON");}});
Test("legal lifecycle",()=>{Lifecycle.Check("CREATED","VALIDATED");Lifecycle.Check("VERIFYING","VERIFIED");});
Test("cannot skip verification",()=>{try{Lifecycle.Check("RUNNING","VERIFIED");throw new Exception();}catch(InvalidOperationException){}});
Test("terminal state cannot silently change",()=>{try{Lifecycle.Check("VERIFIED","RUNNING");throw new Exception();}catch(InvalidOperationException){}});
Test("content fingerprints detect edits",()=>{var path=Path.Combine(root,"hash.txt");File.WriteAllText(path,"first");var hash=JsonData.HashFile(path);File.WriteAllText(path,"second");Assert(hash!=JsonData.HashFile(path));});
var report=System.Text.Json.JsonSerializer.Serialize(new {suite="contracts",count=results.Count,failed=results.Count(r=>r.GetType().GetProperty("passed")?.GetValue(r) is false),results},JsonData.Options);
File.WriteAllText(Path.Combine(root,"results.json"),report);Console.WriteLine(report);
return results.Any(r=>r.GetType().GetProperty("passed")?.GetValue(r) is false)?1:0;
