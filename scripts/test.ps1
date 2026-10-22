#requires -Version 7.0
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'environment.ps1')
$PSNativeCommandUseErrorActionPreference = $false
$started = [DateTime]::UtcNow
$runId = $started.ToString('yyyyMMddTHHmmssfffZ') + '-' + [Guid]::NewGuid().ToString('N').Substring(0,8)
$runDirectory = Join-Path $RepositoryRoot ".local/test-runs/$runId"
New-Item -ItemType Directory -Force -Path $runDirectory | Out-Null
$suites = [Collections.Generic.List[object]]::new()

function Invoke-TestSuite {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Executable,
        [Parameter(Mandatory)][string[]]$Arguments,
        [Parameter(Mandatory)][string]$ResultsDirectory
    )
    New-Item -ItemType Directory -Force -Path $ResultsDirectory | Out-Null
    $stdoutPath = Join-Path $runDirectory "$Name.stdout.log"
    $stderrPath = Join-Path $runDirectory "$Name.stderr.log"
    $watch = [Diagnostics.Stopwatch]::StartNew()
    $exitCode = $null
    $errorCode = $null
    $reportPath = $null
    $total = $null
    $passed = $null
    $failed = $null
    $skipped = 0
    Write-Host "Running $Name tests..."
    try {
        if (-not (Test-Path -LiteralPath $Executable -PathType Leaf)) { throw "TEST_EXECUTABLE_UNAVAILABLE: $Executable" }
        & $Executable @Arguments 1> $stdoutPath 2> $stderrPath
        $exitCode = $LASTEXITCODE
        $reports = @(Get-ChildItem -LiteralPath $ResultsDirectory -Filter results.json -File -Recurse)
        if ($reports.Count -ne 1) { throw 'EXPECTED_ONE_MACHINE_READABLE_TEST_REPORT' }
        $reportPath = $reports[0].FullName
        $report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
        if ($null -ne $report.total) { $total = [int]$report.total }
        elseif ($null -ne $report.count) { $total = [int]$report.count }
        else { throw 'TEST_COUNT_MISSING_FROM_REPORT' }
        if ($null -eq $report.failed) { throw 'TEST_FAILURE_COUNT_MISSING_FROM_REPORT' }
        $failed = [int]$report.failed
        if ($null -ne $report.skipped) { $skipped = [int]$report.skipped }
        if ($null -ne $report.passed) { $passed = [int]$report.passed }
        else { $passed = $total - $failed - $skipped }
        if ($total -lt 1 -or $failed -lt 0 -or $passed -lt 0 -or $passed -gt $total) { throw 'INVALID_TEST_COUNTS' }
    } catch {
        $errorCode = $_.Exception.Message
    }
    $suites.Add([ordered]@{
        suite = $Name
        command = @($Executable) + $Arguments
        working_directory = $RepositoryRoot
        exit_code = $exitCode
        passed = ($null -eq $errorCode -and $exitCode -eq 0 -and $failed -eq 0)
        tests = $total
        passed_count = $passed
        failed_count = $failed
        skipped_count = $skipped
        duration_seconds = $watch.Elapsed.TotalSeconds
        report_path = $reportPath
        stdout_path = $stdoutPath
        stderr_path = $stderrPath
        infrastructure_error = $errorCode
    })
}

Push-Location -LiteralPath $RepositoryRoot
try {
    $contracts = Join-Path $runDirectory 'contracts'
    Invoke-TestSuite -Name 'contracts' -Executable $DotnetPath -ResultsDirectory $contracts -Arguments @(
        'run','--project',(Join-Path $RepositoryRoot 'tests/contracts/Hybrid.Contracts.Tests.csproj'),'-c','Release','--',$contracts)
    $store = Join-Path $runDirectory 'store'
    Invoke-TestSuite -Name 'store' -Executable $DotnetPath -ResultsDirectory $store -Arguments @(
        'run','--project',(Join-Path $RepositoryRoot 'tests/store/Hybrid.Store.Tests.csproj'),'-c','Release','--','--root',$store)
    $process = Join-Path $runDirectory 'process'
    Invoke-TestSuite -Name 'process' -Executable $DotnetPath -ResultsDirectory $process -Arguments @(
        'run','--project',(Join-Path $RepositoryRoot 'tests/process/Hybrid.ProcessTests.csproj'),'-c','Release','--',$process)
    $python = Join-Path $runDirectory 'python'
    Invoke-TestSuite -Name 'python' -Executable $PythonPath -ResultsDirectory $python -Arguments @(
        (Join-Path $RepositoryRoot 'tests/python/run_tests.py'),'--report',(Join-Path $python 'results.json'))
} finally {
    Pop-Location
}

$summary = [ordered]@{
    schema_version = '1.0'
    kind = 'automated_regression_tests'
    started_at = $started.ToString('o')
    finished_at = [DateTime]::UtcNow.ToString('o')
    powershell_version = $PSVersionTable.PSVersion.ToString()
    repository_root = $RepositoryRoot
    passed = (@($suites | Where-Object { -not $_.passed }).Count -eq 0 -and $suites.Count -eq 4)
    suite_count = $suites.Count
    tests_reported = ($suites | ForEach-Object { $_.tests } | Measure-Object -Sum).Sum
    passed_tests_reported = ($suites | ForEach-Object { $_.passed_count } | Measure-Object -Sum).Sum
    failed_tests_reported = ($suites | ForEach-Object { $_.failed_count } | Measure-Object -Sum).Sum
    suites_without_counts = @($suites | Where-Object { $null -eq $_.tests } | ForEach-Object { $_.suite })
    real_libreoffice_acceptance = 'NOT_RUN_BY_THIS_SCRIPT'
    packaged_executable_acceptance = 'NOT_RUN_BY_THIS_SCRIPT'
    suites = @($suites.ToArray())
    limitations = @('Synthetic PDFs and owned child processes are not real LibreOffice acceptance evidence.',
        'Use the separate Genesis 2 demonstration and verification scripts for packaged real-application acceptance.')
}
$summaryPath = Join-Path $runDirectory 'summary.json'
$summary | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $summaryPath -Encoding utf8
$summary | ConvertTo-Json -Depth 12
Write-Host "Test evidence: $summaryPath"
if (-not $summary.passed) { exit 1 }
exit 0
