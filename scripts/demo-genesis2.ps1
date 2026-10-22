#requires -Version 7.0
[CmdletBinding()]
param([string]$RunDirectory, [ValidateSet('all','benchmark','failures')][string]$Phase='all', [string]$LibreOffice='C:\Program Files\LibreOffice\program\soffice.com')
. $PSScriptRoot/environment.ps1
if (-not $RunDirectory) { $RunDirectory=Join-Path $RepositoryRoot ('.local/experiments/'+[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')) }
$resolved=[IO.Path]::GetFullPath($RunDirectory)
if (-not $resolved.StartsWith($RepositoryRoot+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)) { throw 'Experiments must stay in this repository.' }
$cli=Join-Path $RepositoryRoot '.local/publish/win-x64/Hybrid.CLI.exe'
if (-not (Test-Path -LiteralPath $cli)) { throw 'Run scripts/build.ps1 first.' }
& $PythonPath (Join-Path $RepositoryRoot 'tests/acceptance/run_experiments.py') --cli $cli --python $PythonPath --root $resolved --libreoffice $LibreOffice --phase $Phase
exit $LASTEXITCODE
