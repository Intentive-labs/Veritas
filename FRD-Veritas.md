# Functional Requirements Document (FRD)
## Veritas
**Version:** 1.4  
**Date:** April 2026  
**Owner:** Intentive Labs AB  
**Customer:** Cill AB  
**Status:** Draft – Copilot Agent Mode Ready

---

## 1. System Overview

### 1.1 Architecture Principles

- **User-provided corpora first** — no bundled data, no default corpus, no crawler in core
- **AI/LLM where it adds value** — extraction, querying, classification
- **Deterministic logic where it suffices** — ingestion, storage, pack loading, validation, statistics
- **MAF multi-agent only where coordination is required** — parameter extraction pipeline
- **Classical software architecture everywhere else** — REST API, CRUD, file operations
- **Domain Pack runtime is field-agnostic** — zero LENR logic in core
- **Veritas RAG has no domain logic** — finds and answers only
- **RAG designed for standalone institutional hosting**
- **Primary language: C#** — Python only for statistical analysis
- **React/TypeScript** — frontend

### 1.2 Solution Namespaces

```
Veritas.Core          ← domain models, contracts, shared interfaces
Veritas.Corpora       ← corpus CRUD, document ingestion, rights, indexing
Veritas.DomainPacks   ← domain pack runtime, schema validation
Veritas.Extraction    ← MAF agent extraction pipeline
Veritas.Storage       ← Azure Data Lake abstraction
Veritas.Api           ← ASP.NET Core REST API
Veritas.Rag           ← Generic RAG core (in veritas-rag repo)
Lenr.Rag              ← LENR corpus connector (in veritas-rag repo)
DomainPacks.LenrMagneticField ← Cill AB pack (in cill-ab/lenr-pack repo)
```

### 1.3 Repository Structure

```
intentive-labs/veritas          ← Core engine (public, MIT)
intentive-labs/veritas-rag      ← RAG service (public, MIT, hostable)
cill-ab/lenr-pack               ← Cill AB domain pack (private)
```

### 1.4 Component Map

```
┌────────────────────────────────────────────────────────────────┐
│                        React Frontend                           │
│  Corpus Mgmt │ Upload │ Validation │ Pack Compare │ Dashboard   │
└──────────────────────────────┬─────────────────────────────────┘
                               │ HTTPS / REST
┌──────────────────────────────▼─────────────────────────────────┐
│                     Veritas.Api (C#)                            │
│  Corpora │ Documents │ Ingestion │ Extraction │ Experiments      │
│  Hypothesis │ Analysis │ Pack Management                        │
└──────┬──────────────────────────────────────────────────────────┘
       │
       ├──────────────────────────────────────────────┐
       ▼                                              ▼
┌──────────────────────┐              ┌───────────────────────────┐
│   Veritas.Corpora    │              │      Veritas.Rag API      │
│  Corpus CRUD         │              │  POST /api/rag/query      │
│  Document ingestion  │              │  POST /api/rag/search     │
│  Rights declaration  │              │  GET  /api/rag/documents  │
│  Indexing pipeline   │              │  corpus_id scoped         │
└──────┬───────────────┘              └──────────┬────────────────┘
       │                                         │
       ▼                                         ▼
┌──────────────────────┐              ┌───────────────────────────┐
│  Veritas.Extraction  │              │     IIndexBackend         │
│  MAF Agent Pipeline  │              │  AzureSearchIndexBackend  │
│  + DomainPacks       │              │  (pluggable)              │
└──────┬───────────────┘              └──────────┬────────────────┘
       │                                         │
       └──────────────────┬──────────────────────┘
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Azure Data Lake Gen2                          │
│  /raw/corpora/{corpus_id}/documents/{document_id}/              │
│  /extracted/corpora/{corpus_id}/{document_id}/                  │
│  /validated/corpora/{corpus_id}/{document_id}/                  │
│  /classified/corpora/{corpus_id}/{pack_id}/{version}/{doc_id}/  │
│  /experiments/{corpus_id}/{experiment_id}/                      │
│  /analysis/corpora/{corpus_id}/{pack_id}/{version}/             │
└─────────────────────────────────────────────────────────────────┘
```

### 1.5 Data Lake Zone Design

| Zone | Path | Pack-specific? | Written by |
|------|------|---------------|-----------|
| Raw | `/raw/corpora/{corpus_id}/documents/{doc_id}/` | No | Ingestion pipeline |
| Extracted | `/extracted/corpora/{corpus_id}/{doc_id}/` | No | ExtractionAgent |
| Validated | `/validated/corpora/{corpus_id}/{doc_id}/` | No | Human validation |
| Classified | `/classified/corpora/{corpus_id}/{pack_id}/{version}/{doc_id}/` | Yes | ClassificationAgent |
| Experiments | `/experiments/{corpus_id}/{experiment_id}/` | No | Experiment API |
| Analysis | `/analysis/corpora/{corpus_id}/{pack_id}/{version}/` | Yes | Python notebooks |

Raw through validated zones are immutable once written. Pack-specific zones keyed by `pack_id` + `pack_version`.

---

## 2. Phase 0 — Foundation, Pack Schema, and Skills

### 2.1 Repository Setup

**FR-0.1** Create `intentive-labs/veritas` (public) by forking gh-maf-template. Create `intentive-labs/veritas-rag` (public). Create `cill-ab/lenr-pack` (private).

**FR-0.2** Both public repos pass gh-maf-template CI before new code is added.

**FR-0.3** Directory structure:

```
veritas/
├── .github/
│   ├── agents/
│   └── skills/
├── domain-pack-schema/
│   ├── schema-v1/
│   └── examples/lenr-pack-example/
├── src/
│   ├── Veritas.Core/
│   ├── Veritas.Corpora/
│   ├── Veritas.DomainPacks/
│   ├── Veritas.Extraction/
│   ├── Veritas.Storage/
│   └── Veritas.Api/
├── analysis/
├── frontend/
├── mock-data/             ← sample documents and mock corpora for dev/test
└── infrastructure/

veritas-rag/
├── .github/skills/
├── src/
│   ├── Veritas.Rag/
│   └── Lenr.Rag/
├── samples/
├── HOSTING.md
└── infrastructure/
```

`mock-data/` contains sample documents and mock corpora for development and testing. No real LENR documents. No Cill AB data.

### 2.2 Domain Pack Schema v1.0

**FR-0.4** JSON Schema files in `domain-pack-schema/schema-v1/`:
- `manifest.schema.json`
- `ontology.schema.json`
- `classification.schema.json`
- `validation.schema.json`
- `normalization.schema.json`
- `hypothesis.schema.json`

**FR-0.5** `manifest.schema.json` enforces:
```json
{
  "pack_id":        { "type": "string", "pattern": "^[a-z0-9-]+$" },
  "version":        { "type": "string", "pattern": "^\\d+\\.\\d+\\.\\d+$" },
  "name":           { "type": "string" },
  "field":          { "type": "string" },
  "owner":          { "type": "string" },
  "status":         { "enum": ["official", "experimental", "community"] },
  "assumptions":    { "type": "array", "minItems": 1 },
  "ignores":        { "type": "array", "minItems": 1 },
  "source":         { "type": "string" },
  "schema_version": { "type": "string" }
}
```

**FR-0.6** Generic LENR example pack in `domain-pack-schema/examples/lenr-pack-example/` — placeholder values only. No Cill AB data.

### 2.3 Veritas.DomainPacks Runtime (C#)

**FR-0.7** `IDomainPackRuntime` interface:
```csharp
public interface IDomainPackRuntime
{
    Task<DomainPack> LoadPackAsync(string packPath);
    ValidationResult ValidatePack(DomainPack pack);
    ClassificationResult Classify(ExtractedParameters parameters, DomainPack pack);
    NormalizationResult Normalize(RawParameters parameters, DomainPack pack);
    bool ValidateParameters(ExtractedParameters parameters, DomainPack pack,
                            out List<ValidationIssue> issues);
}
```

**FR-0.8** Runtime validates pack against schema before use. Invalid packs rejected with field-level errors.

**FR-0.9** Runtime is completely field-agnostic.

### 2.4 Skills

**FR-0.10** Core engine skills in `veritas/.github/skills/`:

#### `corpus-ingestion-rules.skill`
- Supported document formats and how each is processed
- Rights declaration values and their meaning
- What `index_status` transitions mean
- How document_id is generated and guaranteed unique within corpus

#### `content-understanding-schema.skill`
- Azure Content Understanding API contract
- Base extraction schema, confidence interpretation, fallback behaviour

#### `normalization-rules.skill`
- General unit normalisation, range handling, `unit_unknown` usage

#### `domain-pack-schema.skill`
- How agents interact with domain pack files
- How to tag all outputs with `pack_id` + `pack_version`

**FR-0.11** RAG skill in `veritas-rag/.github/skills/`:

#### `rag-plugin-contract.skill`
- `IRagPlugin` contract, corpus scoping
- Grounded answer requirements, citation format
- Refusal behaviour when evidence weak

**FR-0.12** Cill AB pack skills in `cill-ab/lenr-pack/.github/skills/` — authored during Pekka ontology session:
- `lenr-parameter-ontology.skill` **[PEKKA REQUIRED]**
- `lenr-outcome-classification.skill` **[PEKKA REQUIRED]**
- `lenr-validation-rules.skill` **[PEKKA REQUIRED]**
- `lenr-experiment-schema.skill` **[PEKKA REQUIRED]**

---

## 3. Phase 1 — Private Corpus Knowledge Engine

Phase 1 delivers the complete end-to-end flow for a private user-provided corpus: create, upload, index, query.

### 3.1 Corpus Management (C#)

**FR-1.1** `POST /api/corpora` creates a new corpus:
```json
{
  "name": "string",
  "source_type": "user_upload | api_import | connector",
  "description": "string | null"
}
```
Returns `corpus_id` (system-generated slug from name + random suffix).

**FR-1.2** `GET /api/corpora` lists all corpora for authenticated user.

**FR-1.3** `GET /api/corpora/{corpus_id}` returns corpus metadata including `index_status` and `document_count`.

**FR-1.4** `DELETE /api/corpora/{corpus_id}` deletes corpus and all associated documents, extractions, and index entries. Irreversible. Requires explicit confirmation parameter.

**FR-1.5** Corpus `index_status` state machine:
```
pending → indexing → ready | error
```

---

### 3.2 Document Ingestion (C#)

**FR-1.6** `POST /api/corpora/{corpus_id}/documents` accepts multipart/form-data with:
- `file` — document file (required)
- `rights_declaration` — one of the six values (required)
- `title` — override extracted title (optional)
- `metadata` — JSON object of additional metadata (optional)

**FR-1.7** Supported formats v1: `.pdf`, `.docx`, `.md`, `.txt`, `.html`.

**FR-1.8** Unsupported formats SHALL be rejected with HTTP 415 and a list of supported formats.

**FR-1.9** Each uploaded document SHALL be assigned a `document_id` (UUID) unique within the corpus.

**FR-1.10** The original file SHALL be stored unmodified in `/raw/corpora/{corpus_id}/documents/{document_id}/original.{ext}` immediately on upload.

**FR-1.11** A metadata sidecar SHALL be written alongside the original:
```json
{
  "document_id": "string",
  "corpus_id": "string",
  "original_filename": "string",
  "format": "string",
  "file_size_bytes": "integer",
  "sha256_hash": "string",
  "rights_declaration": "string",
  "uploaded_at": "ISO8601",
  "uploaded_by": "string",
  "title_override": "string | null",
  "user_metadata": {}
}
```

**FR-1.12** Upload SHALL be idempotent per corpus — re-uploading the same file (SHA256 match) to the same corpus returns the existing `document_id` with HTTP 200.

**FR-1.13** `GET /api/corpora/{corpus_id}/documents` lists all documents in corpus with metadata and processing status.

**FR-1.14** `GET /api/corpora/{corpus_id}/documents/{document_id}` returns full document metadata and processing history.

**FR-1.15** `DELETE /api/corpora/{corpus_id}/documents/{document_id}` removes document and all associated data from storage and index. Triggers corpus re-index.

**FR-1.16** Document processing status state machine:
```
uploaded → extracting → indexing → ready | error
```

---

### 3.3 Text Extraction Pipeline (C#)

**FR-1.17** After upload, each document enters text extraction automatically.

**FR-1.18** Extraction uses Azure Content Understanding with the base schema from `content-understanding-schema.skill`:
- Title, authors, year, abstract, institutions, document_type
- Confidence score per field

**FR-1.19** Full text extracted and stored in `/extracted/corpora/{corpus_id}/{document_id}/text.json`.

**FR-1.20** Documents where Content Understanding confidence <0.6 for title or abstract are flagged `needs_review`. User can manually correct title/metadata via `PATCH /api/corpora/{corpus_id}/documents/{document_id}`.

**FR-1.21** Extraction pipeline is resumable — documents already extracted are not reprocessed unless explicitly re-queued via `POST /api/corpora/{corpus_id}/documents/{document_id}/reprocess`.

---

### 3.4 Azure AI Search Index

**FR-1.22** Single index `veritas-documents` with `corpus_id` as a filterable field. All corpora share one index; corpus scoping via filter.

**FR-1.23** Index schema, chunking strategy, chunk size, overlap, scoring profiles, and vector configuration to be designed by search expert before implementation (see search expert session document).

**FR-1.24** Minimum required fields:

| Field | Type | Searchable | Filterable | Facetable |
|-------|------|-----------|-----------|----------|
| id | string | No | Yes | No |
| corpus_id | string | No | Yes | Yes |
| document_id | string | No | Yes | No |
| title | string | Yes | No | No |
| authors | collection(string) | Yes | Yes | Yes |
| year | int32 | No | Yes | Yes |
| abstract | string | Yes | No | No |
| full_text | string | Yes | No | No |
| document_type | string | No | Yes | Yes |
| rights_declaration | string | No | Yes | Yes |
| extraction_confidence | double | No | Yes | No |
| chunk_id | string | No | Yes | No |
| embedding | Collection(Single) | Yes | No | No |

**FR-1.25** Index backend accessed exclusively via `IIndexBackend` from `Veritas.Rag`. No direct Azure AI Search SDK calls outside that interface.

**FR-1.26** When a document is deleted, all its chunks are removed from the index.

---

### 3.5 Veritas RAG Service (veritas-rag repo)

**FR-1.27** `Veritas.Rag` defines all contracts — no implementations in this assembly:

```csharp
public interface IRagPlugin
{
    Task<RagResponse> QueryAsync(RagRequest request);
}

public record RagRequest(
    string Query,
    string CorpusId,
    Dictionary<string, string>? Filters,
    int TopK = 10
);

public record RagResponse(
    string Answer,
    string Confidence,
    IReadOnlyList<RagSource> Sources,
    string Disclaimer,
    bool IsRefused,
    string? RefusalReason
);

public interface ICorpusConnector
{
    string CorpusId { get; }
    CorpusConfig Config { get; }
    Task<bool> IsAvailableAsync();
}

public interface IIndexBackend
{
    Task<IReadOnlyList<SearchChunk>> SearchAsync(
        string query, string indexName,
        Dictionary<string, string>? filters, int topK);
}

public interface IRetriever
{
    Task<IReadOnlyList<RetrievedChunk>> RetrieveAsync(
        string query, ICorpusConnector corpus,
        int topK, Dictionary<string, string>? filters);
}

public interface IAnswerGenerator
{
    Task<GeneratedAnswer> GenerateAsync(
        string query, IReadOnlyList<RetrievedChunk> chunks,
        CorpusConfig config);
}

public interface ICitationValidator
{
    CitationValidationResult Validate(
        string answer, IReadOnlyList<RetrievedChunk> chunks);
}
```

**FR-1.28** `CorpusConfig` loaded from configuration files — not hardcoded:
```csharp
public record CorpusConfig
{
    public required string CorpusId { get; init; }
    public required string IndexName { get; init; }
    public required string ChunkingStrategy { get; init; }
    public required int ChunkSize { get; init; }
    public required int ChunkOverlap { get; init; }
    public required string EmbeddingModel { get; init; }
    public required string Disclaimer { get; init; }
    public bool IsPublic { get; init; }
    public Dictionary<string, string> DefaultFilters { get; init; } = new();
}
```

**FR-1.29** RAG API endpoints:
- `POST /api/rag/query` — accepts `{ corpus_id, question, filters?, top_k? }`, returns `RagResponse`
- `POST /api/rag/search` — keyword/semantic search, returns ranked document list
- `GET /api/rag/documents/{id}` — document metadata and available chunks

**FR-1.30** All endpoints require `corpus_id`. Queries are strictly scoped to the specified corpus.

**FR-1.31** `GroundedAnswerGenerator` refuses to answer when retrieved chunks do not support a response — sets `IsRefused = true` with reason. Never fabricates citations.

**FR-1.32** `CitationValidator` verifies every claim traces to at least one retrieved chunk.

**FR-1.33** `Lenr.Rag` implements `ICorpusConnector` for LENR corpus with `LenrSearchFilters` convenience presets. No classification logic. No ontology.

**FR-1.34** `HOSTING.md` documents how to deploy Veritas RAG and register a new corpus connector.

---

### 3.6 RAG Evaluation

**FR-1.35** RAG evaluation runs weekly via Azure AI Foundry on four dimensions:
- Relevance, Groundedness, Citation accuracy, Answerability

**FR-1.36** Every query logged to Application Insights: query, corpus_id, chunk IDs, latency, answer, IsRefused.

---

## 4. Phase 2 — Extraction Pipeline and Domain Pack Application

### 4.1 MAF Agent Pipeline (C#)

**FR-2.1** MAF multi-agent pipeline in `Veritas.Extraction`. Instructions in `.github/agents/`. Implementations in `src/Veritas.Extraction/Agents/`.

**FR-2.2** Agents reference skills. No domain knowledge in agent instructions.

**FR-2.3** The pipeline accepts `document_id`, `corpus_id`, and `pack_id` as input.

**FR-2.4** Agent definitions:

#### `orchestrator.md` / `OrchestratorAgent.cs`
- Coordinates: Extraction → Validation → Normalization → Classification
- Writes intermediate state after each step
- Retries per-step max 2 times before marking failed

#### `extraction.md` / `ExtractionAgent.cs`
- References: `content-understanding-schema.skill`, `domain-pack-schema.skill`
- Calls Content Understanding with pack ontology fields
- Returns raw JSON with confidence scores
- Does NOT interpret or normalise

#### `validation.md` / `ValidationAgent.cs`
- References: `domain-pack-schema.skill`
- Loads `validation.json` from active pack
- Returns issues with severity (error, warning, info)
- Does NOT correct values

#### `normalization.md` / `NormalizationAgent.cs`
- References: `normalization-rules.skill`, `domain-pack-schema.skill`
- Loads `normalization.json` from active pack
- Converts units to canonical form
- Sets `unit_unknown` where unit cannot be determined

#### `classification.md` / `ClassificationAgent.cs`
- References: `domain-pack-schema.skill`
- Loads `classification.json` from active pack
- Classifies outcome per pack rules
- Tags output with `pack_id` + `pack_version`

**FR-2.5** Intermediate results to `/extracted/corpora/{corpus_id}/{document_id}/step_{n}.json`.

**FR-2.6** Classification outputs to `/classified/corpora/{corpus_id}/{pack_id}/{version}/{document_id}.json`.

**FR-2.7** Processing status:
```
queued → extracting → validating → normalizing → classifying →
awaiting_human_review → validated → failed
```

**FR-2.8** `ContractCoverageValidator` extended to validate extraction output against active pack ontology.

---

### 4.2 Human Validation

**FR-2.9** `GET /api/corpora/{corpus_id}/validation/queue` — documents awaiting review, ordered by confidence ascending.

**FR-2.10** Validation UI: rendered document + extracted parameters + confidence + issues + Accept/Edit/Reject per field.

**FR-2.11** Corrections stored in `/validated/corpora/{corpus_id}/{document_id}/corrections.json`.

**FR-2.12** Validated documents re-indexed with `extraction_method: hybrid`.

---

### 4.3 Multi-Pack Comparison

**FR-2.13** `GET /api/corpora/{corpus_id}/compare?packs={pack_id_1},{pack_id_2}` — side-by-side classification comparison with agreement flag and parameter diff.

**FR-2.14** Disagreements filterable and exportable as CSV.

---

## 5. Phase 3 — Experiment Log

**FR-3.1** `POST /api/corpora/{corpus_id}/experiments` — accepts experiment record validated against `lenr-experiment-schema.skill`.

**FR-3.2** `hypothesis_version` references declared hypothesis in active pack.

**FR-3.3** Records in `/experiments/{corpus_id}/{experiment_id}/record.json`.

**FR-3.4** Measurements in `/experiments/{corpus_id}/{experiment_id}/measurements/`.

**FR-3.5** Experiments immutable. Corrections via `POST /api/corpora/{corpus_id}/experiments/{id}/corrections`.

**FR-3.6** Every record tagged `pack_id` + `pack_version` at submission.

**FR-3.7** `GET /api/corpora/{corpus_id}/experiments/{id}/similar` — N most similar documents via weighted Euclidean distance. Not LLM.

---

## 6. Phase 4 — Analysis, Hypothesis Testing and Dashboard

**FR-4.1** Python notebooks in `analysis/` per corpus + pack: correlation analysis, hypothesis testing, publication bias.

**FR-4.2** All statistics include n, 95% CI, bias warning. Tagged `corpus_id` + `pack_id` + `pack_version`.

**FR-4.3** `POST /api/corpora/{corpus_id}/hypothesis/test`:
```json
{
  "hypothesis_id": "string",
  "pack_id": "string",
  "pack_version": "string",
  "coverage": { "relevant": "integer", "total": "integer", "percent": "double" },
  "findings": { "supporting": "integer", "contradicting": "integer", "inconclusive": "integer" },
  "confidence": "high|medium|low",
  "bias_warning": "string | null",
  "disclaimer": "string"
}
```

**FR-4.4** `GET /api/corpora/{corpus_id}/hypothesis/compare?packs={id1},{id2}` — hypothesis results across packs.

**FR-4.5** Dashboard per corpus + active pack: document overview, hypothesis results, multi-pack comparison, experiments, findings with citations.

**FR-4.6** Dashboard PDF-exportable. Every page: *"All findings are correlational. Specific to corpus [{corpus_id}], pack [{pack_id}] v[{pack_version}] and its stated assumptions."*

---

## 7. Non-Functional Requirements

**FR-NFR-1** Search queries: ≤2 seconds p95.

**FR-NFR-2** RAG query: ≤15 seconds.

**FR-NFR-3** Document upload + storage: ≤10 seconds for files ≤50MB.

**FR-NFR-4** Extraction pipeline: ≥50 documents/hour.

**FR-NFR-5** Pack loading: ≤500ms from local filesystem.

**FR-NFR-6** All Azure resources protected by Azure Active Directory.

**FR-NFR-7** All corpus data in per-corpus containers in Data Lake with access restricted to corpus owner and Intentive Labs admins.

**FR-NFR-8** No user document content transmitted outside Azure OpenAI under Intentive Labs enterprise agreement.

**FR-NFR-9** Cill AB pack not cached or logged by core engine or RAG.

**FR-NFR-10** Agent pipeline steps logged with: start time, duration, status, error, `corpus_id`, `pack_id`, `pack_version`.

**FR-NFR-11** RAG queries logged with: query, corpus_id, chunk IDs, latency, answer, IsRefused flag.

**FR-NFR-12** Data Lake writes logged with: document_id, corpus_id, operation, zone, timestamp.

**FR-NFR-13** RAG + extraction evals run weekly in Azure AI Foundry.

**FR-NFR-14** All corpus data exportable as JSON/CSV within 24 hours.

---

## 8. Solution Structure

```
intentive-labs/veritas (public, MIT)
│
├── .github/
│   ├── agents/
│   │   ├── orchestrator.md
│   │   ├── extraction.md
│   │   ├── validation.md
│   │   ├── normalization.md
│   │   └── classification.md
│   ├── skills/
│   │   ├── corpus-ingestion-rules.skill    # NEW
│   │   ├── content-understanding-schema.skill
│   │   ├── normalization-rules.skill
│   │   └── domain-pack-schema.skill
│   └── workflows/
│       ├── template-validation.yml         # INHERITED
│       └── extraction-pipeline.yml
│
├── domain-pack-schema/
│   ├── schema-v1/
│   │   ├── manifest.schema.json
│   │   ├── ontology.schema.json
│   │   ├── classification.schema.json
│   │   ├── validation.schema.json
│   │   ├── normalization.schema.json
│   │   └── hypothesis.schema.json
│   └── examples/lenr-pack-example/
│
├── mock-data/                              # NEW — dev/test only, no real docs
│   ├── sample-documents/
│   └── mock-corpora/
│
├── src/
│   ├── Veritas.Core/                       # Shared domain models + contracts
│   │   ├── Models/
│   │   │   ├── Corpus.cs
│   │   │   ├── VeritasDocument.cs
│   │   │   ├── RightsDeclaration.cs
│   │   │   ├── ExtractedParameters.cs
│   │   │   ├── ClassifiedParameters.cs
│   │   │   └── ExperimentRecord.cs
│   │   └── Contracts/
│   │       ├── CorpusContract.cs
│   │       ├── DocumentContract.cs
│   │       ├── ExtractionContract.cs
│   │       ├── ClassificationContract.cs
│   │       ├── ExperimentContract.cs
│   │       └── HypothesisContract.cs
│   │
│   ├── Veritas.Corpora/                    # Corpus + document management
│   │   ├── CorpusService.cs
│   │   ├── DocumentIngestionService.cs
│   │   ├── TextExtractionService.cs
│   │   ├── IndexingService.cs
│   │   └── RightsDeclarationValidator.cs
│   │
│   ├── Veritas.DomainPacks/                # Domain pack runtime
│   │   ├── IDomainPackRuntime.cs
│   │   ├── DomainPackLoader.cs
│   │   ├── DomainPackValidator.cs
│   │   └── Models/
│   │       ├── DomainPack.cs
│   │       ├── PackManifest.cs
│   │       ├── PackOntology.cs
│   │       ├── PackClassification.cs
│   │       └── PackHypothesis.cs
│   │
│   ├── Veritas.Extraction/                 # MAF agent pipeline
│   │   ├── Agents/
│   │   │   ├── OrchestratorAgent.cs
│   │   │   ├── ExtractionAgent.cs
│   │   │   ├── ValidationAgent.cs
│   │   │   ├── NormalizationAgent.cs
│   │   │   └── ClassificationAgent.cs
│   │   └── Pipeline/
│   │       ├── ExtractionQueue.cs
│   │       └── PipelineStatus.cs
│   │
│   ├── Veritas.Storage/                    # Data Lake abstraction
│   │   ├── IDocumentStore.cs
│   │   ├── DataLakeDocumentStore.cs
│   │   └── StoragePaths.cs                 # Centralised path builder
│   │
│   └── Veritas.Api/                        # ASP.NET Core REST API
│       └── Controllers/
│           ├── CorporaController.cs
│           ├── DocumentsController.cs
│           ├── ValidationController.cs
│           ├── ExperimentsController.cs
│           ├── HypothesisController.cs
│           └── AnalysisController.cs
│
├── analysis/
│   ├── correlation_analysis.ipynb
│   ├── hypothesis_testing.ipynb
│   ├── publication_bias.ipynb
│   └── requirements.txt
│
├── frontend/
│   └── src/components/
│       ├── CorpusManager/                  # NEW
│       ├── DocumentUpload/                 # NEW
│       ├── Search/
│       ├── Validation/
│       ├── PackCompare/
│       ├── HypothesisTesting/
│       └── Dashboard/
│
└── infrastructure/
    ├── main.bicep
    └── parameters.json

─────────────────────────────────────────────────────────────────
intentive-labs/veritas-rag (public, MIT)
│
├── .github/skills/
│   └── rag-plugin-contract.skill
├── src/
│   ├── Veritas.Rag/                        # Generic RAG core
│   │   ├── Contracts/
│   │   │   ├── IRagPlugin.cs
│   │   │   ├── ICorpusConnector.cs
│   │   │   ├── IIndexBackend.cs
│   │   │   ├── IRetriever.cs
│   │   │   ├── IAnswerGenerator.cs
│   │   │   └── ICitationValidator.cs
│   │   ├── Models/
│   │   │   ├── RagRequest.cs
│   │   │   ├── RagResponse.cs
│   │   │   ├── CorpusConfig.cs
│   │   │   └── RetrievedChunk.cs
│   │   ├── Implementation/
│   │   │   ├── RagPipeline.cs
│   │   │   ├── RagRetriever.cs
│   │   │   ├── GroundedAnswerGenerator.cs
│   │   │   ├── CitationValidator.cs
│   │   │   └── AzureSearchIndexBackend.cs
│   │   └── Api/
│   │       └── RagController.cs
│   │
│   └── Lenr.Rag/                           # LENR corpus connector
│       ├── LenrCorpusConnector.cs
│       └── LenrSearchFilters.cs
│
├── samples/lenr-corpus-config/
├── HOSTING.md
└── infrastructure/

─────────────────────────────────────────────────────────────────
cill-ab/lenr-pack (private)
│
├── manifest.json
├── ontology.json
├── classification.json
├── validation.json
├── normalization.json
├── hypotheses/magnetic-field-v1.json
├── .github/skills/
│   ├── lenr-parameter-ontology.skill
│   ├── lenr-outcome-classification.skill
│   ├── lenr-validation-rules.skill
│   └── lenr-experiment-schema.skill
└── CHANGELOG.md
```

---

## 9. Azure Services Required

| Service | Repo | Purpose | Phase |
|---------|------|---------|-------|
| Azure Data Lake Gen2 | veritas | Document and corpus storage | 1 |
| Azure Content Understanding | veritas | Document text extraction | 1 |
| Azure AI Search | veritas-rag | Hybrid corpus index | 1 |
| Azure OpenAI (GPT-4o) | veritas-rag | RAG generation | 1 |
| Azure Static Web Apps | veritas | React frontend | 1 |
| Azure App Service | veritas | Core API | 1 |
| Azure App Service | veritas-rag | RAG API | 1 |
| Azure Application Insights | both | Observability | 1 |
| Azure AI Foundry | both | Extraction + RAG evals | 2 |
| Azure Machine Learning | veritas | Python analysis notebooks | 4 |
| Azure Key Vault | both | Secret management | 1 |

---

## 10. Copilot Agent Mode — Implementation Order

| Step | Target | Repo | Depends on | FR |
|------|--------|------|-----------|-----|
| 1 | Create repos, fork gh-maf-template, directory structure | all | — | FR-0.1–0.3 |
| 2 | Domain Pack schema v1.0 | veritas | Step 1 | FR-0.4–0.6 |
| 3 | `corpus-ingestion-rules.skill` | veritas | Step 1 | FR-0.10 |
| 4 | `content-understanding-schema.skill` | veritas | Step 1 | FR-0.10 |
| 5 | `normalization-rules.skill` | veritas | Step 1 | FR-0.10 |
| 6 | `domain-pack-schema.skill` | veritas | Step 2 | FR-0.10 |
| 7 | `rag-plugin-contract.skill` | veritas-rag | Step 1 | FR-0.11 |
| 8 | `Veritas.Rag` contracts and models | veritas-rag | Step 7 | FR-1.27 |
| 9 | `Veritas.DomainPacks` runtime | veritas | Step 2 | FR-0.7–0.9 |
| 10 | `Veritas.Core` models and contracts | veritas | Step 9 | FR-1.1 |
| 11 | `StoragePaths.cs` — centralised path builder | veritas | Step 10 | FR-1.5 |
| 12 | `Veritas.Storage` Data Lake client | veritas | Step 11 | FR-1.10–1.11 |
| 13 | `Veritas.Corpora` — corpus CRUD | veritas | Step 12 | FR-1.1–1.5 |
| 14 | `Veritas.Corpora` — document ingestion | veritas | Step 13 | FR-1.6–1.16 |
| 15 | `Veritas.Corpora` — text extraction service | veritas | Step 14 | FR-1.17–1.21 |
| 16 | **[SEARCH EXPERT]** AI Search index design | veritas-rag | Step 8 | FR-1.22–1.26 |
| 17 | `AzureSearchIndexBackend` : `IIndexBackend` | veritas-rag | Step 16 | FR-1.25 |
| 18 | `Veritas.Corpora` — indexing service | veritas | Steps 15, 17 | FR-1.22–1.26 |
| 19 | `RagRetriever` + `GroundedAnswerGenerator` + `CitationValidator` | veritas-rag | Step 17 | FR-1.27–1.32 |
| 20 | `RagPipeline` : `IRagPlugin` | veritas-rag | Step 19 | FR-1.27 |
| 21 | `LenrCorpusConnector` + `LenrSearchFilters` | veritas-rag | Step 20 | FR-1.33 |
| 22 | `RagController` (3 endpoints) | veritas-rag | Step 20 | FR-1.29 |
| 23 | `Veritas.Api` — Corpora + Documents controllers | veritas | Steps 13–14 | FR-1.1–1.16 |
| 24 | React Corpus Manager + Document Upload UI | veritas | Step 23 | — |
| 25 | React Search UI (queries veritas-rag) | veritas | Step 22 | — |
| 26 | `HOSTING.md` | veritas-rag | Step 22 | FR-1.34 |
| 27 | **[PEKKA SESSION]** Author 4 Cill AB pack skills | lenr-pack | Steps 3–6 | FR-0.12 |
| 28 | `ExtractionAgent.cs` + `extraction.md` | veritas | Steps 9, 10 | FR-2.4 |
| 29 | `ValidationAgent.cs` + `validation.md` | veritas | Step 28 | FR-2.4 |
| 30 | `NormalizationAgent.cs` + `normalization.md` | veritas | Step 28 | FR-2.4 |
| 31 | `ClassificationAgent.cs` + `classification.md` | veritas | Step 28 | FR-2.4 |
| 32 | `OrchestratorAgent.cs` + `orchestrator.md` | veritas | Steps 28–31 | FR-2.1 |
| 33 | `ExtractionQueue` + pipeline status | veritas | Step 32 | FR-2.5–2.7 |
| 34 | `Veritas.Api` — Validation controller | veritas | Step 33 | FR-2.9 |
| 35 | React Validation UI | veritas | Step 34 | — |
| 36 | Multi-pack compare endpoint | veritas | Step 33 | FR-2.13 |
| 37 | React Pack Compare UI | veritas | Step 36 | — |
| 38 | `Veritas.Api` — Experiments controller | veritas | Step 10 | FR-3.1–3.7 |
| 39 | React Experiment Log UI | veritas | Step 38 | — |
| 40 | Python analysis + hypothesis notebooks | veritas | Step 33 | FR-4.1–4.2 |
| 41 | `Veritas.Api` — Hypothesis controller | veritas | Step 40 | FR-4.3–4.4 |
| 42 | React Hypothesis Testing + Dashboard | veritas | Steps 40–41 | FR-4.5–4.6 |
| 43 | Bicep templates (veritas + veritas-rag) | both | All | — |

---

## 11. Acceptance Criteria by Phase

### Phase 0
- [ ] Three repos created, CI passing
- [ ] Domain Pack schema v1.0 in `domain-pack-schema/schema-v1/`
- [ ] `Veritas.DomainPacks` runtime loads and validates a pack
- [ ] `Veritas.Rag` contracts defined
- [ ] All core skills authored
- [ ] `rag-plugin-contract.skill` authored
- [ ] Pekka ontology session completed
- [ ] Cill AB pack skills authored

### Phase 1
- [ ] A private user-provided corpus can be created, populated with documents, indexed, queried and reprocessed with full provenance
- [ ] All five document formats accepted and processed
- [ ] Rights declaration logged on every document
- [ ] `RagPipeline` returns grounded answers scoped to corpus
- [ ] `LenrCorpusConnector` registered and functional
- [ ] All RAG endpoints responding
- [ ] Corpus Manager and Document Upload UI functional
- [ ] `HOSTING.md` complete

### Phase 2
- [ ] Extraction pipeline processes documents through all four agents
- [ ] Pack runtime correctly loads Cill AB pack
- [ ] Classifications stored in correct corpus-scoped zone
- [ ] Human validation interface functional
- [ ] Multi-pack comparison endpoint functional

### Phase 3
- [ ] Experiment submission validates against Cill AB pack schema
- [ ] `hypothesis_version` references declared hypothesis in pack
- [ ] Experiment comparison returns similar corpus documents

### Phase 4
- [ ] Hypothesis testing endpoint functional
- [ ] Multi-pack comparison UI shows agreement/disagreement
- [ ] Analysis notebooks produce bias-corrected statistics
- [ ] Dashboard renders with corpus + pack reference on every finding
- [ ] PDF export functional
- [ ] KTH hosting conversation initiated
