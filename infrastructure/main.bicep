// Veritas Platform — Azure Infrastructure
// Provisions all Azure resources required by Veritas.Api and Veritas.Rag.
//
// Resources provisioned:
//   - Log Analytics Workspace + Application Insights  (FR-NFR-10/11/12)
//   - Key Vault                                        (secret storage for all service keys)
//   - Storage Account (Data Lake Gen2)                 (FR-1.6 — corpus data storage)
//   - Cosmos DB (serverless)                           (corpus metadata, experiment records)
//   - Service Bus (Standard)                           (extraction queue — FR-2.1)
//   - Azure AI Document Intelligence                   (text extraction — FR-1.19)
//   - Azure OpenAI                                     (RAG answer generation — FR-1.27)
//   - Azure AI Search                                  (vector index — FR-1.22/1.23)
//   - App Service Plan + Web App                       (Veritas.Api hosting)
//
// All resources use system-assigned managed identity + RBAC. No connection strings in app config.
// [MOCK] Values that require real decisions are marked with [MOCK] comments.

targetScope = 'resourceGroup'

// ─── Parameters ──────────────────────────────────────────────────────────────

@description('Short environment name: dev | staging | prod')
@allowed(['dev', 'staging', 'prod'])
param environment string = 'dev'

@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Short prefix for all resource names (2–8 chars, lowercase alphanumeric)')
@minLength(2)
@maxLength(8)
param prefix string = 'veritas'

@description('Azure AD tenant ID for AAD auth (FR-NFR-6)')
// [MOCK] Replace with your actual tenant ID
param aadTenantId string

@description('Azure AD client ID for the Veritas.Api app registration')
// [MOCK] Create an app registration in Entra ID and paste the client ID here
param aadClientId string

@description('Azure OpenAI deployment name for the GPT-4o chat model')
// [MOCK] Create the deployment in Azure OpenAI Studio and set name here
param openAiChatDeployment string = 'gpt-4o'

@description('Azure OpenAI deployment name for the text-embedding model')
// [MOCK] Confirm embedding model with search expert before deploying
param openAiEmbeddingDeployment string = 'text-embedding-3-large'

@description('App Service Plan SKU')
// [MOCK] B1 is fine for dev; upgrade to P1v3 for production
param appServiceSku string = 'B1'

// ─── Variables ───────────────────────────────────────────────────────────────

var suffix = '${prefix}-${environment}'
var tags = {
  project: 'veritas'
  environment: environment
  managedBy: 'bicep'
}

// ─── Log Analytics + Application Insights ────────────────────────────────────

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'log-${suffix}'
  location: location
  tags: tags
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-${suffix}'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

// ─── Key Vault ────────────────────────────────────────────────────────────────

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: 'kv-${suffix}'
  location: location
  tags: tags
  properties: {
    sku: { family: 'A', name: 'standard' }
    tenantId: aadTenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enabledForDeployment: false
    enabledForTemplateDeployment: false
  }
}

// ─── Storage Account (Data Lake Gen2) ────────────────────────────────────────

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'st${replace(suffix, '-', '')}' // storage names: no hyphens, max 24 chars
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: { name: 'Standard_LRS' }
  properties: {
    isHnsEnabled: true           // Hierarchical namespace = Data Lake Gen2
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
    networkAcls: {
      defaultAction: 'Allow'     // [MOCK] Restrict to VNet in production (FR-NFR-7)
    }
  }
}

// Data Lake containers: one per storage zone (FR-1.6)
var datalakeContainers = ['raw', 'extracted', 'validated', 'classified', 'experiments', 'analysis']

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
}

resource datalakeContainerResources 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = [for name in datalakeContainers: {
  parent: blobServices
  name: name
  properties: {
    publicAccess: 'None'
  }
}]

// ─── Cosmos DB (serverless) ───────────────────────────────────────────────────

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-11-15' = {
  name: 'cosmos-${suffix}'
  location: location
  tags: tags
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: { defaultConsistencyLevel: 'Session' }
    locations: [{ locationName: location, failoverPriority: 0 }]
    capabilities: [{ name: 'EnableServerless' }]
    enableAutomaticFailover: false
    // [MOCK] Enable multi-region writes + geo-replication for production
  }
}

resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-11-15' = {
  parent: cosmosAccount
  name: 'veritas'
  properties: {
    resource: { id: 'veritas' }
  }
}

var cosmosContainers = [
  { name: 'corpora',     partitionKey: '/owner' }
  { name: 'documents',   partitionKey: '/corpusId' }
  { name: 'experiments', partitionKey: '/corpusId' }
  { name: 'jobs',        partitionKey: '/corpusId' }
]

resource cosmosContainerResources 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-11-15' = [for c in cosmosContainers: {
  parent: cosmosDatabase
  name: c.name
  properties: {
    resource: {
      id: c.name
      partitionKey: { paths: [c.partitionKey], kind: 'Hash' }
    }
  }
}]

// ─── Service Bus ──────────────────────────────────────────────────────────────

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: 'sb-${suffix}'
  location: location
  tags: tags
  sku: { name: 'Standard', tier: 'Standard' }
}

resource extractionQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBus
  name: 'extraction-jobs'
  properties: {
    maxDeliveryCount: 5
    lockDuration: 'PT5M'
    defaultMessageTimeToLive: 'P1D'
  }
}

// ─── Azure AI Document Intelligence ──────────────────────────────────────────

resource documentIntelligence 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: 'di-${suffix}'
  location: location
  tags: tags
  kind: 'FormRecognizer'
  sku: { name: 'S0' }
  properties: {
    publicNetworkAccess: 'Enabled'
    // [MOCK] Replace TextExtractionService stub with Azure.AI.FormRecognizer SDK
    // Endpoint available at: documentIntelligence.properties.endpoint
  }
}

// ─── Azure OpenAI ─────────────────────────────────────────────────────────────

resource openAi 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: 'oai-${suffix}'
  location: location   // [MOCK] Azure OpenAI is not available in all regions — check availability
  tags: tags
  kind: 'OpenAI'
  sku: { name: 'S0' }
  properties: {
    publicNetworkAccess: 'Enabled'
    customSubDomainName: 'oai-${suffix}'
  }
}

resource openAiChatDeploymentResource 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  parent: openAi
  name: openAiChatDeployment
  sku: {
    name: 'Standard'
    // [MOCK] Set TPM capacity after load testing
    capacity: 10
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
      // [MOCK] Pin to specific model version after testing
      version: '2024-11-20'
    }
  }
}

resource openAiEmbeddingDeploymentResource 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  parent: openAi
  name: openAiEmbeddingDeployment
  sku: {
    name: 'Standard'
    // [MOCK] Set TPM capacity based on search expert's chunking strategy
    capacity: 10
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'text-embedding-3-large'
      version: '1'
    }
  }
  dependsOn: [openAiChatDeploymentResource]
}

// ─── Azure AI Search ──────────────────────────────────────────────────────────

resource aiSearch 'Microsoft.Search/searchServices@2023-11-01' = {
  name: 'srch-${suffix}'
  location: location
  tags: tags
  sku: {
    // [MOCK] Basic is fine for dev; Standard for production (needed for semantic ranking)
    name: environment == 'prod' ? 'standard' : 'basic'
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
    publicNetworkAccess: 'enabled'
    // [MOCK] Index schema (fields, chunking, scoring profiles) designed by search expert
    // Replace MockIndexBackend with AzureSearchIndexBackend using Azure.Search.Documents SDK
  }
}

// ─── App Service Plan + Web App (Veritas.Api) ────────────────────────────────

resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: 'asp-${suffix}'
  location: location
  tags: tags
  sku: { name: appServiceSku }
  kind: 'linux'
  properties: {
    reserved: true // required for Linux
  }
}

resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: 'app-${suffix}'
  location: location
  tags: tags
  kind: 'app,linux'
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      minTlsVersion: '1.2'
      appSettings: [
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.properties.ConnectionString }
        { name: 'ASPNETCORE_ENVIRONMENT',                value: environment == 'prod' ? 'Production' : 'Development' }
        { name: 'KeyVault__Uri',                          value: keyVault.properties.vaultUri }
        // All other secrets resolved from Key Vault via managed identity at runtime
        // [MOCK] Add AzureAd__TenantId and AzureAd__ClientId once app registration is created
        { name: 'AzureAd__TenantId',  value: aadTenantId }
        { name: 'AzureAd__ClientId',  value: aadClientId }
        { name: 'AzureAd__Instance',  value: 'https://login.microsoftonline.com/' }
        { name: 'Search__Endpoint',   value: 'https://${aiSearch.name}.search.windows.net' }
        { name: 'OpenAi__Endpoint',   value: openAi.properties.endpoint }
        { name: 'OpenAi__ChatDeployment',      value: openAiChatDeployment }
        { name: 'OpenAi__EmbeddingDeployment', value: openAiEmbeddingDeployment }
        { name: 'Storage__AccountName',        value: storageAccount.name }
        { name: 'Cosmos__Endpoint',            value: cosmosAccount.properties.documentEndpoint }
        { name: 'ServiceBus__FullyQualifiedNamespace', value: '${serviceBus.name}.servicebus.windows.net' }
        { name: 'DocumentIntelligence__Endpoint', value: documentIntelligence.properties.endpoint }
      ]
    }
  }
}

// ─── RBAC — Web App managed identity → each service ──────────────────────────
// FR-NFR-6: no connection strings, all access via managed identity + RBAC

var storageContributorRole  = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe') // Storage Blob Data Contributor
var cosmosContributorRole   = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '00000000-0000-0000-0000-000000000002') // [MOCK] Cosmos built-in role ID — verify in portal
var sbSenderRole            = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39') // Azure Service Bus Data Sender
var sbReceiverRole          = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0') // Azure Service Bus Data Receiver
var searchContributorRole   = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '8ebe5a00-799e-43f5-93ac-243d3dce84a7') // Search Index Data Contributor
var cogServicesUserRole     = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'a97b65f3-24c7-4388-baec-2e87135dc908') // Cognitive Services User
var kvSecretsRole           = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User

resource rbacStorage 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, webApp.id, storageContributorRole)
  scope: storageAccount
  properties: {
    roleDefinitionId: storageContributorRole
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource rbacServiceBusSend 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceBus.id, webApp.id, sbSenderRole)
  scope: serviceBus
  properties: {
    roleDefinitionId: sbSenderRole
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource rbacServiceBusReceive 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceBus.id, webApp.id, sbReceiverRole)
  scope: serviceBus
  properties: {
    roleDefinitionId: sbReceiverRole
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource rbacSearch 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aiSearch.id, webApp.id, searchContributorRole)
  scope: aiSearch
  properties: {
    roleDefinitionId: searchContributorRole
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource rbacOpenAi 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(openAi.id, webApp.id, cogServicesUserRole)
  scope: openAi
  properties: {
    roleDefinitionId: cogServicesUserRole
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource rbacDocIntel 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(documentIntelligence.id, webApp.id, cogServicesUserRole)
  scope: documentIntelligence
  properties: {
    roleDefinitionId: cogServicesUserRole
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource rbacKeyVault 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, webApp.id, kvSecretsRole)
  scope: keyVault
  properties: {
    roleDefinitionId: kvSecretsRole
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// ─── Outputs ──────────────────────────────────────────────────────────────────

output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output webAppName string = webApp.name
output storageAccountName string = storageAccount.name
output cosmosEndpoint string = cosmosAccount.properties.documentEndpoint
output searchEndpoint string = 'https://${aiSearch.name}.search.windows.net'
output openAiEndpoint string = openAi.properties.endpoint
output documentIntelligenceEndpoint string = documentIntelligence.properties.endpoint
output keyVaultUri string = keyVault.properties.vaultUri
output appInsightsConnectionString string = appInsights.properties.ConnectionString
