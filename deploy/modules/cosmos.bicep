// Cosmos DB account (serverless) and the data schema of OddsCollector: the database and its containers.
// The containers are the only source of truth for the schema in Azure: the app never creates them, and
// key-based metadata writes are turned off, so containers can't be created or changed outside this template.
//
// Changing the schema:
// - Indexing policy and time to live change in place; Cosmos DB rebuilds the index in the background.
// - The partition key and unique keys can't change. To change them, add a new container here, copy the data
//   (container copy job or change feed) and switch the CosmosDb__*Container settings to it.

param accountName string
param location string
param tags object
param databaseName string
param eventPredictionsContainerName string
param eventResultsContainerName string

@description('Time to live of stored predictions, in seconds. 0 keeps them forever.')
param predictionsTimeToLiveSeconds int

resource account 'Microsoft.DocumentDB/databaseAccounts@2024-11-15' = {
  name: accountName
  location: location
  tags: tags
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    // Only Microsoft Entra ID (managed identity, RBAC): no account keys.
    disableLocalAuth: true
    disableKeyBasedMetadataWriteAccess: true
    minimalTlsVersion: 'Tls12'
    publicNetworkAccess: 'Enabled'
  }
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-11-15' = {
  parent: account
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
  }
}

// Documents are keyed by the event id (EventPrediction.Id / EventResult.Id, serialized as "id"); the output
// bindings upsert by it, which keeps the functions idempotent. Same partition key as OddsCollector.AppHost.
var partitionKey = {
  paths: [
    '/id'
  ]
  kind: 'Hash'
  version: 2
}

// Everything is indexed: PredictionsHttpFunction filters and sorts by CommenceTime, and ad hoc queries keep working.
var indexingPolicy = {
  indexingMode: 'consistent'
  automatic: true
  includedPaths: [
    {
      path: '/*'
    }
  ]
  excludedPaths: [
    {
      path: '/"_etag"/?'
    }
  ]
}

resource eventPredictions 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-11-15' = {
  parent: database
  name: eventPredictionsContainerName
  properties: {
    resource: union(
      {
        id: eventPredictionsContainerName
        partitionKey: partitionKey
        indexingPolicy: indexingPolicy
      },
      predictionsTimeToLiveSeconds > 0 ? { defaultTtl: predictionsTimeToLiveSeconds } : {}
    )
  }
}

resource eventResults 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-11-15' = {
  parent: database
  name: eventResultsContainerName
  properties: {
    resource: {
      id: eventResultsContainerName
      partitionKey: partitionKey
      indexingPolicy: indexingPolicy
    }
  }
}

output name string = account.name
output endpoint string = account.properties.documentEndpoint
