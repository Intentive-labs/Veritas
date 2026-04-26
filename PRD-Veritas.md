# Product Requirements Document (PRD)
## Veritas
**Version:** 1.4  
**Date:** April 2026  
**Status:** Draft – Copilot Agent Mode Ready

---

## 1. Background and Context

### 1.1 Problem Statement

Scientific research fields with reproducibility problems share a common structural challenge: no shared, queryable infrastructure exists for systematically comparing experimental parameters and outcomes across published literature. Researchers cannot ask: *"Which experiments controlled for variable X, and what were the outcomes?"*

Individual researchers and institutions lack tools to build structured, AI-queryable knowledge bases from their own document collections — without exposing those documents publicly or violating copyright.

### 1.2 What Veritas Is

Veritas is a **private corpus intelligence platform**. Users bring their own documents. Veritas indexes them, makes them queryable via RAG, and optionally applies domain packs for field-specific parameter extraction and hypothesis testing.

The platform separates four concerns:

1. **Veritas.Core** — corpus management, document ingestion, storage, provenance (field-agnostic)
2. **Veritas.Rag** — standalone hostable RAG service, corpus-scoped queries (field-agnostic)
3. **Veritas.Corpora** — corpus CRUD, document upload, rights declaration, indexing pipeline
4. **Veritas.DomainPacks** — domain pack runtime and schema (field-agnostic)
5. **DomainPacks.LenrMagneticField** — first domain pack implementation (customer, private)

**There is no default corpus and no bundled data.** The open source repo contains code, schemas, interfaces, sample documents, and mock corpora only.

### 1.3 First Implementation: customer / LENR

customer holds a patent describing magnetic field configuration as the controlling variable for LENR reproducibility, currently under peer review in JCMNS. customer will:

1. Upload their own curated collection of LENR publications to a private corpus
2. Apply the `DomainPacks.LenrMagneticField` pack for parameter extraction and hypothesis testing
3. Log their own experiments against the same corpus

No crawling. No public data. No copyright risk.

### 1.4 Crawler as Optional Connector (Future)

A crawler connector may be added in a future phase as an optional ingestion source. It is not part of the core platform and will never be a default. Users who choose to use it are responsible for rights compliance.

### 1.5 Open Source Strategy

| Layer | Repo | Licence | Contains |
|-------|------|---------|----------|
| Core engine | `intentive-labs/veritas` | MIT | Code, schemas, interfaces, mocks |
| Veritas RAG | `intentive-labs/veritas-rag` | MIT | Code, schemas, interfaces, samples |
| Domain Pack schema | `intentive-labs/veritas` | MIT | Schema, generic LENR example |
| **customer LENR pack** | `customer/lenr-pack` | **Private** | physicist's rules and thresholds |

Repos contain no LENR fulltext, no physicist's private material, no validated private data.

### 1.6 Users

| User | Role | Primary Need |
|------|------|--------------|
| physicist | Physicist, customer | Upload papers, query corpus, log experiments |
| Engineer | Intentive Labs | Manage infrastructure, maintain platform |
| Research groups | External | Build private queryable knowledge bases |
| Institutional hosts (e.g. KTH) | External | Host Veritas RAG for their own corpus |

---

## 2. Goals and Non-Goals

### 2.1 Goals

**Veritas.Core + Veritas.Corpora**
- Create and manage private corpora
- Upload documents (PDF, Word, Markdown, text, HTML)
- Declare rights at upload time — logged for provenance
- Extract text and structured parameters
- Index corpus for hybrid search and RAG
- Full provenance: every document, extraction step, and decision traceable

**Veritas.Rag**
- Corpus-scoped natural language querying
- Grounded answers with citations — refuse when evidence weak
- Configurable per corpus (chunking, embedding, filters, disclaimer)
- Designed for standalone institutional hosting

**Veritas.DomainPacks**
- Apply pluggable domain packs for field-specific parameter extraction
- Multi-pack comparison against same corpus
- Hypothesis testing with coverage, confidence, bias detection

### 2.2 Non-Goals

- Veritas does not crawl public websites by default
- Veritas does not host or bundle any document corpus
- Veritas does not perform physics simulations
- Veritas does not make causal claims
- Veritas does not validate copyright compliance — it logs rights declarations

---

## 3. Corpus Model

### 3.1 Core Corpus Entity

```json
{
  "corpus_id": "string (slug, immutable)",
  "name": "string",
  "owner": "string",
  "visibility": "private",
  "source_type": "user_upload | api_import | connector",
  "rights_declaration": "see 3.2",
  "created_at": "ISO8601",
  "updated_at": "ISO8601",
  "document_count": "integer",
  "index_status": "pending | indexing | ready | error"
}
```

`visibility` is `private` in v1. Public corpora are a future concern.

### 3.2 Rights Declaration

Every document upload requires a rights declaration. This is logged for provenance — not a legal guarantee.

| Value | Meaning |
|-------|---------|
| `own_content` | Uploader owns this content |
| `permission_granted` | Explicit permission from rights holder |
| `licensed_for_private_use` | Licensed for private/internal use |
| `public_domain` | Content is in the public domain |
| `open_access` | Published under open access terms |
| `unknown_needs_review` | Rights unclear — flagged for review |

### 3.3 Document Ingestion

Supported formats in v1:
- PDF
- Word (.docx)
- Markdown (.md)
- Plain text (.txt)
- HTML

Future: CSV, JSON structured data.

---

## 4. Domain Pack Architecture

### 4.1 What is a Domain Pack?

A Domain Pack is a versioned, declarative configuration that tells Veritas how to interpret documents in a corpus:

```
domain-pack/
├── manifest.json          ← identity, version, owner, status, assumptions, ignores
├── ontology.json          ← parameter definitions, types, canonical values
├── classification.json    ← outcome classification rules and thresholds
├── validation.json        ← physical plausibility rules
├── normalization.json     ← unit conversions and terminology mappings
├── hypotheses/
│   └── hypothesis-v1.json
└── CHANGELOG.md
```

### 4.2 Separation of Extraction and Interpretation

```
Document
    ↓
Extraction (once, field-agnostic)
    ↓
Validated Parameters (stored, immutable)
    ↓      ↓      ↓
 Pack A  Pack B  Pack C
```

### 4.3 RAG and Domain Pack separation

Veritas RAG finds and answers. Domain packs interpret and classify. Zero domain logic in RAG.

### 4.4 Domain Pack Governance

| Rule | Detail |
|------|--------|
| Anyone can author | No approval required |
| Status self-declared | `official`, `experimental`, `community` |
| Versioning mandatory | Semver + CHANGELOG |
| Breaking changes = major version | Classification changes are breaking |
| Old results preserved | New version does not overwrite |
| Pack ID immutable | Never changes once published |

---

## 5. Success Metrics

| Metric | Target | Timeframe |
|--------|--------|-----------|
| Private corpus creation | User can create corpus, upload docs, index, query | Phase 1 complete |
| Document formats supported | PDF, Word, Markdown, text, HTML | Phase 1 complete |
| Rights declaration logged | Every document has rights declaration | Phase 1 complete |
| RAG corpus-scoped query | Answers grounded in corpus with citations | Phase 1 complete |
| Domain Pack schema published | Open, documented, versioned | Phase 2 complete |
| customer pack v1.0 complete | All required fields authored by physicist | Phase 2 complete |
| Parameter extraction working | >80% of customer documents ≥1 structured parameter | Phase 2 complete |
| Multi-pack comparison | ≥2 packs against same corpus | Phase 4 complete |
| Experiment logging | customer experiments fully logged | Phase 3 complete |
| KTH hosting conversation | Initiated | Phase 4 complete |

---

## 6. Foundation: gh-maf-template

Veritas is built on **gh-maf-template**, Intentive Labs' engineering harness for agentic products.

### 6.1 Inherited

| Component | Role |
|---|---|
| `.github/agents/` structure | Extraction agents |
| MAF runtime (C#) | Orchestrator and agent execution |
| OpenTelemetry / App Insights | Observability |
| Azure AI Foundry eval integration | Extraction and RAG quality evals |
| ContractCoverageValidator | Validates extraction against pack schema |
| EffectiveRiskLevel | Confidence classification |
| CI/CD pipeline | Extended, not replaced |

### 6.2 New in Veritas

| Component | Repo | Description |
|---|---|---|
| Veritas.Corpora | veritas | Corpus CRUD, document ingestion, rights |
| Veritas.DomainPacks | veritas | Domain pack runtime and schema |
| Veritas.Rag | veritas-rag | Standalone hostable RAG service |
| Azure Content Understanding | veritas | PDF/document parameter extraction |
| Azure Data Lake Gen2 | veritas | Multi-zone corpus storage |
| C# API | veritas | REST API for all operations |
| React frontend | veritas | Corpus mgmt, upload, search, dashboard |
| Python analysis notebooks | veritas | Bias-corrected statistical analysis |

---

## 7. Skills Required

| Skill | Location | physicist Input |
|-------|----------|------------|
| `content-understanding-schema.skill` | Core | No |
| `normalization-rules.skill` | Core | No |
| `domain-pack-schema.skill` | Core | No |
| `corpus-ingestion-rules.skill` | Core | No |
| `rag-plugin-contract.skill` | veritas-rag | No |
| `lenr-parameter-ontology.skill` | customer pack | **YES** |
| `lenr-outcome-classification.skill` | customer pack | **YES** |
| `lenr-validation-rules.skill` | customer pack | **YES** |
| `lenr-experiment-schema.skill` | customer pack | **YES** |

---

## 8. Phased Delivery

### Phase 0 — Foundation (Week 1)
Fork gh-maf-template. Define Domain Pack schema. Author core skills. Schedule physicist session.

### Phase 1 — Private Corpus Knowledge Engine (Weeks 2–5)
Create corpus, upload documents, extract text, index, query with citations. No crawler. User-provided documents only.

**Phase 1 exit criteria:** A private user-provided corpus can be created, populated, indexed, queried and reprocessed with full provenance.

### Phase 2 — Extraction Pipeline + customer Pack v1.0 (Weeks 5–9)
MAF agent parameter extraction. Author customer domain pack. Human validation. **Requires physicist session.**

### Phase 3 — Experiment Log (Weeks 9–11)
Structured experiment logging. Comparison against corpus.

### Phase 4 — Analysis, Multi-Pack and Hypothesis Testing (Weeks 11–15)
Correlation analysis, multi-pack comparison, hypothesis testing. Export-ready for ICCF27. Initiate KTH conversation.

### Phase 5 — Open Source Launch (Post Phase 4)
Clean public repos, documentation, contribution guidelines, pack registry.

### Future — Crawler Connector (Optional)
A crawler connector may be added as an optional ingestion source. Not in core platform. Users responsible for rights compliance.

---

## 9. GitHub Copilot Agent Mode Strategy

### 9.1 What Copilot implements
- Corpus CRUD and document ingestion pipeline
- `Veritas.Rag` contracts and implementation
- Domain Pack runtime
- MAF agent extraction pipeline
- ASP.NET Core API controllers
- React frontend components
- Bicep infrastructure

### 9.2 What Copilot does NOT decide
- Domain Pack schema — Engineer
- customer pack content — physicist + Engineer
- RAG plugin contract — Engineer + search expert
- Chunking strategy — search expert
- Statistical methodology — Engineer

### 9.3 Sequenced prompts
Per FRD Section 9. Corpus ingestion implemented before extraction pipeline.

---

## 10. Constraints

- No bundled corpus data in any public repo
- Rights declaration required on every document upload
- Core engine + Veritas RAG: MIT licence, zero domain logic
- customer pack: private, customer IP
- All findings reference `pack_id` + `pack_version`
- Veritas RAG: grounded answers, must cite, must refuse when evidence weak
- Primary language: C# — Python only for statistical analysis
- gh-maf-template architecture layers must not be violated

---

## 11. Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Users upload copyrighted material | Medium | Medium | Rights declaration logged; platform not responsible |
| Content Understanding accuracy on user docs | Medium | High | Human validation workflow |
| Publication bias in analysis | High | Medium | Explicit bias flagging |
| physicist unavailable for ontology session | Medium | High | Blocks Phase 2 — schedule immediately |
| KTH not interested in hosting | Medium | Low | Other institutions can host |
| Copilot conflicts on shared surfaces | Medium | Medium | Sequenced prompts per FRD |

---

## 12. Dependencies

| Dependency | Owner | Blocks |
|-----------|-------|--------|
| gh-maf-template fork | Engineer | Everything |
| Domain Pack schema v1.0 | Engineer | Phase 2 |
| RAG plugin contract | Engineer + search expert | Phase 1 |
| Ontology session with physicist | Engineer + physicist | Phase 2 |
| Search expert: index + RAG design | Colleague | Phase 1 |
| Azure subscription | Intentive Labs | Phase 1 |

---

## 13. Open Questions

1. Should `corpus_id` be user-defined (slug) or system-generated (UUID)?
2. Should the pack registry be in the Veritas repo or `veritas-registry`?
3. Which Azure region minimises cost?
4. What is the minimum viable ICCF27 deliverable?
5. Should multi-pack comparison be Phase 4 or Phase 5?
6. Which index backends should Veritas RAG support beyond Azure AI Search in v1?
