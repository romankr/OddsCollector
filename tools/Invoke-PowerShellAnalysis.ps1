#Requires -Version 7.4
#Requires -Modules @{ ModuleName = 'PSScriptAnalyzer'; ModuleVersion = '1.24.0' }

<#
.SYNOPSIS
    Checks the PowerShell code of the repository: static analysis and code style.

.DESCRIPTION
    Runs PSScriptAnalyzer with PSScriptAnalyzerSettings.psd1 on every *.ps1, *.psm1 and *.psd1 file (bin, obj and
    node_modules folders are skipped) and checks that each file is formatted as Invoke-Formatter would format it with
    the same settings. Writes one object per finding; no output means the code is clean.

    With -Fix, unformatted files are rewritten by Invoke-Formatter (the code style findings disappear; the analysis
    findings have to be fixed by hand). The "PowerShell code quality" job of the "Build and verify pull request" GitHub
    workflow runs this script.

.INPUTS
    System.String. Paths of files or folders, for example from Get-ChildItem (FullName).

.OUTPUTS
    OddsCollector.CodeAnalysis.Finding. One object per finding:
    - Path, Line, Column: where the finding is.
    - Severity: Error, Warning or Information.
    - RuleName: the PSScriptAnalyzer rule, or Formatting for code that Invoke-Formatter would change.
    - Message: what is wrong.

.EXAMPLE
    ./tools/Invoke-PowerShellAnalysis.ps1

.EXAMPLE
    ./tools/Invoke-PowerShellAnalysis.ps1 -Path ./deploy -Fix

.EXAMPLE
    Get-ChildItem ./deploy -Filter *.ps1 | ./tools/Invoke-PowerShellAnalysis.ps1 | Format-Table -AutoSize
#>
[CmdletBinding(SupportsShouldProcess)]
[OutputType('OddsCollector.CodeAnalysis.Finding')]
param(
    # Files or folders to check; wildcards are supported. Default: the whole repository.
    [Parameter(Position = 0, ValueFromPipeline, ValueFromPipelineByPropertyName)]
    [Alias('PSPath', 'FullName')]
    [SupportsWildcards()]
    [ValidateNotNullOrEmpty()]
    [string[]] $Path = @((Split-Path $PSScriptRoot -Parent)),

    # PSScriptAnalyzer settings file. Default: PSScriptAnalyzerSettings.psd1 at the repository root.
    [ValidateNotNullOrEmpty()]
    [string] $SettingsPath = (Join-Path (Split-Path $PSScriptRoot -Parent) 'PSScriptAnalyzerSettings.psd1'),

    # Rewrite the files that are not formatted according to the settings.
    [switch] $Fix
)

begin {
    Set-StrictMode -Version Latest
    $ErrorActionPreference = 'Stop'

    $extensions = @('.ps1', '.psm1', '.psd1')
    $skippedFolder = '[\\/](bin|obj|node_modules)[\\/]'

    $settings = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($SettingsPath)
    if (-not (Test-Path -LiteralPath $settings -PathType Leaf)) {
        $errorRecord = [System.Management.Automation.ErrorRecord]::new(
            [System.IO.FileNotFoundException]::new("PSScriptAnalyzer settings $settings were not found."),
            'SettingsNotFound',
            [System.Management.Automation.ErrorCategory]::ObjectNotFound,
            $settings)
        $PSCmdlet.ThrowTerminatingError($errorRecord)
    }

    function Get-PowerShellFile {
        [OutputType([System.IO.FileInfo])]
        param([string] $ResolvedPath)

        $item = Get-Item -LiteralPath $ResolvedPath

        $files = if ($item.PSIsContainer) {
            Get-ChildItem -LiteralPath $item.FullName -Recurse -File
        } else {
            $item
        }

        $files | Where-Object { $_.Extension -in $extensions -and $_.FullName -notmatch $skippedFolder }
    }

    function New-Finding {
        [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseShouldProcessForStateChangingFunctions', '',
            Justification = 'Creates an object in memory; nothing in the system changes.')]
        param([string] $FilePath, [int] $Line, [int] $Column, [string] $Severity, [string] $RuleName, [string] $Message)

        [pscustomobject]@{
            PSTypeName = 'OddsCollector.CodeAnalysis.Finding'
            Path       = $FilePath
            Line       = $Line
            Column     = $Column
            Severity   = $Severity
            RuleName   = $RuleName
            Message    = $Message
        }
    }

    function Invoke-FormattingCheck {
        [CmdletBinding(SupportsShouldProcess)]
        param([System.IO.FileInfo] $File, [switch] $Fix)

        $bytes = [System.IO.File]::ReadAllBytes($File.FullName)
        $hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
        $text = [System.IO.File]::ReadAllText($File.FullName)
        $newLine = $text.Contains("`r`n") ? "`r`n" : "`n"

        # Compare with LF only, so that the check does not depend on core.autocrlf.
        $original = $text -replace "`r`n", "`n"
        $formatted = Invoke-Formatter -ScriptDefinition $original -Settings $settings

        if ($formatted -ceq $original) {
            return
        }

        if ($Fix -and $PSCmdlet.ShouldProcess($File.FullName, 'Format with Invoke-Formatter')) {
            $encoding = [System.Text.UTF8Encoding]::new($hasBom)
            [System.IO.File]::WriteAllText($File.FullName, ($formatted -replace "`n", $newLine), $encoding)
            return
        }

        $originalLines = $original -split "`n"
        $formattedLines = $formatted -split "`n"
        $line = 0
        while ($line -lt $originalLines.Count -and $line -lt $formattedLines.Count -and
            $originalLines[$line] -ceq $formattedLines[$line]) {
            $line++
        }

        New-Finding -FilePath $File.FullName -Line ($line + 1) -Column 1 -Severity 'Warning' -RuleName 'Formatting' `
            -Message 'The file is not formatted according to PSScriptAnalyzerSettings.psd1, starting at this line. Run ./tools/Invoke-PowerShellAnalysis.ps1 -Fix.'
    }
}

process {
    foreach ($item in $Path) {
        foreach ($resolvedPath in (Resolve-Path -Path $item).ProviderPath) {
            foreach ($file in (Get-PowerShellFile -ResolvedPath $resolvedPath)) {
                Write-Verbose "Analyzing $($file.FullName)"

                # Formatting first: with -Fix, the analysis then sees the formatted file.
                Invoke-FormattingCheck -File $file -Fix:$Fix

                # Analysis only reads the file, so it also runs with -WhatIf and without -Confirm prompts.
                $records = Invoke-ScriptAnalyzer -Path $file.FullName -Settings $settings -WhatIf:$false -Confirm:$false

                foreach ($record in $records) {
                    $severity = $record.Severity -eq 'ParseError' ? 'Error' : $record.Severity.ToString()
                    New-Finding -FilePath $file.FullName -Line $record.Line -Column $record.Column `
                        -Severity $severity -RuleName $record.RuleName -Message $record.Message
                }
            }
        }
    }
}
