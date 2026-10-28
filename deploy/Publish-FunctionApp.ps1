#Requires -Version 7.4
#Requires -Modules Az.Accounts

<#
.SYNOPSIS
    Builds OddsCollector.Functions and publishes it to a Flex Consumption function app.

.DESCRIPTION
    Runs dotnet publish once, zips the output and uploads it with the OneDeploy API (/api/publish) of each function
    app, signed in with Microsoft Entra ID (basic publishing credentials are turned off by the template). Flex
    Consumption stores the package in the deployment container and restarts the app on it; triggers are synced
    automatically.

    A function app that is not found is reported as a non-terminating error and the next one is published.

.INPUTS
    OddsCollector.Deployment.Infrastructure, or any object with ResourceGroupName and FunctionAppName properties,
    such as the output of Deploy-Infrastructure.ps1.

.OUTPUTS
    None by default.

    OddsCollector.Deployment.FunctionApp. With -PassThru, one object per published app:
    - ResourceGroupName, FunctionAppName: the app.
    - HostName: default host name of the app.
    - PredictionsUrl: URL of PredictionsHttpFunction; add ?code=<function key> to call it.

.EXAMPLE
    ./Publish-FunctionApp.ps1 -ResourceGroupName rg-oddscollector-prod -FunctionAppName func-oddscollector-prod-abc123

.EXAMPLE
    ./Deploy-Infrastructure.ps1 -EnvironmentName dev -Location westeurope | ./Publish-FunctionApp.ps1 -PassThru
#>
[CmdletBinding(SupportsShouldProcess)]
[OutputType('OddsCollector.Deployment.FunctionApp')]
param(
    # Resource group of the function app.
    [Parameter(Mandatory, ValueFromPipelineByPropertyName)]
    [ValidateNotNullOrEmpty()]
    [string] $ResourceGroupName,

    # Name of the function app.
    [Parameter(Mandatory, ValueFromPipelineByPropertyName)]
    [ValidateNotNullOrEmpty()]
    [string] $FunctionAppName,

    # Build configuration of OddsCollector.Functions.
    [ValidateSet('Release', 'Debug')]
    [string] $Configuration = 'Release',

    # Subscription of the function app. Default: the subscription of the current Az context.
    [guid] $SubscriptionId,

    # How long to wait for a deployment to finish.
    [ValidateScript({ $_ -gt [timespan]::Zero }, ErrorMessage = 'The timeout must be positive.')]
    [timespan] $Timeout = [timespan]::FromMinutes(15),

    # Write an object for each published app.
    [switch] $PassThru
)

begin {
    Set-StrictMode -Version Latest
    $ErrorActionPreference = 'Stop'

    Import-Module (Join-Path $PSScriptRoot 'OddsCollector.Deployment.psm1') -Verbose:$false

    $projectFile = Join-Path (Split-Path $PSScriptRoot -Parent) 'OddsCollector.Functions/OddsCollector.Functions.csproj'
    $webApiVersion = '2024-04-01'

    # Kudu deployment status codes: https://github.com/projectkudu/kudu/wiki/REST-API
    $deploymentFailed = 3
    $deploymentSucceeded = 4

    function Get-FunctionApp {
        param([string] $ResourceGroupName, [string] $FunctionAppName, [string] $SubscriptionId)

        $response = Invoke-AzRestMethod -Method GET -SubscriptionId $SubscriptionId `
            -ResourceGroupName $ResourceGroupName -ResourceProviderName 'Microsoft.Web' -ResourceType 'sites' `
            -Name $FunctionAppName -ApiVersion $webApiVersion

        if ($response.StatusCode -ne 200) {
            return $null
        }

        return $response.Content | ConvertFrom-Json
    }

    function Get-AuthorizationHeader {
        [OutputType([hashtable])]
        param($Context)

        # Kudu accepts Microsoft Entra ID tokens for the Azure Resource Manager audience. The plain token is only
        # kept in the header of the requests (AC06).
        $token = (Get-AzAccessToken -ResourceUrl $Context.Environment.ActiveDirectoryServiceEndpointResourceId).Token
        if ($token -is [securestring]) {
            $token = ConvertFrom-SecureString -SecureString $token -AsPlainText
        }

        return @{ Authorization = "Bearer $token" }
    }

    function Build-DeploymentPackage {
        [OutputType([string])]
        param([string] $WorkDirectory)

        $publishDirectory = Join-Path $WorkDirectory 'publish'
        $package = Join-Path $WorkDirectory 'OddsCollector.Functions.zip'

        Write-Progress -Activity 'Building OddsCollector.Functions' -Status $Configuration

        # The build log goes to the verbose stream, so that only the package path is written to the pipeline.
        dotnet publish $projectFile --configuration $Configuration --output $publishDirectory --nologo 2>&1 |
            ForEach-Object { Write-Verbose "$_" }
        if ($LASTEXITCODE -ne 0) {
            $errorRecord = New-OddsCollectorErrorRecord `
                -Message "dotnet publish failed with exit code $LASTEXITCODE; run with -Verbose to see the build log." `
                -ErrorId 'BuildFailed' -Category InvalidResult -TargetObject $projectFile
            $PSCmdlet.ThrowTerminatingError($errorRecord)
        }

        # ZipFile keeps the .azurefunctions folder, which Compress-Archive skips as hidden on Linux and macOS.
        [System.IO.Compression.ZipFile]::CreateFromDirectory($publishDirectory, $package)

        Write-Progress -Activity 'Building OddsCollector.Functions' -Completed

        return $package
    }

    function Wait-Deployment {
        param([uri] $StatusUri, [hashtable] $Headers, [string] $FunctionAppName)

        $deadline = (Get-Date) + $Timeout

        while ((Get-Date) -lt $deadline) {
            Start-Sleep -Seconds 10

            $status = Invoke-RestMethod -Method Get -Uri $StatusUri -Headers $Headers
            Write-Progress -Activity "Publishing to $FunctionAppName" -Status "$($status.status_text) $($status.progress)"
            Write-Verbose "Deployment status of ${FunctionAppName}: $($status.status_text) $($status.progress)"

            if ($status.status -eq $deploymentSucceeded) {
                return
            }

            if ($status.status -eq $deploymentFailed) {
                $errorRecord = New-OddsCollectorErrorRecord `
                    -Message "Deployment to $FunctionAppName failed: $($status.message) $($status.log_url)" `
                    -ErrorId 'DeploymentFailed' -Category InvalidResult -TargetObject $FunctionAppName
                $PSCmdlet.ThrowTerminatingError($errorRecord)
            }
        }

        $errorRecord = New-OddsCollectorErrorRecord `
            -Message "The deployment did not finish in $Timeout; check the Deployment Center of $FunctionAppName." `
            -ErrorId 'DeploymentTimedOut' -Category OperationTimeout -TargetObject $FunctionAppName `
            -ExceptionType ([System.TimeoutException])
        $PSCmdlet.ThrowTerminatingError($errorRecord)
    }

    $connectParameters = @{}
    if ($PSBoundParameters.ContainsKey('SubscriptionId')) {
        $connectParameters.SubscriptionId = $SubscriptionId
    }
    $context = Connect-OddsCollectorSubscription @connectParameters

    $workDirectory = Join-Path ([System.IO.Path]::GetTempPath()) "oddscollector-publish-$([guid]::NewGuid())"
    $package = $null
}

process {
    $site = Get-FunctionApp -ResourceGroupName $ResourceGroupName -FunctionAppName $FunctionAppName `
        -SubscriptionId $context.Subscription.Id

    if (-not $site) {
        # Only this app fails; the others in the pipeline are still published (RC06).
        $errorRecord = New-OddsCollectorErrorRecord `
            -Message "Function app $FunctionAppName was not found in resource group $ResourceGroupName." `
            -ErrorId 'FunctionAppNotFound' -Category ObjectNotFound -TargetObject $FunctionAppName `
            -ExceptionType ([System.Management.Automation.ItemNotFoundException])
        $PSCmdlet.WriteError($errorRecord)
        return
    }

    if (-not $PSCmdlet.ShouldProcess($FunctionAppName, "Publish OddsCollector.Functions ($Configuration)")) {
        return
    }

    if (-not $package) {
        New-Item -ItemType Directory -Path $workDirectory | Out-Null
        $package = Build-DeploymentPackage -WorkDirectory $workDirectory
    }

    $scmHostName = $site.properties.hostNameSslStates |
        Where-Object { $_.hostType -eq 'Repository' } |
        Select-Object -First 1 -ExpandProperty name
    $headers = Get-AuthorizationHeader -Context $context

    Write-Progress -Activity "Publishing to $FunctionAppName" -Status 'Uploading the package'

    $response = Invoke-WebRequest -Method Post -Uri "https://$scmHostName/api/publish?RemoteBuild=false" `
        -InFile $package -ContentType 'application/zip' -Headers $headers -TimeoutSec $Timeout.TotalSeconds

    # OneDeploy answers 202 with the status URL when it finishes the deployment in the background.
    $location = $response.Headers['Location'] | Select-Object -First 1
    if ($response.StatusCode -eq 202 -and $location) {
        Wait-Deployment -StatusUri $location -Headers $headers -FunctionAppName $FunctionAppName
    }

    Write-Progress -Activity "Publishing to $FunctionAppName" -Completed

    $predictionsUrl = "https://$($site.properties.defaultHostName)/api/PredictionsHttpFunction"

    Write-Verbose "Published $FunctionAppName. Function key of ${predictionsUrl}: (Invoke-AzResourceAction -ResourceId '$($site.id)/functions/PredictionsHttpFunction' -Action listKeys -ApiVersion $webApiVersion -Force).default"

    if ($PassThru) {
        [pscustomobject]@{
            PSTypeName        = 'OddsCollector.Deployment.FunctionApp'
            ResourceGroupName = $ResourceGroupName
            FunctionAppName   = $FunctionAppName
            HostName          = $site.properties.defaultHostName
            PredictionsUrl    = [uri] $predictionsUrl
        }
    }
}

clean {
    # Runs after end, Ctrl+C or a terminating error (AC03, AC04).
    # The variable is missing when begin failed before creating it (and Set-StrictMode rejects unset variables).
    $directory = Get-Variable -Name workDirectory -ValueOnly -ErrorAction Ignore
    if ($directory -and (Test-Path -LiteralPath $directory)) {
        Remove-Item -LiteralPath $directory -Recurse -Force -ErrorAction SilentlyContinue
    }
}
