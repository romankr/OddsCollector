#Requires -Version 7.4
#Requires -Modules Az.Accounts

# Helpers shared by the deployment scripts in this folder (RC07: shared code lives in a module, not copied).

Set-StrictMode -Version Latest

function New-OddsCollectorErrorRecord {
    <#
    .SYNOPSIS
        Creates an ErrorRecord for $PSCmdlet.ThrowTerminatingError() or $PSCmdlet.WriteError().

    .DESCRIPTION
        Builds the error in memory only; it changes nothing, so it does not support -WhatIf.

    .OUTPUTS
        System.Management.Automation.ErrorRecord. The error with the exception, id, category and target.

    .EXAMPLE
        $errorRecord = New-OddsCollectorErrorRecord -Message 'Bicep CLI is missing.' `
            -ErrorId 'BicepNotInstalled' -Category NotInstalled -TargetObject 'bicep'
        $PSCmdlet.ThrowTerminatingError($errorRecord)
    #>
    [CmdletBinding()]
    [OutputType([System.Management.Automation.ErrorRecord])]
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseShouldProcessForStateChangingFunctions', '',
        Justification = 'Creates an object in memory, like New-Object; nothing in the system changes.')]
    param(
        # What went wrong and, when possible, how to fix it.
        [Parameter(Mandatory)]
        [string] $Message,

        # Stable identifier of the error, for example BicepNotInstalled.
        [Parameter(Mandatory)]
        [string] $ErrorId,

        [Parameter(Mandatory)]
        [System.Management.Automation.ErrorCategory] $Category,

        # The object the error is about: a resource name, a path, a URI.
        [object] $TargetObject,

        # Type of the exception in the record. It needs a (string) constructor.
        [ValidateScript({ [System.Exception].IsAssignableFrom($_) })]
        [type] $ExceptionType = [System.InvalidOperationException]
    )

    $exception = $ExceptionType::new($Message)

    return [System.Management.Automation.ErrorRecord]::new($exception, $ErrorId, $Category, $TargetObject)
}

function Connect-OddsCollectorSubscription {
    <#
    .SYNOPSIS
        Returns the Az context to deploy with, signing in and switching subscription when needed.

    .OUTPUTS
        Microsoft.Azure.Commands.Profile.Models.Core.PSAzureContext. The current Az context.

    .EXAMPLE
        $context = Connect-OddsCollectorSubscription -SubscriptionId 00000000-0000-0000-0000-000000000000
    #>
    [CmdletBinding()]
    [OutputType('Microsoft.Azure.Commands.Profile.Models.Core.PSAzureContext')]
    param(
        # Subscription to use. Default: the subscription of the current Az context.
        [guid] $SubscriptionId
    )

    $context = Get-AzContext
    if (-not $context) {
        Connect-AzAccount | Out-Null
        $context = Get-AzContext
    }

    if ($PSBoundParameters.ContainsKey('SubscriptionId') -and $context.Subscription.Id -ne $SubscriptionId.ToString()) {
        $context = Set-AzContext -Subscription $SubscriptionId.ToString()
    }

    Write-Verbose "Subscription: $($context.Subscription.Name) ($($context.Subscription.Id))"

    return $context
}

Export-ModuleMember -Function New-OddsCollectorErrorRecord, Connect-OddsCollectorSubscription
