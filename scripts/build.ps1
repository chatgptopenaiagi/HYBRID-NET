#requires -Version 7.0
[CmdletBinding()]
param([string]$OutputDirectory, [switch]$FrameworkDependent)
. $PSScriptRoot/environment.ps1
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $RepositoryRoot '.local/publish/win-x64' }
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
if (-not $OutputDirectory.StartsWith($RepositoryRoot + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)) { throw 'Build output must remain inside this repository.' }
$project = Join-Path $RepositoryRoot 'client/Hybrid.CLI/Hybrid.CLI.csproj'
$arguments = @('publish',$project,'--configuration','Release','--runtime','win-x64','--self-contained',(-not $FrameworkDependent).ToString().ToLowerInvariant(),'--output',$OutputDirectory,'--verbosity','minimal')
$watch = [Diagnostics.Stopwatch]::StartNew()
Push-Location -LiteralPath $RepositoryRoot
try {
    & $DotnetPath @arguments
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    $sdkVersion = & $DotnetPath --version
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
} finally { Pop-Location }
$files = @(Get-ChildItem -LiteralPath $OutputDirectory -File -Recurse | Where-Object { $_.FullName -ne (Join-Path $OutputDirectory 'build-receipt.json') } | ForEach-Object { [ordered]@{path=$_.FullName;size=$_.Length;sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()} })
$sourceFiles = @('Directory.Build.props','global.json') + @(Get-ChildItem (Join-Path $RepositoryRoot 'core/Hybrid.Core'),(Join-Path $RepositoryRoot 'client/Hybrid.CLI'),(Join-Path $RepositoryRoot 'verification') -File | ForEach-Object { [IO.Path]::GetRelativePath($RepositoryRoot,$_.FullName).Replace('\','/') })
$sourceManifest = @($sourceFiles | Sort-Object | ForEach-Object { [ordered]@{path=$_;sha256=(Get-FileHash -LiteralPath (Join-Path $RepositoryRoot $_) -Algorithm SHA256).Hash.ToLowerInvariant()} })
$sourceBytes = [Text.Encoding]::UTF8.GetBytes(($sourceManifest | ConvertTo-Json -Compress))
$sourceFingerprint = [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($sourceBytes))
$receipt = [ordered]@{configuration='Release';rid='win-x64';architecture='x64';self_contained=(-not $FrameworkDependent);command=@($DotnetPath)+$arguments;exit_code=0;duration_ms=$watch.Elapsed.TotalMilliseconds;dotnet_sdk=$sdkVersion;repository_head=(& git -C $RepositoryRoot rev-parse HEAD);repository_dirty=(@(& git -C $RepositoryRoot status --porcelain).Count -gt 0);source_fingerprint=$sourceFingerprint;source_manifest=$sourceManifest;files=$files;external_dependencies=@('LibreOffice','Python and pinned verifier requirements');clean_machine_tested=$false}
$receipt | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'build-receipt.json') -Encoding utf8
Write-Output "Published executable: $(Join-Path $OutputDirectory 'Hybrid.CLI.exe')"
