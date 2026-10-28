// Service Bus namespace and the queue between UpcomingEventsFunction and PredictionFunction.

param namespaceName string
param location string
param tags object

@allowed([
  'Basic'
  'Standard'
])
param sku string

param queueName string

resource namespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: namespaceName
  location: location
  tags: tags
  sku: {
    name: sku
    tier: sku
  }
  properties: {
    // Only Microsoft Entra ID (managed identity, RBAC): no shared access keys.
    disableLocalAuth: true
    minimumTlsVersion: '1.2'
  }
}

// Same settings as the queue in OddsCollector.AppHost: a message that fails 3 times is dead-lettered,
// and an upcoming event older than an hour is replaced by the next collection anyway.
resource queue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: namespace
  name: queueName
  properties: {
    maxDeliveryCount: 3
    lockDuration: 'PT1M'
    defaultMessageTimeToLive: 'PT1H'
    deadLetteringOnMessageExpiration: false
  }
}

output name string = namespace.name
// serviceBusEndpoint is https://<name>.servicebus.windows.net:443/ (the suffix differs in sovereign clouds).
output fullyQualifiedNamespace string = split(split(namespace.properties.serviceBusEndpoint, '//')[1], ':')[0]
