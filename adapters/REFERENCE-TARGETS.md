# Future application adapter targets

Status: **PROPOSED ARCHITECTURE**. Every target below is **UNIMPLEMENTED / NOT
VERIFIED in HYBRID.NET**. Research checked primary documentation on 2026-10-22;
it did not inspect installed applications or execute their interfaces. Candidate
integration levels describe a possible route, not a certification of the product.
Actual scope depends on installed version, permissions, interface coverage and
adapter tests. See [capability-engine design](../docs/architecture/CAPABILITY-ENGINE.md).

| Target | Candidate route | Proposed bounded first operation | Required evidence and limit |
|---|---|---|---|
| LibreOffice | L3 UNO, possibly L1 conversion CLI | Export a disposable document to PDF | Record document/filter versions and validate text/page content; successful saving alone is insufficient. UNO's documented `XStorable.storeToURL` is a supported storage interface, not a promise that every application action is accessible. [UNO reference](https://api.libreoffice.org/docs/idl/ref/interfacecom_1_1sun_1_1star_1_1frame_1_1XStorable.html) |
| Blender | L2 Python scripting | Render a small synthetic scene to a new artifact | Record scene hash, application version, operation context and output dimensions; do not assume arbitrary Python environments can control an already running Blender session. [Blender Python/module discussion](https://docs.blender.org/api/main/info_advanced_blender_as_bpy.html) |
| Photoshop | L2 UXP scripting; selected API-backed operations | Export a synthetic image under fixed settings | Verify pixels/dimensions and supported API version; UXP DOM and action interfaces expose specific operations. A `Photoshop.AI.exe` adapter cannot invent unavailable internals. [Adobe batchPlay API](https://developer.adobe.com/photoshop/uxp/2022/ps-reference/media/batchplay/) |
| Dreamweaver | L2 scripting or L3 documented extension APIs | Inspect or transform a disposable document through a supported extension | First confirm the installed release's extension support and document model. Adobe describes JavaScript/C++ extensions and CEP; these do not imply an external headless service. [Adobe developer overview](https://developer.adobe.com/dreamweaver/) |
| CCleaner | L1 selected documented CLI | Evaluate whether a narrowly scoped, reversible fixture operation is feasible | Defer destructive operations and do not run cleanup during Genesis. The published CLI has limited coverage and does not automate registry cleaning. Confirm version-specific behavior before designing any capability. [Vendor CLI documentation](https://support.ccleaner.com/articles/en_US/Master_Article/command-line-parameters-for-ccleaner-for-windows) |
| Joomla | L3 authorized HTTP application API | Read a synthetic article from a disposable site | Confirm route, site version, enabled API and least-privilege identity. A web application need not be a Windows executable. Publication requires separate authorization. [Joomla web services](https://manual.joomla.org/docs/general-concepts/webservices/) |
| Poker-Odds-Knowledge | Potential L4 native interface | Evaluate an explicit synthetic game state | Inspect actual source/API first; compare known deterministic results and enforce public/private state boundaries. No present native interface is claimed. [POK concept](../docs/integrations/POK.md) |
| PowerShell | L1 explicit process or L2 owned scripts | Run an allowlisted deterministic data transformation | Pin executable/script identity, pass arguments explicitly and validate typed output. An unrestricted shell is not a single safe capability. [PowerShell CLI documentation](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_pwsh?view=powershell-7.5) |
| Windows services | L3 documented Service Control Manager APIs | Read the state of a designated synthetic service | Apply service-specific access checks. Starting/stopping a service is a separate mutation requiring scope; no shared services are changed in Genesis. [SCM interface](https://learn.microsoft.com/en-us/windows/win32/services/service-control-manager) |
| Other Windows applications | L1-L3 when documented; explicit L0 fallback; L4-L5 only by native design | One useful operation with independently observable output | Research each product's supported interface. Inventory or an `.exe` path is insufficient. If no direct interface exists, label the limitation rather than promise automatic internal access. |

The owner's addendum selects LibreOffice DOCX-to-PDF as the preferred real
[Genesis 2 experiment](../docs/genesis/GENESIS-2-EXPERIMENT.md). A supported UNO or
headless CLI route must be resolved against the actual installation, followed by
independent PDF verification and local SQLite evidence. This is a planned proof,
not a completed adapter evaluation. Begin with an isolated profile and disposable
input/output documents when implementation is authorized; do not alter a user's
open documents or install the application now. Genesis 5 broadens adapter lifecycle
and operation coverage beyond this initial narrow proof.

Each future adapter proposal must name the supported application versions,
integration level, allowed operations, state observations, effect scope, timeouts,
cancellation limits, verifier and recovery behavior. Registration requires evidence
that the particular capability works, not just that the application launches.
