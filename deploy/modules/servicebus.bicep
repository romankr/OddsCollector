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
    disableLocalAuth: true
    minimumTlsVersion: '1.2'
  }
}

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
output fullyQualifiedNamespace string = split(split(namespace.properties.serviceBusEndpoint, '//')[1], ':')[0]
