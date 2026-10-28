---
name: powershell-development-guidelines
description: PowerShell rules for OddsCollector, adapted from Microsoft's Cmdlet Development Guidelines (required, strongly encouraged, advisory) to advanced scripts and functions. Use whenever you write, change, or review PowerShell code (*.ps1, *.psm1) in OddsCollector, such as the deployment scripts in deploy/.
---

# PowerShell development guidelines

Based on Microsoft Learn, "Cmdlet Development Guidelines"
(https://learn.microsoft.com/powershell/scripting/developer/cmdlet/cmdlet-development-guidelines) and its three
sub-pages: Required (RD/RC), Strongly Encouraged (SD/SC) and Advisory (AD/AC), plus "Standard Cmdlet Parameter Names
and Types". Rule ids below match the Microsoft pages, so a review comment can cite them.

**Scope.** The guidelines are written for compiled C# cmdlets. OddsCollector has PowerShell 7 *advanced scripts*
(`[CmdletBinding()]` scripts in `deploy/`) and may get advanced functions and modules. An advanced script or function
is a cmdlet to its caller (same parameter binding, common parameters, `-WhatIf`, pipeline), so the design rules apply
as written; the code rules are translated to their script equivalents in the table below.

**Precedence:** this skill → PSScriptAnalyzer default rules → personal habit. Secrets and Azure specifics also follow
`azure-functions-well-architected`.

## Repo adaptations (cmdlet concept → script equivalent)

| Guideline | In OddsCollector scripts |
|---|---|
| RC01/RC02 derive from `PSCmdlet`, `Cmdlet` attribute | `[CmdletBinding()]` on every script and non-trivial function (`SupportsShouldProcess`, `DefaultParameterSetName`, `ConfirmImpact` as needed). |
| RC03 override `BeginProcessing`/`ProcessRecord`/`EndProcessing` | `begin`/`process`/`end` blocks. Code that handles pipeline input goes in `process`. A script without pipeline input may use a plain body (it runs as `end`; it is the first command in the pipeline anyway). |
| RC04 `OutputType` attribute | `[OutputType(...)]` on the script/function; for custom objects, a `PSTypeName` and `[OutputType('<that type name>')]`. |
| RC06 `ThrowTerminatingError` / `WriteError` with `ErrorRecord` | `$PSCmdlet.ThrowTerminatingError($errorRecord)` / `$PSCmdlet.WriteError($errorRecord)` (or `Write-Error -Exception ... -ErrorId ... -Category ...`). Not `throw 'text'`. |
| RC07 deploy in a module | Entry-point scripts (`Deploy-*.ps1`) are run by path. Helpers used by more than one script live in a script module (`*.psm1`) next to them and are imported with `Import-Module`; no copy-pasted helpers. |
| SD05 `Help.xml` | Comment-based help: `.SYNOPSIS`, `.DESCRIPTION`, `.INPUTS`, `.OUTPUTS`, `.EXAMPLE`; each parameter described by a comment directly above it in `param()`. `Get-Help <script> -Full` must show all of it. |
| SC01 parameters are public properties with `[Parameter]` | Parameters in `param()` with `[Parameter()]` where binding options are needed. |
| SC03 `WriteObject` per record | Emit objects as soon as they exist (`$item` or `Write-Output`); never collect into an array to return at the end. |
| AC01 `<Verb><Noun>Command` class naming | Script and function names are `Verb-Noun` (`Deploy-Infrastructure.ps1`); module = `<Product>.<Area>.psm1`. |
| AC03 `StopProcessing`, AC04 `IDisposable` | Ctrl+C or a terminating error skip the rest of the script: release temp files, connections and other resources in `try { } finally { }` or a `clean { }` block (PowerShell 7.3+). |
| Script hygiene (repo) | `#Requires -Version 7.2` and `#Requires -Modules` for every module used; `Set-StrictMode -Version Latest`; `$ErrorActionPreference = 'Stop'`; check `$LASTEXITCODE` after every native command (`dotnet`, `bicep`). |

## Required (must follow)

- **RD01 Approved verbs only.** Script, function and module-exported names use a verb from `Get-Verb`
  (`Deploy`, `Publish`, `Get`, `New`, `Set`, `Test`, `Wait`, `Connect`, `Read`...). Never synonyms such as
  `Create`, `Make`, `Fetch`, `Check`, `Execute`. The noun says what is acted on, the verb what is done.
- **RD02 No special characters in names.** None of ``# , ( ) { } [ ] & - / \ $ ^ ; : " ' < > | ? @ ` * % + = ~`` in the
  verb or noun; the single hyphen only separates verb and noun.
- **RD03 Never declare common parameter names.** `Confirm`, `Debug`, `ErrorAction`, `ErrorVariable`, `OutBuffer`,
  `OutVariable`, `WarningAction`, `WarningVariable`, `WhatIf`, `UseTransaction`, `Verbose` (and `InformationAction`,
  `InformationVariable`, `PipelineVariable`, `ProgressAction`) come from `[CmdletBinding()]`.
- **RD04 Confirmation for changes.** Anything that changes a system (Azure resources, files, deployments) declares
  `SupportsShouldProcess` and wraps *each* change in `if ($PSCmdlet.ShouldProcess($target, $action)) { ... }`, so
  `-WhatIf` and `-Confirm` work. Declaring `SupportsShouldProcess` without calling `ShouldProcess` is a bug.
  `ShouldContinue` is only for dangerous operations, only after `ShouldProcess`, and requires a `-Force` that skips both.
  Don't branch on `$WhatIfPreference` instead of calling `ShouldProcess`; reading it is fine to *show* a preview
  (such as an ARM what-if) when `ShouldProcess` returned `$false`.
- **RD05 `-Force` overrides interactive prompts.** A script that may prompt (`Read-Host`, `$Host.UI.PromptFor*`,
  credential prompts) has a `-Force` switch that never prompts: it uses defaults or fails with a clear error, so it
  runs in CI and non-interactive hosts.
- **RD06 Document the output.** `.OUTPUTS` in the help names the output type and what each member is for;
  "None" when nothing is written.
- **RC04 Declare `[OutputType()]`** matching what is actually written to the pipeline.
- **RC05 Don't keep references to output objects** after writing them; build a new object per output.
- **RC06 Report errors with `ErrorRecord`s.**
  - Terminating (nothing more can be done): `$PSCmdlet.ThrowTerminatingError()` with an `ErrorRecord` that has a
    specific exception (`InvalidOperationException`, `ArgumentException`, `ItemNotFoundException`...), a stable
    `ErrorId` and the right `ErrorCategory` (`ObjectNotFound`, `InvalidArgument`, `InvalidOperation`,
    `NotInstalled`, `ConnectionError`, `OperationTimeout`...), plus the target object.
  - Non-terminating (only the current item failed): `$PSCmdlet.WriteError()` and continue with the next item.
  - Don't let exceptions escape from `finally`/`clean` blocks, background jobs or threads.
- **RC07** see the adaptation table: shared code in a module.

## Strongly encouraged

- **SD01 Specific, singular nouns.** `Publish-FunctionApp`, not `Publish-Functions`; prefix generic nouns with the
  product or area for exported functions (`Connect-OddsCollectorSubscription`, not `Connect-Subscription`).
- **SD02 Pascal case** for verbs and every word of the noun (`Deploy-Infrastructure`), and for parameters (SD03).
- **SD03 Parameters.**
  - Standard names first (see below); a more specific name becomes an `[Alias()]`.
  - Singular names, even for arrays (`-League soccer_epl, soccer_italy_serie_a`), unless the value is *always*
    several items. Accept arrays where several values make sense instead of delimiter-separated strings.
  - Closed sets: `[ValidateSet()]` or an enum. Free-form text only for really free-form values.
  - Strongly typed .NET types: `[uri]`, `[guid]`, `[mailaddress]`, `[timespan]`, `[int]` with `[ValidateRange()]`,
    `[securestring]` — not `[string]` with ad hoc parsing. Use `[ValidatePattern()]` with an `ErrorMessage` for strings
    that must follow a format.
  - True/false: `[switch]`, never `[bool]` (use `[Nullable[bool]]` only for "leave unchanged" semantics).
  - Same parameter, same type and validation in every script that has it (for example the forwarding parameters of
    `Deploy-Environment.ps1` and `Deploy-Infrastructure.ps1`).
  - `-PassThru` on commands that are sinks by default (`Publish-*`, `Set-*`, `Add-*`, `New-*`) to return the result.
  - Parameter sets for alternative ways to identify the target; one `ValueFromPipeline` parameter per set; the default
    set holds the parameters most used interactively.
- **SD04 Feedback through the right stream.**
  - `Write-Verbose` for what the script is doing, `Write-Debug` for diagnostics, `Write-Warning` before something that
    may have an unintended result, `Write-Progress` for long operations (deployments, uploads, polling).
  - Results go to the pipeline as objects, not as text. `Write-Host` only for interactive presentation that nobody
    needs to capture; never for data. Never `[Console]::...`.
  - Prompt through the host (`Read-Host`, `$Host.UI`), never through `System.Console`.
- **SD05 Help** — see the adaptation table; every example in `.EXAMPLE` must run as written.
- **SC01 Paths and wildcards.** File or data-source input: `-Path` (`[string[]]`, wildcards, alias `PSPath`) and/or
  `-LiteralPath`; resolve with `Resolve-Path`/`$PSCmdlet.GetUnresolvedProviderPathFromPSPath()` before passing to
  .NET or native tools. Support wildcards where a string selects items from a set.
- **SC02 Pipeline input.** Design for the middle of a pipeline: parameters that identify the target accept
  `ValueFromPipelineByPropertyName` (and `ValueFromPipeline` for the input object), so the output of one script feeds
  the next (`Deploy-Infrastructure.ps1 | Publish-FunctionApp.ps1`). Handle each input in `process {}`.
- **SC03 One record at a time** — see the adaptation table.
- **SC04 Case-insensitive, case-preserving.** Compare and validate case-insensitively (`-eq`, `-match`,
  `[ValidatePattern()]`/`[ValidateSet()]` defaults); normalize case only where the target requires it (Azure resource
  names are lowercase) and keep the user's casing everywhere else.

## Advisory

- **AD01 `-InputObject`** is the name of a parameter that takes whole objects from the pipeline (`ValueFromPipeline`).
- **AD02 `-Force`** overrides a protection the script applies (overwrite, skip a safety check) — never permissions.
- **AD03 Credentials** as `[pscredential]` with `[System.Management.Automation.Credential()]`, never user/password strings.
- **AD04 `-Encoding`** on commands that read or write text files.
- **AD05 `Test-*` returns `[bool]`** (one value per input), usable in `if`.
- **AC01** naming — see the adaptation table.
- **AC02** no pipeline input → the work can run once (plain body or `begin`).
- **AC03/AC04** cleanup on stop — see the adaptation table.
- **AC05 Serialization-friendly parameter types** (primitives, `[guid]`, `[uri]`, `[version]`, `[timespan]`,
  `[datetime]`, `[securestring]`, `[pscredential]`, `[mailaddress]`, arrays/hashtables of these) so scripts work
  with remoting and `Start-Job`.
- **AC06 Secrets as `[securestring]`** in parameters and output. Convert to plain text only at the last moment, for
  the one call that needs it, keep the plain value in as small a scope as possible and never write it to the
  pipeline, logs, verbose output or files.

## Standard parameter names (pick these before inventing one)

From "Standard Cmdlet Parameter Names and Types". Most useful here:

| Name | Type | Meaning |
|---|---|---|
| `Force` | switch | Act despite restrictions or prompts (AD02, RD05). |
| `PassThru` | switch | Return the processed object from a sink command. |
| `InputObject` | object | Whole objects from the pipeline (`ValueFromPipeline`). |
| `Name` | string[] | Name of the target; specific names as aliases. |
| `Path` / `LiteralPath` | string[] | Paths with / without wildcards. |
| `Include` / `Exclude` / `Filter` | string[] | Select items. |
| `Recurse`, `Append`, `NoClobber`, `Overwrite`, `Wait`, `Quiet`, `Retry` | switch / int | As named. |
| `Timeout` | Microsoft: Int32 milliseconds | Repo: `[timespan]` (strongly typed, SD03), named `Timeout`. |
| `Credential` | pscredential | AD03. |
| `Encoding` | Encoding | AD04. |
| `Count`, `All`, `Scope` | int64 / switch / keyword | Quantity parameters. |

## Review checklist

- [ ] `Verb-Noun` names with approved verbs, specific singular nouns, Pascal case (RD01, RD02, SD01, SD02)
- [ ] `[CmdletBinding()]`; no common parameter names declared (RC02, RD03)
- [ ] Every change wrapped in `$PSCmdlet.ShouldProcess()`; `-WhatIf` changes nothing (RD04)
- [ ] Any prompt can be skipped with `-Force` (RD05)
- [ ] `[OutputType()]` and `.OUTPUTS` match the real output; results are objects, not text (RD06, RC04, SD04)
- [ ] Errors are `ErrorRecord`s with exception, id and category; no `throw 'text'` (RC06)
- [ ] Shared helpers in a module, not copied (RC07)
- [ ] Standard, singular, Pascal-case parameter names; strongly typed; `[switch]` not `[bool]`; arrays where useful;
      same type everywhere (SD03)
- [ ] `-PassThru` on sink commands; pipeline input by property name for target-identifying parameters (SD03, SC02)
- [ ] Verbose/warning/progress streams used; `Write-Host` never carries data; no `System.Console` (SD04)
- [ ] Comment-based help complete, every parameter described, examples run (SD05)
- [ ] Case-insensitive validation; case normalized only where required (SC04)
- [ ] Secrets stay `[securestring]` until the single call that needs them (AC06)
- [ ] Cleanup in `finally`/`clean` (AC03, AC04); `$LASTEXITCODE` checked after native commands
- [ ] Parsed and run with `-WhatIf` (and PSScriptAnalyzer when available) before committing
