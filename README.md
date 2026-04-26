## Veritas

A private corpus intelligence platform for scientific document analysis.
Built for researchers who need to query, extract, and cross-validate structured evidence
from a closed corpus — with grounded answers, full citations, and no hallucination outside
the registered document set.

---

## What it does

- **Ingest** scientific documents (PDF/text) into a versioned corpus
- **Extract** structured parameters and experimental results using a domain pack–driven agent pipeline
- **Validate** extractions against pack ontology with field-level coverage reports
- **Query** the corpus with RAG — answers are grounded in the indexed documents and refused when evidence is insufficient
- **Compare** extraction outcomes across multiple domain packs to surface agreement, conflict, and gaps
- **Analyse** multi-pack results with statistical notebooks (correlation, hypothesis testing, publication bias)

---

## Repository layout

### Backend (`src/`)
| Project | Role |
|---|---|
| `Veritas.Core` | Shared models and contracts |
| `Veritas.Storage` | Document store interface + Data Lake path builder |
| `Veritas.Corpora` | Corpus CRUD, document ingestion, text extraction, indexing |
| `Veritas.DomainPacks` | Pack loader, validator, runtime |
| `Veritas.Extraction` | 5-agent extraction pipeline + ContractCoverageValidator |
| `Veritas.Rag` | RAG pipeline: retriever, answer generator, citation validator |
| `Veritas.Api` | REST API — 7 controllers, JWT auth stub, Scalar UI |

### Frontend (`src/Veritas.Web/`)
React + TypeScript + Tailwind v4, built with Vite.

| Page | Route |
|---|---|
| Corpora | `/corpora` |
| Corpus detail | `/corpora/:id` |
| Document detail | `/corpora/:id/documents/:docId` |
| Validation queue | `/validation` |
| RAG Search | `/search` |
| Experiments | `/experiments` |
| Hypothesis | `/hypothesis` |
| Pack compare | `/corpora/:id/compare` |
| Analysis | `/analysis` |

### Domain packs (`domain-pack-schema/`)
- `schema-v1/` — JSON schemas for pack structure
- `examples/lenr-pack-example/` — LENR example pack (`[MOCK]` placeholders, physicist session required)

### Analysis (`analysis/`)
Jupyter notebooks (Python):
- `01_parameter_analysis.ipynb` — distributions + histograms
- `02_hypothesis_testing.ipynb` — support/contradict/inconclusive + chi-square
- `03_correlation_analysis.ipynb` — Pearson matrix + scatter plots
- `04_publication_bias.ipynb` — funnel plot + Egger's test
- `requirements.txt`

### Infrastructure (`infrastructure/`)
Azure Bicep — 9 resources (Cosmos DB, Azure AI Search, Data Lake, OpenAI, Key Vault, App Service, managed identity, RBAC).

---

## Quick start

**API:**
```powershell
dotnet restore
dotnet build gh-maf-template.sln
dotnet run --project src/Veritas.Api/Veritas.Api.csproj
# Scalar API UI: https://localhost:5001/scalar/v1
```

**Web UI:**
```powershell
cd src/Veritas.Web
npm install
npm run dev
# http://localhost:5173
```

**Analysis notebooks:**
```powershell
cd analysis
pip install -r requirements.txt
jupyter lab
```

**Infrastructure (deploy to Azure):**
```powershell
az deployment group create `
  --resource-group <rg> `
  --template-file infrastructure/main.bicep `
  --parameters @infrastructure/parameters.json
```
See `HOSTING.md` for full deployment instructions.

---

## GitHub guidance layout
- `.github/copilot-instructions.md` — repo-wide Copilot operating rules
- `.github/instructions/*` — path-specific coding, testing, docs, and security constraints
- `.github/agents/` — Planner, Implementer, Reviewer, Verifier instruction files
- `.github/skills/` — `corpus-ingestion-rules`, `content-understanding-schema`, `normalization-rules`, `domain-pack-schema`, `rag-plugin-contract`
- `.github/workflows/` — CI, PR checks, evals, extraction pipeline trigger

---

## Key design decisions

- **All answers are corpus-scoped** — the RAG pipeline refuses queries when no relevant chunks are found rather than generating unsupported answers.
- **Domain packs are versioned** — every extraction output is tagged with `pack_id` + `pack_version` so results are reproducible as packs evolve.
- **Pipeline is resumable** — intermediate state is written to storage after each step; a failed job can be retried from any point.
- **`[MOCK]` discipline** — every Azure service stub is annotated with the NuGet package, config key, and exact replacement needed. No silent stubs.

---

## Current status

All service stubs are marked `[MOCK]`. The system compiles and runs locally with in-memory storage.
Real Azure connections (Data Lake, Cosmos DB, Azure AI Search, OpenAI) require configuration.
See `REVIEW.md` for a full implementation assessment and priority list for next steps.

---

## Architecture docs
- `docs/architecture/system-overview.md`
- `docs/architecture/decision-log.md`
- `REVIEW.md` — critical implementation assessment
- `HOSTING.md` — local dev, Docker, App Service deployment
- `SETUP.md` — developer environment setup
- `USAGE.md` — API and UI usage guide
