#requires -Version 7.0
[CmdletBinding()]
param([string]$ReportPath)

$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$failures = [Collections.Generic.List[string]]::new()
$checks = [ordered]@{ markdown_files = 0; local_links = 0; json_files = 0; founding_sections = 0; reviewed_files = 0; implementation_text_files = 0 }
$started = [DateTime]::UtcNow.ToString('o')

try {
    $gitRoot = & git -C $repositoryRoot rev-parse --show-toplevel 2>$null
    if ($LASTEXITCODE -ne 0 -or [IO.Path]::GetFullPath($gitRoot) -ne $repositoryRoot) {
        throw 'Documentation validation must run in the intended repository root.'
    }
    $files = @(& git -C $repositoryRoot ls-files --cached --others --exclude-standard | Sort-Object -Unique)
    if ($LASTEXITCODE -ne 0) { throw 'Cannot enumerate repository files.' }
    $required = @('README.md','ARCHITECTURE.md','PROJECT_STATUS.md','ROADMAP.md','AGENTS.md',
        'CONTRIBUTING.md','CHANGELOG.md','VERSION','docs/genesis/GENESIS-1-SCOPE.md',
        'docs/genesis/GENESIS-1-ACCEPTANCE.md','docs/genesis/GENESIS-1-REPORT.md',
        'docs/genesis/FOUNDING-TRACEABILITY.md','docs/genesis/founding-requirements.json')
    foreach ($name in $required) {
        if (-not (Test-Path -LiteralPath (Join-Path $repositoryRoot $name) -PathType Leaf)) {
            $failures.Add("Missing required file: $name")
        }
    }
    $requirements = Get-Content -LiteralPath (Join-Path $repositoryRoot 'docs/genesis/founding-requirements.json') -Raw | ConvertFrom-Json
    $ids = @($requirements.sections.id | Sort-Object -Unique)
    if ($requirements.sections.Count -ne 45 -or $ids.Count -ne 45 -or (Compare-Object $ids @(0..44))) {
        $failures.Add('Founding coverage must contain exactly the unique section IDs 0 through 44.')
    }
    foreach ($section in $requirements.sections) {
        $checks.founding_sections++
        if (-not $section.title -or -not $section.coverage -or -not $section.documents.Count) {
            $failures.Add("Incomplete coverage entry: $($section.id)")
        }
        foreach ($target in $section.documents) {
            $path = [IO.Path]::GetFullPath((Join-Path $repositoryRoot $target))
            if (-not $path.StartsWith($repositoryRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or
                -not (Test-Path -LiteralPath $path -PathType Leaf)) {
                $failures.Add("Invalid coverage target in section $($section.id): $target")
            }
        }
    }

    # These are narrow accidental-inclusion checks, not a comprehensive secret scanner.
    $secretPatterns = @('gh[pousr]_[A-Za-z0-9]{20,}', 'github_pat_[A-Za-z0-9_]{40,}',
        'sk-[A-Za-z0-9_-]{32,}', 'AKIA[A-Z0-9]{16}', '-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----')
    foreach ($name in $files) {
        $path = Join-Path $repositoryRoot $name
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            $failures.Add("Listed file is missing: $name")
            continue
        }
        $checks.reviewed_files++
        $extension = [IO.Path]::GetExtension($path)
        # Genesis 2 deliberately adds text source and project files. Build outputs,
        # fixtures, PDFs, databases, caches and machine evidence remain ignored.
        # Do not turn this into a broad binary-artifact allowlist.
        $sourceExtensions = @('.cs','.csproj','.props','.py')
        $isRequirements = $extension -eq '.txt' -and [IO.Path]::GetFileName($name) -match '^requirements(?:-[a-z0-9_-]+)?\.txt$'
        if ($extension -notin (@('.md','.json','.ps1') + $sourceExtensions) -and -not $isRequirements -and
            $name -notin @('VERSION','.gitignore','.gitattributes')) {
            $failures.Add("Unexpected tracked/source file type for Genesis 2: $name")
            continue
        }
        if ($extension -in $sourceExtensions -or $isRequirements) { $checks.implementation_text_files++ }
        $content = Get-Content -LiteralPath $path -Raw
        foreach ($pattern in $secretPatterns) {
            if ($content -match $pattern) { $failures.Add("Possible credential material: $name (matched value suppressed)") }
        }
        if ($extension -eq '.json') {
            $checks.json_files++
            try { $null = $content | ConvertFrom-Json -Depth 100 } catch { $failures.Add("Invalid JSON: $name") }
        }
        if ($extension -ne '.md') { continue }
        $checks.markdown_files++
        if ($content.Trim().Length -lt 100) { $failures.Add("Empty or insubstantial document: $name") }
        $fenceCount = [regex]::Matches($content, '(?m)^\s*```').Count
        if ($fenceCount % 2 -ne 0) { $failures.Add("Unbalanced fenced blocks: $name") }
        foreach ($match in [regex]::Matches($content, '\[[^\]\r\n]+\]\(([^)\r\n]+)\)')) {
            $target = $match.Groups[1].Value.Trim().Trim('<','>')
            if ($target -match '^(https?://|mailto:|#)') { continue }
            $target = [Uri]::UnescapeDataString(($target -split '#',2)[0])
            if (-not $target) { continue }
            $checks.local_links++
            $resolved = [IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $path) $target))
            if (-not $resolved.StartsWith($repositoryRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or
                -not (Test-Path -LiteralPath $resolved)) {
                $failures.Add("Broken or external local link in ${name}: $target")
            }
        }
    }
    if ((Get-Content -LiteralPath (Join-Path $repositoryRoot 'VERSION') -Raw).Trim() -ne '0.1.0') {
        $failures.Add('Genesis 2 version must be 0.1.0; historical Genesis 1 documents retain their original version.')
    }
} catch {
    $failures.Add("Validation infrastructure error: $($_.Exception.GetType().Name)")
}

$result = [ordered]@{
    kind = 'documentation_integrity_only'
    started_at = $started
    finished_at = [DateTime]::UtcNow.ToString('o')
    powershell_version = $PSVersionTable.PSVersion.ToString()
    passed = ($failures.Count -eq 0)
    checks = $checks
    failures = @($failures)
    limitations = @('No runtime, application, external-link, OS compatibility or exhaustive security verification.',
        'Path and topic coverage do not replace substantive architectural review.')
}
if ($ReportPath) {
    $destination = [IO.Path]::GetFullPath($ReportPath)
    if (-not $destination.StartsWith($repositoryRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Validation report must stay within this repository; use the ignored .local directory.'
    }
    if ($destination -notlike (Join-Path $repositoryRoot '.local/*')) {
        throw 'Machine-local validation reports must be written beneath .local.'
    }
    New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
    $result | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $destination -Encoding utf8
}
$result | ConvertTo-Json -Depth 10
if ($failures.Count) { exit 1 }
exit 0
