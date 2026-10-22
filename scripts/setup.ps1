#requires -Version 7.0
[CmdletBinding()]
param([string]$Python = 'C:\Python314\python.exe')
. $PSScriptRoot/environment.ps1
if (-not (Test-Path -LiteralPath $PythonPath)) {
    if (-not (Test-Path -LiteralPath $Python)) { throw 'Supply an installed compatible 64-bit Python with -Python.' }
    & $Python -c 'import struct,sys; sys.exit(0 if struct.calcsize("P")==8 and sys.version_info >= (3,11) else 1)'
    if ($LASTEXITCODE -ne 0) { throw '64-bit Python 3.11 or newer is required; verified host is 3.14.7.' }
    & $Python -m venv (Join-Path $RepositoryRoot '.local/venv')
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
& $PythonPath -m pip install --requirement (Join-Path $RepositoryRoot 'verification/requirements.txt')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Push-Location -LiteralPath $RepositoryRoot
try {
    & $DotnetPath restore (Join-Path $RepositoryRoot 'client/Hybrid.CLI/Hybrid.CLI.csproj') --runtime win-x64
    $restoreExitCode = $LASTEXITCODE
} finally { Pop-Location }
exit $restoreExitCode
