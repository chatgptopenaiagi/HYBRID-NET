# Process-local engineering paths; dot-source from other entry points.
$ErrorActionPreference = 'Stop'
$script:RepositoryRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$env:DOTNET_CLI_HOME = Join-Path $RepositoryRoot '.local/dotnet-home'
$env:NUGET_PACKAGES = Join-Path $RepositoryRoot '.local/nuget/packages'
$env:NUGET_HTTP_CACHE_PATH = Join-Path $RepositoryRoot '.local/nuget/http'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_NOLOGO = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:PIP_CACHE_DIR = Join-Path $RepositoryRoot '.local/pip-cache'
$env:PYTHONDONTWRITEBYTECODE = '1'
$env:TEMP = Join-Path $RepositoryRoot '.local/tmp'
$env:TMP = $env:TEMP
foreach ($path in @($env:DOTNET_CLI_HOME,$env:NUGET_PACKAGES,$env:NUGET_HTTP_CACHE_PATH,$env:TEMP)) { New-Item -ItemType Directory -Force -Path $path | Out-Null }
$script:DotnetPath = (Get-Command dotnet -ErrorAction Stop).Source
$script:PythonPath = Join-Path $RepositoryRoot '.local/venv/Scripts/python.exe'
