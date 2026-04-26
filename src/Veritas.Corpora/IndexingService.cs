using Veritas.Storage;

namespace Veritas.Corpora;

// [MOCK] Indexing service stub — logs calls but does not write to a real search index.
// Replace with injection of IIndexBackend from Veritas.Rag (AzureSearchIndexBackend).
//
// BLOCKED on search expert: index schema (chunking strategy, chunk size, overlap,
// embedding model, scoring profiles) must be designed before implementation.
// See FRD FR-1.22–1.26 and the search expert session document.
//
// When implementing for real:
//   1. Inject IIndexBackend (Azure AI Search via Azure.Search.Documents NuGet)
//   2. Split full_text into chunks per strategy decided by search expert
//   3. Generate embeddings via Azure OpenAI text-embedding endpoint
//   4. POST chunks to index "veritas-documents" with corpus_id as filter field
public class IndexingService
{
    private readonly IDocumentStore _store;

    // [MOCK] In-memory log. Replace with structured telemetry (Application Insights).
    private readonly List<string> _indexLog = new();

    public IndexingService(IDocumentStore store) => _store = store;

    public Task IndexDocumentAsync(
        string corpusId, string documentId,
        ExtractionResult? extraction,
        CancellationToken ct = default)
    {
        // [MOCK] Real implementation: chunk → embed → write to Azure AI Search
        _indexLog.Add(
            $"[MOCK] Indexed document {documentId} in corpus {corpusId} at {DateTime.UtcNow:O}");
        return Task.CompletedTask;
    }

    public Task RemoveDocumentAsync(
        string corpusId, string documentId, CancellationToken ct = default)
    {
        // [MOCK] Real implementation: delete all chunks for documentId from Azure AI Search
        _indexLog.Add(
            $"[MOCK] Removed document {documentId} from corpus {corpusId} index at {DateTime.UtcNow:O}");
        return Task.CompletedTask;
    }

    public IReadOnlyList<string> GetIndexLog() => _indexLog.AsReadOnly();
}
