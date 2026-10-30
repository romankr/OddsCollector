param vaultName string
param location string
param tags object

@description('The Odds API key. Empty: the secret is left as it is.')
@secure()
param oddsApiKey string

param enablePurgeProtection bool

var secretName = 'OddsApiKey'

resource vault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: vaultName
  location: location
  tags: tags
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enablePurgeProtection: enablePurgeProtection ? true : null
    publicNetworkAccess: 'Enabled'
  }
}

resource secret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (!empty(oddsApiKey)) {
  parent: vault
  name: secretName
  properties: {
    value: oddsApiKey
    contentType: 'The Odds API key'
  }
}

output name string = vault.name
output oddsApiKeySecretUri string = '${vault.properties.vaultUri}secrets/${secretName}/'
