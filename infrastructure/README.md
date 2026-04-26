# Veritas Infrastructure

Bicep templates provisioning all Azure resources for the Veritas platform.

## Resources provisioned

| Resource | Name pattern | Purpose |
|----------|-------------|---------|
| Log Analytics + App Insights | `log-veritas-{env}` | Logging (FR-NFR-10/11/12) |
| Key Vault | `kv-veritas-{env}` | Secret storage |
| Storage Account (Data Lake Gen2) | `stveritas{env}` | Corpus data (FR-1.6) |
| Cosmos DB (serverless) | `cosmos-veritas-{env}` | Corpus/doc/experiment metadata |
| Service Bus | `sb-veritas-{env}` | Extraction job queue (FR-2.1) |
| Azure AI Document Intelligence | `di-veritas-{env}` | Text extraction (FR-1.19) |
| Azure OpenAI | `oai-veritas-{env}` | RAG answer + embeddings (FR-1.27) |
| Azure AI Search | `srch-veritas-{env}` | Vector index (FR-1.22/1.23) |
| App Service Plan + Web App | `app-veritas-{env}` | Hosts Veritas.Api |

All service-to-service access uses **managed identity + RBAC** — no connection strings.

## Pre-requisites

1. Azure CLI installed: `az --version`
2. Bicep CLI installed: `az bicep install`
3. Resource group created: `az group create -n rg-veritas-dev -l swedencentral`
4. Fill in `parameters.json`:
   - `aadTenantId` — from Entra ID → Overview
   - `aadClientId` — create an App Registration for Veritas.Api

## Deploy

```bash
az deployment group create \
  --resource-group rg-veritas-dev \
  --template-file infrastructure/main.bicep \
  --parameters infrastructure/parameters.json
```

## What to do after deploy

1. **Azure OpenAI**: create deployments in Azure OpenAI Studio for `gpt-4o` and `text-embedding-3-large`
2. **AI Search index**: design index schema with search expert, then create the `veritas-documents` index
3. **Replace `[MOCK]` stubs** in code — all endpoints are emitted as deployment outputs

## Outputs

After deployment, retrieve outputs:

```bash
az deployment group show \
  --resource-group rg-veritas-dev \
  --name main \
  --query properties.outputs
```

Key output: `webAppUrl` — the public URL of Veritas.Api.

## [MOCK] items requiring action before production

| Item | File | What to do |
|------|------|-----------|
| `aadTenantId` / `aadClientId` | `parameters.json` | Create App Registration in Entra ID |
| OpenAI region | `main.bicep` L96 | Azure OpenAI availability varies by region — verify before deploy |
| Cosmos RBAC role ID | `main.bicep` L220 | Verify built-in Cosmos DB role ID in portal |
| Search index schema | `main.bicep` L243 | Design with search expert before implementing `AzureSearchIndexBackend` |
| App Service SKU | `parameters.json` | Upgrade to P1v3 for production |
| Network lockdown | `main.bicep` L80 | Restrict storage to VNet in production (FR-NFR-7) |
