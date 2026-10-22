# Repository engineering scripts

Status: **IMPLEMENTED COMPONENT: documentation integrity tooling only**.

`validate-docs.ps1` runs with PowerShell 7 and Git. It resolves this script's parent
directory, verifies the Git root, checks required documents and founding-topic
coverage, validates local Markdown targets and fence balance, parses conceptual
JSON, and flags selected accidental-secret patterns without printing matched values.
It performs no application invocation, network call, inference or installation.

```powershell
& I:\HYBRID-NET\scripts\validate-docs.ps1
& I:\HYBRID-NET\scripts\validate-docs.ps1 -ReportPath I:\HYBRID-NET\.local\docs-validation.json
```

Exit 0 means these documentation checks passed; exit 1 means a check failed.
The optional report is machine-local evidence and should remain ignored. Review
substance and architecture separately: path coverage cannot prove conceptual
completeness or software correctness. Future build/install/run scripts belong to
later milestones and must document dependencies and actual tested outcomes.
