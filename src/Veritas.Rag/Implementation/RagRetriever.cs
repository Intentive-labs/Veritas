using Veritas.Rag.Contracts;
using Veritas.Rag.Models;

namespace Veritas.Rag.Implementation;

public class RagRetriever : IRetriever
{
    private readonly IIndexBackend _indexBackend;

    public RagRetriever(IIndexBackend indexBackend) => _indexBackend = indexBackend;

    public async Task<IReadOnlyList<RetrievedChunk>> RetrieveAsync(
        string query, ICorpusConnector corpus,
        int topK, Dictionary<string, string>? filters,
        CancellationToken ct = default)
    {
        var allFilters = new Dictionary<string, string>(corpus.Config.DefaultFilters)
        {
            ["corpus_id"] = corpus.CorpusId
        };
        if (filters is not null)
            foreach (var (k, v) in filters) allFilters[k] = v;

        var chunks = await _indexBackend.SearchAsync(
            query, corpus.Config.IndexName, allFilters, topK, ct);

        return chunks.Select(c => new RetrievedChunk(
            c.ChunkId, c.DocumentId, c.CorpusId,
            // [MOCK] Title lookup: replace with a document metadata service call
            $"[MOCK] Title for {c.DocumentId}",
            c.Text, c.Score, 0, []
        )).ToList();
    }
}
