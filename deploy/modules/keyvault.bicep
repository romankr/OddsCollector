// Key Vault with The Odds API key: the only secret of the app (everything else uses managed identity).
// The function app reads it through a Key Vault reference in the OddsApiClient__ApiKey setting.

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
    // Key Vault rejects false; null leaves purge protection off.
    enablePurgeProtection: enablePurgeProtection ? true : null
    publicNetworkAccess: 'Enabled'
  }
}

// Written through Azure Resource Manager, so the deployer needs no data-plane role on the vault.
resource secret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (!empty(oddsApiKey)) {
  parent: vault
  name: secretName
  properties: {
    value: oddsApiKey
    contentType: 'The Odds API key'
  }
}

output name string = vault.name
// Versionless URI: the app picks up a rotated key without a redeployment (within a day, or on restart).
output oddsApiKeySecretUri string = '${vault.properties.vaultUri}secrets/${secretName}/'
