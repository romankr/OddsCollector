// Azure environment of OddsCollector: Flex Consumption function app, Cosmos DB (serverless) with the database and
// containers, Service Bus queue, Key Vault for The Odds API key, storage, Application Insights and alerts.
// Every connection is identity-based (managed identity + RBAC); no keys or connection strings are stored.
// Deploy with Deploy-Infrastructure.ps1 (or Deploy-Environment.ps1, which also publishes the code).

targetScope = 'resourceGroup'

@description('Short environment name, used in resource names: lowercase letters and digits, for example dev or prod.')
@minLength(2)
@maxLength(8)
param environmentName string

@description('Region of every resource. Defaults to the resource group region.')
param location string = resourceGroup().location

@description('Semicolon-separated soccer_* sport keys of The Odds API, for example soccer_epl;soccer_spain_la_liga.')
param leagues string = 'soccer_epl'

@description('NCRONTAB schedule of UpcomingEventsFunction.')
param upcomingEventsTimerInterval string = '0 0 */6 * * *'

@description('NCRONTAB schedule of EventResultsFunction. Keep it under 3 days, or some results are never stored.')
param eventResultsTimerInterval string = '0 0 6 * * *'

@description('The Odds API key. Leave empty to keep the secret already stored in Key Vault.')
@secure()
param oddsApiKey string = ''

@description('Service Bus tier. Basic is enough for a single queue with dead-lettering.')
@allowed([
  'Basic'
  'Standard'
])
param serviceBusSku string = 'Basic'

@description('Maximum number of Flex Consumption instances.')
@minValue(40)
@maxValue(1000)
param maximumInstanceCount int = 40

@description('Memory of a Flex Consumption instance, in MB.')
@allowed([
  512
  2048
  4096
])
param instanceMemoryMB int = 2048

@description('Time to live of stored predictions, in seconds. 0 keeps them forever.')
@minValue(0)
param predictionsTimeToLiveSeconds int = 0

@description('Object id of a user, group or service principal that gets data access to Cosmos DB (Data Explorer, data migrations). Empty: nobody.')
param dataContributorPrincipalId string = ''

@description('E-mail that receives the alerts. Empty: no alerts are created.')
param alertEmail string = ''

@description('Turn on Key Vault purge protection. It cannot be turned off later.')
param enablePurgeProtection bool = false

// Names that the code and OddsCollector.AppHost (AppHostSettings.cs) also use.
var databaseName = 'OddsCollector'
var eventPredictionsContainerName = 'EventPredictions'
var eventResultsContainerName = 'EventResults'
var queueName = 'upcoming-events'

var suffix = uniqueString(subscription().id, resourceGroup().id, environmentName)
var tags = {
  application: 'OddsCollector'
  environment: environmentName
}

module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring'
  params: {
    workspaceName: 'log-oddscollector-${environmentName}-${suffix}'
    applicationInsightsName: 'appi-oddscollector-${environmentName}-${suffix}'
    location: location
    tags: tags
  }
}

module storage 'modules/storage.bicep' = {
  name: 'storage'
  params: {
    accountName: take('stodds${environmentName}${suffix}', 24)
    location: location
    tags: tags
  }
}

module cosmos 'modules/cosmos.bicep' = {
  name: 'cosmos'
  params: {
    accountName: 'cosmos-oddscollector-${environmentName}-${suffix}'
    location: location
    tags: tags
    databaseName: databaseName
    eventPredictionsContainerName: eventPredictionsContainerName
    eventResultsContainerName: eventResultsContainerName
    predictionsTimeToLiveSeconds: predictionsTimeToLiveSeconds
  }
}

module serviceBus 'modules/servicebus.bicep' = {
  name: 'serviceBus'
  params: {
    namespaceName: 'sbns-oddscollector-${environmentName}-${suffix}'
    location: location
    tags: tags
    sku: serviceBusSku
    queueName: queueName
  }
}

module keyVault 'modules/keyvault.bicep' = {
  name: 'keyVault'
  params: {
    vaultName: 'kv-odds-${environmentName}-${take(suffix, 7)}'
    location: location
    tags: tags
    oddsApiKey: oddsApiKey
    enablePurgeProtection: enablePurgeProtection
  }
}

module functionApp 'modules/functionapp.bicep' = {
  name: 'functionApp'
  params: {
    planName: 'plan-oddscollector-${environmentName}-${suffix}'
    appName: 'func-oddscollector-${environmentName}-${suffix}'
    location: location
    tags: tags
    maximumInstanceCount: maximumInstanceCount
    instanceMemoryMB: instanceMemoryMB
    storageAccountName: storage.outputs.name
    storageBlobEndpoint: storage.outputs.blobEndpoint
    deploymentContainerName: storage.outputs.deploymentContainerName
    applicationInsightsConnectionString: monitoring.outputs.applicationInsightsConnectionString
    // Settings of the app, see README "Application settings" and OddsCollector.AppHost/Program.cs.
    appSettings: {
      CosmosDb__Connection__accountEndpoint: cosmos.outputs.endpoint
      CosmosDb__Database: databaseName
      CosmosDb__EventPredictionsContainer: eventPredictionsContainerName
      CosmosDb__EventResultsContainer: eventResultsContainerName
      ServiceBus__Connection__fullyQualifiedNamespace: serviceBus.outputs.fullyQualifiedNamespace
      ServiceBus__Queue: queueName
      OddsApiClient__ApiKey: '@Microsoft.KeyVault(SecretUri=${keyVault.outputs.oddsApiKeySecretUri})'
      OddsApiClient__Leagues: leagues
      UpcomingEventsFunction__TimerInterval: upcomingEventsTimerInterval
      EventResultsFunction__TimerInterval: eventResultsTimerInterval
    }
  }
}

module access 'modules/access.bicep' = {
  name: 'access'
  params: {
    functionAppPrincipalId: functionApp.outputs.principalId
    storageAccountName: storage.outputs.name
    cosmosAccountName: cosmos.outputs.name
    serviceBusNamespaceName: serviceBus.outputs.name
    keyVaultName: keyVault.outputs.name
    dataContributorPrincipalId: dataContributorPrincipalId
  }
}

module alerts 'modules/alerts.bicep' = if (!empty(alertEmail)) {
  name: 'alerts'
  params: {
    environmentName: environmentName
    location: location
    tags: tags
    alertEmail: alertEmail
    applicationInsightsId: monitoring.outputs.applicationInsightsId
    serviceBusNamespaceName: serviceBus.outputs.name
    queueName: queueName
  }
}

output functionAppName string = functionApp.outputs.name
output functionAppHostName string = functionApp.outputs.defaultHostName
output cosmosAccountName string = cosmos.outputs.name
output cosmosEndpoint string = cosmos.outputs.endpoint
output serviceBusNamespace string = serviceBus.outputs.fullyQualifiedNamespace
output keyVaultName string = keyVault.outputs.name
output applicationInsightsName string = monitoring.outputs.applicationInsightsName
