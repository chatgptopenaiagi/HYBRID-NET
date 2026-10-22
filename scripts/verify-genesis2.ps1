#requires -Version 7.0
[CmdletBinding()]
param([Parameter(Mandatory)][string]$Workspace,[Parameter(Mandatory)][string]$OperationId,[string]$Executable)
. $PSScriptRoot/environment.ps1
if (-not $Executable) { $Executable=Join-Path $RepositoryRoot '.local/publish/win-x64/Hybrid.CLI.exe' }
& $Executable verify $OperationId --workspace $Workspace --python $PythonPath
exit $LASTEXITCODE
