# PSScriptAnalyzer settings of the repository: static analysis and code style of every *.ps1, *.psm1 and *.psd1.
# Used by tools/Invoke-PowerShellAnalysis.ps1 (locally and in the "PowerShell code quality" job of the "Build and
# verify pull request" workflow) and, as the default settings path, by the VS Code PowerShell extension.
# Rules follow the powershell-development-guidelines skill.
@{
    Severity            = @('Error', 'Warning', 'Information')

    # Every default rule, plus the code style rules below (they are off by default).
    IncludeDefaultRules = $true

    Rules               = @{
        # Code style: one true brace style (opening brace on the same line, '} else {' on one line).
        PSPlaceOpenBrace           = @{
            Enable             = $true
            OnSameLine         = $true
            NewLineAfter       = $true
            IgnoreOneLineBlock = $true
        }

        PSPlaceCloseBrace          = @{
            Enable             = $true
            NewLineAfter       = $false
            IgnoreOneLineBlock = $true
            NoEmptyLineBefore  = $false
        }

        # Four spaces, no tabs; a continued pipeline is indented once.
        PSUseConsistentIndentation = @{
            Enable              = $true
            Kind                = 'space'
            IndentationSize     = 4
            PipelineIndentation = 'IncreaseIndentationForFirstPipeline'
        }

        PSUseConsistentWhitespace  = @{
            Enable                                  = $true
            CheckInnerBrace                         = $true
            CheckOpenBrace                          = $true
            CheckOpenParen                          = $true
            CheckOperator                           = $true
            CheckPipe                               = $true
            CheckPipeForRedundantWhitespace         = $true
            CheckSeparator                          = $true
            CheckParameter                          = $true
            IgnoreAssignmentOperatorInsideHashTable = $true
        }

        PSAlignAssignmentStatement = @{
            Enable         = $true
            CheckHashtable = $true
        }

        # Commands, parameters and keywords in their declared casing (SC04: case-insensitive, but written consistently).
        PSUseCorrectCasing         = @{
            Enable = $true
        }

        # Full command names, never aliases (they differ between platforms, for example 'ls' or 'curl').
        PSAvoidUsingCmdletAliases  = @{
            Enable = $true
        }

        # The scripts require PowerShell 7.4; flag commands and syntax that don't exist there on any platform.
        PSUseCompatibleSyntax      = @{
            Enable         = $true
            TargetVersions = @('7.4')
        }
    }
}
