#Requires -Version 7.4
#Requires -Modules Az.Accounts, Az.Resources

<#
.SYNOPSIS
    Creates or updates the Azure environment of OddsCollector, including the Cosmos DB database and containers.

.DESCRIPTION
    Deploys main.bicep to a resource group (created if missing): Flex Consumption function app with a managed
    identity, Cosmos DB (serverless) with the OddsCollector database and the EventPredictions and EventResults
    containers, Service Bus queue, Key Vault with The Odds API key, storage, Application Insights, role assignments
    and, with -AlertEmail, alerts.

    The deployment is idempotent: run it again after changing the template or a parameter. With -WhatIf it shows the
    Azure Resource Manager what-if result and changes nothing; -Confirm asks before each change.

    The Odds API key is asked for on the first deployment (when Key Vault has no key yet) unless -OddsApiKey is given;
    with -Force the script never asks and fails instead. Later deployments keep the stored key; pass -OddsApiKey to
    rotate it.

.INPUTS
    None. The script does not take pipeline input.

.OUTPUTS
    OddsCollector.Deployment.Infrastructure. The deployed environment; pipe it to Publish-FunctionApp.ps1.
    - ResourceGroupName, FunctionAppName: where the code goes.
    - FunctionAppHostName: host name of the HTTP API (https://<host>/api/PredictionsHttpFunction).
    - CosmosAccountName, CosmosEndpoint: the Cosmos DB account with the OddsCollector database.
    - ServiceBusNamespace: fully qualified namespace of the upcoming-events queue.
    - KeyVaultName: the vault that holds the OddsApiKey secret.
    - ApplicationInsightsName: where logs, traces and metrics go.

    Microsoft.Azure.Commands.ResourceManager.Cmdlets.SdkModels.Deployments.PSWhatIfOperationResult. With -WhatIf, when
    the resource group exists: the changes the deployment would make.

.EXAMPLE
    ./Deploy-Infrastructure.ps1 -EnvironmentName prod -Location westeurope -League soccer_epl, soccer_spain_la_liga

.EXAMPLE
    ./Deploy-Infrastructure.ps1 -EnvironmentName prod -Location westeurope -WhatIf

.EXAMPLE
    ./Deploy-Infrastructure.ps1 -EnvironmentName dev -Location westeurope | ./Publish-FunctionApp.ps1
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
    [string[]] $League = @('soccer_epl'),

    [ValidateNotNullOrEmpty()]
    [string] $UpcomingEventsTimerInterval = '0 0 */6 * * *',

    [ValidateNotNullOrEmpty()]
    [string] $EventResultsTimerInterval = '0 0 6 * * *',

    [securestring] $OddsApiKey,

    [ValidateSet('Basic', 'Standard')]
    [string] $ServiceBusSku = 'Basic',

    [ValidateRange(40, 1000)]
    [int] $MaximumInstanceCount = 40,

    [ValidateSet(512, 2048, 4096)]
    [int] $InstanceMemoryMB = 2048,

    [ValidateScript({ $_ -ge [timespan]::Zero }, ErrorMessage = 'The time to live cannot be negative.')]
    [timespan] $PredictionTimeToLive = [timespan]::Zero,

    [mailaddress] $AlertEmail,

    [guid] $DataContributorPrincipalId,

    [switch] $GrantMeDataAccess,

    [switch] $EnablePurgeProtection,

    [switch] $Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'OddsCollector.Deployment.psm1') -Verbose:$false

$templateFile = Join-Path $PSScriptRoot 'main.bicep'
$oddsApiKeySecretName = 'OddsApiKey'
$keyVaultApiVersion = '2023-07-01'

function Get-SignedInPrincipalId {
    param($Context)

    if ($Context.Account.Type -eq 'User') {
        return [guid] (Get-AzADUser -SignedIn).Id
    }

    return [guid] (Get-AzADServicePrincipal -ApplicationId $Context.Account.Id).Id
}

function Test-StoredOddsApiKey {
    [OutputType([bool])]
    param([string] $ResourceGroupName)

    $vault = Get-AzResource -ResourceGroupName $ResourceGroupName -ResourceType 'Microsoft.KeyVault/vaults' `
        -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $vault) {
        return $false
    }

    $response = Invoke-AzRestMethod -Method GET `
        -Path "$($vault.ResourceId)/secrets/$($oddsApiKeySecretName)?api-version=$keyVaultApiVersion"

    return $response.StatusCode -eq 200
}

function Read-OddsApiKey {
    [OutputType([securestring])]
    param()

    $key = Read-Host -Prompt 'The Odds API key' -AsSecureString
    if ($key.Length -eq 0) {
        $errorRecord = New-OddsCollectorErrorRecord -Message 'The Odds API key is required on the first deployment.' `
            -ErrorId 'OddsApiKeyEmpty' -Category InvalidArgument -ExceptionType ([System.ArgumentException])
        $PSCmdlet.ThrowTerminatingError($errorRecord)
    }

    return $key
}

if (-not (Get-Command bicep -ErrorAction SilentlyContinue)) {
    $errorRecord = New-OddsCollectorErrorRecord `
        -Message 'Bicep CLI is not on PATH. Install it: https://learn.microsoft.com/azure/azure-resource-manager/bicep/install' `
        -ErrorId 'BicepNotInstalled' -Category NotInstalled -TargetObject 'bicep'
    $PSCmdlet.ThrowTerminatingError($errorRecord)
}

# Azure resource names are lowercase (SC04: validation above is case-insensitive, the names are normalized here).
$EnvironmentName = $EnvironmentName.ToLowerInvariant()
$League = @($League | ForEach-Object { $_.ToLowerInvariant() } | Select-Object -Unique)
if (-not $ResourceGroupName) {
    $ResourceGroupName = "rg-oddscollector-$EnvironmentName"
}

$connectParameters = @{}
if ($PSBoundParameters.ContainsKey('SubscriptionId')) {
    $connectParameters.SubscriptionId = $SubscriptionId
}
$context = Connect-OddsCollectorSubscription @connectParameters

$resourceGroup = Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue

if (-not $resourceGroup) {
    if (-not $PSCmdlet.ShouldProcess($ResourceGroupName, "Create resource group in $Location")) {
        if ($WhatIfPreference) {
            Write-Information -InformationAction Continue `
                -MessageData "Resource group $ResourceGroupName does not exist yet: every resource in main.bicep would be created."
        }

        return
    }

    $resourceGroup = New-AzResourceGroup -Name $ResourceGroupName -Location $Location `
        -Tag @{ application = 'OddsCollector'; environment = $EnvironmentName } -Confirm:$false
} elseif ($resourceGroup.Location -ne $Location) {
    Write-Warning "Resource group $ResourceGroupName is in $($resourceGroup.Location); the resources go to $Location."
}

if (-not $OddsApiKey -and -not $WhatIfPreference -and -not (Test-StoredOddsApiKey -ResourceGroupName $ResourceGroupName)) {
    if ($Force) {
        $errorRecord = New-OddsCollectorErrorRecord `
            -Message 'Key Vault has no Odds API key yet: pass -OddsApiKey.' -ErrorId 'OddsApiKeyMissing' `
            -Category InvalidArgument -TargetObject $ResourceGroupName -ExceptionType ([System.ArgumentException])
        $PSCmdlet.ThrowTerminatingError($errorRecord)
    }

    $OddsApiKey = Read-OddsApiKey
}

$dataContributor = ''
if ($GrantMeDataAccess) {
    $dataContributor = (Get-SignedInPrincipalId -Context $context).ToString()
} elseif ($PSBoundParameters.ContainsKey('DataContributorPrincipalId')) {
    $dataContributor = $DataContributorPrincipalId.ToString()
}

$templateParameters = @{
    environmentName              = $EnvironmentName
    location                     = $Location
    leagues                      = $League -join ';'
    upcomingEventsTimerInterval  = $UpcomingEventsTimerInterval
    eventResultsTimerInterval    = $EventResultsTimerInterval
    serviceBusSku                = $ServiceBusSku
    maximumInstanceCount         = $MaximumInstanceCount
    instanceMemoryMB             = $InstanceMemoryMB
    predictionsTimeToLiveSeconds = [int] $PredictionTimeToLive.TotalSeconds
    dataContributorPrincipalId   = $dataContributor
    alertEmail                   = $AlertEmail ? $AlertEmail.Address : ''
    enablePurgeProtection        = $EnablePurgeProtection.IsPresent
}

$deploymentName = "oddscollector-$EnvironmentName-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

if (-not $PSCmdlet.ShouldProcess($ResourceGroupName, "Deploy $(Split-Path $templateFile -Leaf) as $deploymentName")) {
    if ($WhatIfPreference) {
        Get-AzResourceGroupDeploymentWhatIfResult -Name $deploymentName -ResourceGroupName $ResourceGroupName `
            -TemplateFile $templateFile -TemplateParameterObject $templateParameters
    }

    return
}

$progress = @{ Activity = "Deploying $deploymentName"; Status = $ResourceGroupName }
Write-Progress @progress

try {
    if ($OddsApiKey) {
        $templateParameters.oddsApiKey = ConvertFrom-SecureString -SecureString $OddsApiKey -AsPlainText
    }

    $deployment = New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $ResourceGroupName `
        -TemplateFile $templateFile -TemplateParameterObject $templateParameters -Confirm:$false
} finally {
    $templateParameters.Remove('oddsApiKey')
    Write-Progress @progress -Completed
}

if ($deployment.ProvisioningState -ne 'Succeeded') {
    $errorRecord = New-OddsCollectorErrorRecord `
        -Message "Deployment $deploymentName finished with state $($deployment.ProvisioningState)." `
        -ErrorId 'DeploymentFailed' -Category InvalidResult -TargetObject $deploymentName
    $PSCmdlet.ThrowTerminatingError($errorRecord)
}

Write-Verbose "Deployment $deploymentName succeeded."

[pscustomobject]@{
    PSTypeName              = 'OddsCollector.Deployment.Infrastructure'
    ResourceGroupName       = $ResourceGroupName
    FunctionAppName         = $deployment.Outputs.functionAppName.Value
    FunctionAppHostName     = $deployment.Outputs.functionAppHostName.Value
    CosmosAccountName       = $deployment.Outputs.cosmosAccountName.Value
    CosmosEndpoint          = $deployment.Outputs.cosmosEndpoint.Value
    ServiceBusNamespace     = $deployment.Outputs.serviceBusNamespace.Value
    KeyVaultName            = $deployment.Outputs.keyVaultName.Value
    ApplicationInsightsName = $deployment.Outputs.applicationInsightsName.Value
}
