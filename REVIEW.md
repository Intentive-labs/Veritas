# Veritas — Implementation Review

> Last updated: 2026-04-26
> Status: scaffold complete, not production-ready

This document records the critical review of the Veritas platform as built.
Use it as a starting checklist for the next development session.

---

## Overall Assessment

The architecture is correct and the right abstractions are in place.
The critical path to a working system runs through: real persistence → real LLM calls → automated tests.
Everything else is polish.

---

## Layer Status

| Layer | State | Notes |
|---|---|---|
| API surface | ✅ Complete shape | 7 controllers, all endpoints stubbed |
| Extraction pipeline | ⚠️ Wired, no real AI calls | Agents are stubs; requires physicist + LLM session |
| RAG pipeline | ⚠️ Wired, no real index or LLM | `MockIndexBackend` + `GroundedAnswerGenerator` are stubs |
| Persistence | ❌ All in-memory | Resets on restart; no real Data Lake or Cosmos |
| Auth | ⚠️ Stub — no `[Authorize]` on controllers | Dev skips all validation; prod config missing |
| Tests | ❌ None | Template tests deleted; no Veritas tests written |
| UI | ✅ Good structure, thin error handling | No error boundaries, no loading-state guards |
| Infrastructure | ✅ Bicep exists, not deployable yet | Needs real tenant/client IDs and role GUIDs |

---

## Critical Issues (fix before first real use)

### 1. No real persistence
`DataLakeDocumentStore`, `MockIndexBackend`, and all Cosmos stubs are in-memory.
The app resets on restart. Ingesting and retrieving a document end-to-end does not work today.

**Next step:** Implement `DataLakeDocumentStore` using `Azure.Storage.Files.DataLake` and
wire `AzureSearchIndexBackend` using `Azure.Search.Documents` once the search expert
designs the chunking/embedding schema.

### 2. `ICorpusConnector` is a singleton wired to one corpus
```csharp
// Program.cs
builder.Services.AddSingleton<ICorpusConnector>(_ =>
    LenrCorpusConnector.CreateMock("default-corpus"));
```
Every API call uses the same hardcoded corpus. Multi-corpus support (advertised in PRD)
is broken at the DI level.

**Next step:** Replace with a factory (`ICorpusConnectorFactory`) keyed by corpus ID,
resolved from the authenticated user's active corpus in the request context.

### 3. No automated tests
The extraction pipeline has retry logic, coverage validation, and citation validation —
none of it is verified. This is high-risk for a system whose core value is scientific integrity.

**Next step:** Create `tests/Veritas.Tests/` and cover at minimum:
- `OrchestratorAgent` retry and intermediate-write behaviour
- `ContractCoverageValidator` severity levels
- `CitationValidator` grounding checks
- `CorpusService` create/delete/index-status transitions

### 4. `ContractCoverageValidator` is not wired into the pipeline
The file exists at `src/Veritas.Extraction/ContractCoverageValidator.cs`
but `OrchestratorAgent` never calls it. Extraction output is never validated against
the pack ontology at runtime.

**Next step:** Call `ContractCoverageValidator.Validate(extracted, pack)` after step 1
(extraction) and before step 2 (validation). Surface gaps in the `ValidationResult`.

---

## High Issues (fix before beta)

### 5. Auth: no `[Authorize]` attributes on controllers
JWT validation is skipped in dev and misconfigured in prod. Any caller can reach any
endpoint. Controllers should declare `[Authorize]` explicitly; sensitive mutation endpoints
should additionally scope to roles.

### 6. RAG pipeline makes no real LLM calls
`GroundedAnswerGenerator` returns a mock response. The retrieval → grounding → citation
pipeline is architecturally correct but produces no real answers.

**Next step:** Wire `Azure.AI.OpenAI` (or `Microsoft.Extensions.AI`) into
`GroundedAnswerGenerator` with a grounded system prompt that enforces corpus-only answers.

### 7. React UI has no error boundaries
A failed API call will crash the component subtree instead of showing a user-friendly message.
Pages with no data returned silently show blank content.

**Next step:** Add a top-level `<ErrorBoundary>` in `App.tsx` and per-page loading/error
state guards (the API client already returns typed errors — consume them).

---

## Medium Issues (fix before production)

### 8. CORS hardcoded to `localhost:5173`
```csharp
policy.WithOrigins("http://localhost:5173")
```
**Next step:** Read from config key `Veritas:AllowedOrigins` so staging/prod URLs
can be set via environment variable.

### 9. Bicep Cosmos role ID is unverified
`cosmosContributorRole` uses `00000000-0000-0000-0000-000000000002`. Built-in Cosmos
role GUIDs differ by API type. Verify in target subscription before deploying.

### 10. Extraction agents make no AI calls
All five agents (`ExtractionAgent`, `ValidationAgent`, `NormalizationAgent`,
`ClassificationAgent`, `OrchestratorAgent`) are stubs. The real value — using an LLM
to extract structured parameters from PDFs — is not implemented.
Blocked on physicist session for field definitions + LLM prompt design.

---

## Strengths to Preserve

- **Domain Pack concept** — decoupling ontology from agent logic is correct and extensible.
- **`[MOCK]` annotation discipline** — every stub says exactly what to replace it with.
- **RAG refusal logic** — refuses when evidence is insufficient rather than hallucinating.
- **`ContractCoverageValidator` design** — Error/Warning/Info severity + field-level gaps
  is the right shape; just needs to be wired in.
- **Pipeline is resumable** — intermediate state is written after each step; safe to retry
  from any point.

---

## Blocked Items (external session required)

| Item | Blocked on |
|---|---|
| `AzureSearchIndexBackend` | Search expert: chunking strategy, embedding model, index schema |
| LENR extraction prompts | Physicist: real field names, filter definitions, ontology content |
| `LenrSearchFilters` real filters | Physicist session |
| Real AAD config | Azure subscription: tenant ID, client ID, app registration |
| `infrastructure/parameters.json` real values | Azure subscription |
| `VERITAS_API_URL` / `VERITAS_API_KEY` secrets | GitHub repo settings |

---

## Suggested Priority Order for Next Session

1. Wire `ContractCoverageValidator` into `OrchestratorAgent` (30 min, no blockers)
2. Add `tests/Veritas.Tests/` with unit tests for Orchestrator + CoverageValidator (2 h, no blockers)
3. Implement `ICorpusConnectorFactory` to fix multi-corpus DI (1 h, no blockers)
4. Add `[Authorize]` to all controllers; read CORS from config (1 h, no blockers)
5. Implement real `DataLakeDocumentStore` (needs Azure Storage connection string) 
6. Design Azure AI Search index schema with search expert; implement `AzureSearchIndexBackend`
7. Wire `GroundedAnswerGenerator` to a real LLM endpoint
8. Add React error boundaries and loading guards
