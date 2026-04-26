using Veritas.Rag.Contracts;
using Veritas.Rag.Models;

namespace Veritas.Rag.Implementation;

// LENR domain corpus connector (FR-1.33).
// Wraps a generic CorpusConfig with LENR-specific availability checks
// and default search filters from LenrSearchFilters.
//
// [MOCK] IsAvailableAsync always returns true.
// Replace with a real check against the index health endpoint once
// AzureSearchIndexBackend is implemented (blocked on search expert session).
public class LenrCorpusConnector : ICorpusConnector
{
    public string CorpusId => Config.CorpusId;
    public CorpusConfig Config { get; }

    public LenrCorpusConnector(CorpusConfig config)
    {
        Config = config;
    }

    // [MOCK] Returns true unconditionally.
    // Real implementation: call IIndexBackend.IsHealthyAsync() or similar.
    public Task<bool> IsAvailableAsync(CancellationToken ct = default)
        => Task.FromResult(true);

    /// <summary>
    /// Builds a RagRequest enriched with LENR domain filters.
    /// Caller filters are merged on top of defaults.
    /// </summary>
    public RagRequest BuildRequest(
        string query,
        int topK = 10,
        Dictionary<string, string>? callerFilters = null)
    {
        var filters = LenrSearchFilters.BuildFilters(CorpusId, callerFilters);
        return new RagRequest(query, CorpusId, filters, topK);
    }

    /// <summary>
    /// Creates a LenrCorpusConnector with placeholder config.
    /// Replace with real config loaded from corpus metadata once the physicist
    /// ontology session and search expert session are complete.
    /// </summary>
    public static LenrCorpusConnector CreateMock(string corpusId) =>
        new(new CorpusConfig
        {
            CorpusId = corpusId,
            IndexName = "veritas-documents",
            // [MOCK] Chunking parameters — to be set by search expert
            ChunkingStrategy = "sliding-window",
            ChunkSize = 512,
            ChunkOverlap = 64,
            EmbeddingModel = "text-embedding-3-large",
            Disclaimer = $"[MOCK] All findings correlational only. Specific to LENR corpus [{corpusId}] " +
                         "and its domain pack assumptions."
        });
}
