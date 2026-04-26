# Hosting Guide

This document explains how to run and deploy the Veritas platform.

---

## Local development

### Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 10.0+ |
| Node.js | 20+ |
| npm | 10+ |

### Start the API

```bash
cd src/Veritas.Api
dotnet run
# API listens on http://localhost:5000
# Scalar UI at http://localhost:5000/scalar/v1 (development only)
```

### Start the React UI

```bash
cd src/Veritas.Web
npm install
npm run dev
# UI at http://localhost:5173
# Requests to /api/* are proxied to http://localhost:5000
```

---

## Environment variables

Set these before running the API. All are optional in local development (mock implementations are used when values are absent).

| Variable | Description | Required in prod |
|----------|-------------|-----------------|
| `AZURE_STORAGE_ACCOUNT` | Data Lake Gen2 storage account name | ✅ |
| `AZURE_STORAGE_CONTAINER` | Container name (default: `veritas`) | ✅ |
| `AZURE_SEARCH_ENDPOINT` | Azure AI Search endpoint URL | ✅ |
| `AZURE_SEARCH_KEY` | Azure AI Search admin key (or use managed identity) | ✅ |
| `AZURE_OPENAI_ENDPOINT` | Azure OpenAI endpoint URL | ✅ |
| `AZURE_OPENAI_DEPLOYMENT` | Chat completion deployment name | ✅ |
| `AZURE_OPENAI_EMBEDDING_DEPLOYMENT` | Embedding model deployment name | ✅ |
| `COSMOS_DB_ENDPOINT` | Azure Cosmos DB endpoint URL | ✅ |
| `COSMOS_DB_DATABASE` | Database name (default: `veritas`) | ✅ |

> **[MOCK]** In local dev, all Azure calls are intercepted by mock implementations.  
> Wire real clients in `Program.cs` after completing the external sessions noted in `DECISIONS.md`.

---

## Docker

A `Dockerfile` is not yet provided. The recommended approach:

```dockerfile
# [MOCK] — fill in after deciding on base image and build stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Veritas.Api/Veritas.Api.csproj -c Release -o /app/publish

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Veritas.Api.dll"]
```

---

## Azure App Service

1. Provision App Service with .NET 10 runtime (Linux or Windows).
2. Assign a **system-assigned managed identity** to the App Service instance.
3. Grant the identity these roles (see `infrastructure/main.bicep` for role IDs):
   - Storage Blob Data Contributor
   - Search Index Data Contributor
   - Cognitive Services OpenAI User
   - Cosmos DB Built-in Data Contributor
4. Deploy via GitHub Actions (see `.github/workflows/`) or `az webapp deploy`.
5. Set all environment variables listed above via App Service **Configuration → Application settings**.

> **Managed identity**: Do not store any credentials in application settings.  
> All Azure SDK calls should use `DefaultAzureCredential` which automatically picks up the managed identity.

---

## Azure infrastructure provisioning

```bash
az deployment group create \
  --resource-group <rg-name> \
  --template-file infrastructure/main.bicep \
  --parameters @infrastructure/parameters.json
```

See [`infrastructure/README.md`](../infrastructure/README.md) for parameter descriptions and [MOCK] items to fill in.

---

## React UI production build

```bash
cd src/Veritas.Web
npm run build
# Output in src/Veritas.Web/dist/
```

Serve `dist/` as a static site (Azure Static Web Apps, a CDN, or from the API host using `UseStaticFiles`).

The UI reads `/api/*` for all backend calls. In production the API must be on the same origin or the `VITE_API_BASE` environment variable must be set to the full API URL and the `vite.config.ts` proxy must be replaced with a CORS policy.

---

## Running tests

```bash
dotnet test gh-maf-template.sln
```

All 42 tests should pass. Tests use in-memory/mock implementations and require no external services.
