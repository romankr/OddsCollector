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
    # Short environment name used in resource names: 2-8 letters and digits, for example dev or prod.
    [Parameter(Mandatory)]
    [ValidatePattern('^[a-z0-9]{2,8}$', ErrorMessage = '{0} is not 2-8 letters and digits.')]
    [string] $EnvironmentName,

    # Azure region, for example westeurope. It must offer Flex Consumption with .NET 10.
    [Parameter(Mandatory)]
    [string] $Location,

    # Resource group, created if missing. Default: rg-oddscollector-<EnvironmentName>.
    [string] $ResourceGroupName,

    # Subscription to deploy to. Default: the subscription of the current Az context.
    [guid] $SubscriptionId,

    # Sport keys of The Odds API to collect; only football (soccer_*) is supported. Default: soccer_epl.
    [Alias('Leagues')]
    [ValidateNotNullOrEmpty()]
    [ValidatePattern('^soccer_[a-z0-9_]+$', ErrorMessage = '{0} is not a soccer_* sport key of The Odds API.')]
    [string[]] $League,

    # NCRONTAB schedule of UpcomingEventsFunction. Default: every 6 hours.
    [ValidateNotNullOrEmpty()]
    [string] $UpcomingEventsTimerInterval,

    # NCRONTAB schedule of EventResultsFunction. Default: daily at 06:00 UTC.
    [ValidateNotNullOrEmpty()]
    [string] $EventResultsTimerInterval,

    # The Odds API key. Default: keep the stored key, or ask for it on the first deployment.
    [securestring] $OddsApiKey,

    # Service Bus tier. Default: Basic.
    [ValidateSet('Basic', 'Standard')]
    [string] $ServiceBusSku,

    # Maximum number of Flex Consumption instances. Default: 40.
    [ValidateRange(40, 1000)]
    [int] $MaximumInstanceCount,

    # Memory of a Flex Consumption instance, in MB. Default: 2048.
    [ValidateSet(512, 2048, 4096)]
    [int] $InstanceMemoryMB,

    # How long stored predictions are kept. Default: forever.
    [ValidateScript({ $_ -ge [timespan]::Zero }, ErrorMessage = 'The time to live cannot be negative.')]
    [timespan] $PredictionTimeToLive,

    # E-mail that receives the alerts. Default: no alerts.
    [mailaddress] $AlertEmail,

    # Object id of a user, group or service principal that gets Cosmos DB data access.
    [guid] $DataContributorPrincipalId,

    # Give the signed-in user or service principal Cosmos DB data access.
    [switch] $GrantMeDataAccess,

    # Turn on Key Vault purge protection. It can't be turned off later.
    [switch] $EnablePurgeProtection,

    # Never prompt (for CI): fail when The Odds API key is needed and -OddsApiKey is missing.
    [switch] $Force,

    # Build configuration of OddsCollector.Functions.
    [ValidateSet('Release', 'Debug')]
    [string] $Configuration = 'Release',

    # Deploy only the infrastructure.
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
