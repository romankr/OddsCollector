// Flex Consumption plan and the function app (.NET 10 isolated worker, Linux) with a system-assigned identity.
// The template owns all application settings: a redeployment replaces settings changed in the portal.

param planName string
param appName string
param location string
param tags object
param maximumInstanceCount int
param instanceMemoryMB int
param storageAccountName string
param storageBlobEndpoint string
param deploymentContainerName string

@secure()
param applicationInsightsConnectionString string

@description('Settings of the app itself (Linux names: __ instead of :).')
param appSettings object

resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: planName
  location: location
  tags: tags
  kind: 'functionapp'
  sku: {
    name: 'FC1'
    tier: 'FlexConsumption'
  }
  properties: {
    reserved: true
  }
}

// Host settings. Flex Consumption sets the runtime in functionAppConfig and rejects
// FUNCTIONS_WORKER_RUNTIME and FUNCTIONS_EXTENSION_VERSION.
var hostSettings = {
  AzureWebJobsStorage__accountName: storageAccountName
  APPLICATIONINSIGHTS_CONNECTION_STRING: applicationInsightsConnectionString
}

resource app 'Microsoft.Web/sites@2024-04-01' = {
  name: appName
  location: location
  tags: tags
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    keyVaultReferenceIdentity: 'SystemAssigned'
    siteConfig: {
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      appSettings: [for setting in items(union(hostSettings, appSettings)): {
        name: setting.key
        value: setting.value
      }]
    }
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${storageBlobEndpoint}${deploymentContainerName}'
          authentication: {
            type: 'SystemAssignedIdentity'
          }
        }
      }
      scaleAndConcurrency: {
        maximumInstanceCount: maximumInstanceCount
        instanceMemoryMB: instanceMemoryMB
      }
      runtime: {
        name: 'dotnet-isolated'
        version: '10.0'
      }
    }
  }
}

// Deployments use Microsoft Entra ID (Publish-FunctionApp.ps1), so basic (user/password) publishing is turned off.
resource scmBasicAuth 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-04-01' = {
  parent: app
  name: 'scm'
  properties: {
    allow: false
  }
}

resource ftpBasicAuth 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-04-01' = {
  parent: app
  name: 'ftp'
  properties: {
    allow: false
  }
}

output name string = app.name
output defaultHostName string = app.properties.defaultHostName
output principalId string = app.identity.principalId
