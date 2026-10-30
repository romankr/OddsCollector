#Requires -Version 7.4
#Requires -Modules Az.Accounts, Az.Resources

<#
.SYNOPSIS
    Deploys the whole OddsCollector environment to Azure: infrastructure, Cosmos DB schema and the function code.

.DESCRIPTION
    Runs Deploy-Infrastructure.ps1 and pipes its result to Publish-FunctionApp.ps1. The parameters are the same as
    those of Deploy-Infrastructure.ps1 (Get-Help ./Deploy-Infrastructure.ps1 -Full) plus -Configuration and
    -SkipPublish; only the parameters given here are passed on, so the defaults of Deploy-Infrastructure.ps1 apply.
    With -WhatIf, shows the infrastructure changes and deploys nothing.

.INPUTS
    None. The script does not take pipeline input.

.OUTPUTS
    OddsCollector.Deployment.Infrastructure. The deployed environment, as written by Deploy-Infrastructure.ps1.

    Microsoft.Azure.Commands.ResourceManager.Cmdlets.SdkModels.Deployments.PSWhatIfOperationResult. With -WhatIf.

.EXAMPLE
    ./Deploy-Environment.ps1 -EnvironmentName prod -Location westeurope -League soccer_epl, soccer_spain_la_liga `
        -AlertEmail ops@example.com

.EXAMPLE
    ./Deploy-Environment.ps1 -EnvironmentName prod -Location westeurope -OddsApiKey $key -Force
#>
[CmdletBinding(SupportsShouldProcess)]
[OutputType('OddsCollector.Deployment.Infrastructure')]
[OutputType('Microsoft.Azure.Commands.ResourceManager.Cmdlets.SdkModels.Deployments.PSWhatIfOperationResult')]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[a-z0-9]{2,8}$', ErrorMessage = '{0} is not 2-8 letters and digits.')]
    [string] $EnvironmentName,

    [Parameter(Mandatory)]
    [string] $Location,

    [string] $ResourceGroupName,

    [guid] $SubscriptionId,

    [Alias('Leagues')]
    [ValidateNotNullOrEmpty()]
    [ValidatePattern('^soccer_[a-z0-9_]+$', ErrorMessage = '{0} is not a soccer_* sport key of The Odds API.')]
    [string[]] $League,

    [ValidateNotNullOrEmpty()]
    [string] $UpcomingEventsTimerInterval,

    [ValidateNotNullOrEmpty()]
    [string] $EventResultsTimerInterval,

    [securestring] $OddsApiKey,

    [ValidateSet('Basic', 'Standard')]
    [string] $ServiceBusSku,

    [ValidateRange(40, 1000)]
    [int] $MaximumInstanceCount,

    [ValidateSet(512, 2048, 4096)]
    [int] $InstanceMemoryMB,

    [ValidateScript({ $_ -ge [timespan]::Zero }, ErrorMessage = 'The time to live cannot be negative.')]
    [timespan] $PredictionTimeToLive,

    [mailaddress] $AlertEmail,

    [guid] $DataContributorPrincipalId,

    [switch] $GrantMeDataAccess,

    [switch] $EnablePurgeProtection,

    [switch] $Force,

    [ValidateSet('Release', 'Debug')]
    [string] $Configuration = 'Release',

    [switch] $SkipPublish
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$infrastructureParameters = @{} + $PSBoundParameters
$infrastructureParameters.Remove('Configuration') | Out-Null
$infrastructureParameters.Remove('SkipPublish') | Out-Null

$infrastructure = & (Join-Path $PSScriptRoot 'Deploy-Infrastructure.ps1') @infrastructureParameters

# With -WhatIf (or a declined -Confirm) there is a what-if result or nothing, not a deployed environment.
$deployed = $infrastructure -and $infrastructure.PSObject.TypeNames -contains 'OddsCollector.Deployment.Infrastructure'

if ($deployed -and -not $SkipPublish) {
    $publishParameters = @{ Configuration = $Configuration }
    if ($PSBoundParameters.ContainsKey('SubscriptionId')) {
        $publishParameters.SubscriptionId = $SubscriptionId
    }

    $infrastructure | & (Join-Path $PSScriptRoot 'Publish-FunctionApp.ps1') @publishParameters
}

$infrastructure
