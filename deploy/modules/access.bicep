// Role assignments of the function app's managed identity (README "Managed identity in Azure"),
// and optional Cosmos DB data access for a person or pipeline (Data Explorer, data migrations).

param functionAppPrincipalId string
param storageAccountName string
param cosmosAccountName string
param serviceBusNamespaceName string
param keyVaultName string

@description('Object id of a user, group or service principal that gets Cosmos DB data access. Empty: nobody.')
param dataContributorPrincipalId string

// Built-in Azure roles: https://learn.microsoft.com/azure/role-based-access-control/built-in-roles
var storageBlobDataOwner = 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
var serviceBusDataSender = '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
var serviceBusDataReceiver = '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
var keyVaultSecretsUser = '4633458b-17de-408a-b874-0445c86b69e6'

// Cosmos DB Built-in Data Contributor (a Cosmos DB data-plane role, not an Azure role).
var cosmosDataContributor = '00000000-0000-0000-0000-000000000002'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-11-15' existing = {
  name: cosmosAccountName
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusNamespaceName
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// AzureWebJobsStorage (timer leases, host keys) and the deployment package container.
resource appStorage 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, functionAppPrincipalId, storageBlobDataOwner)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataOwner)
    principalId: functionAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// UpcomingEventsFunction sends to the queue, PredictionFunction receives and dead-letters.
resource appServiceBusSender 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: serviceBusNamespace
  name: guid(serviceBusNamespace.id, functionAppPrincipalId, serviceBusDataSender)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', serviceBusDataSender)
    principalId: functionAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource appServiceBusReceiver 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: serviceBusNamespace
  name: guid(serviceBusNamespace.id, functionAppPrincipalId, serviceBusDataReceiver)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', serviceBusDataReceiver)
    principalId: functionAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Key Vault reference of OddsApiClient__ApiKey.
resource appKeyVaultSecrets 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyVault
  name: guid(keyVault.id, functionAppPrincipalId, keyVaultSecretsUser)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUser)
    principalId: functionAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Cosmos DB input/output bindings (read predictions, upsert predictions and results).
resource appCosmosData 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-11-15' = {
  parent: cosmosAccount
  name: guid(cosmosAccount.id, functionAppPrincipalId, cosmosDataContributor)
  properties: {
    roleDefinitionId: '${cosmosAccount.id}/sqlRoleDefinitions/${cosmosDataContributor}'
    principalId: functionAppPrincipalId
    scope: cosmosAccount.id
  }
}

resource personCosmosData 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-11-15' = if (!empty(dataContributorPrincipalId)) {
  parent: cosmosAccount
  name: guid(cosmosAccount.id, dataContributorPrincipalId, cosmosDataContributor)
  properties: {
    roleDefinitionId: '${cosmosAccount.id}/sqlRoleDefinitions/${cosmosDataContributor}'
    principalId: dataContributorPrincipalId
    scope: cosmosAccount.id
  }
}
