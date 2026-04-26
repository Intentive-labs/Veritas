using Veritas.Rag.Contracts;
using Veritas.Rag.Models;

namespace Veritas.Rag.Implementation;

// [MOCK] Mock index backend — returns synthetic search results for dev/test.
// Replace with AzureSearchIndexBackend using Azure.Search.Documents NuGet package.
// BLOCKED on search expert: index schema must be designed first (FRD FR-1.23).
// Index name: "veritas-documents". Corpus scoping via corpus_id filter field.
public class MockIndexBackend : IIndexBackend
{
    public Task<IReadOnlyList<SearchChunk>> SearchAsync(
        string query, string indexName,
        Dictionary<string, string>? filters, int topK,
        CancellationToken ct = default)
    {
        // [MOCK] Returns synthetic chunks. Replace with SearchClient.SearchAsync() call.
        var corpusId = filters?.GetValueOrDefault("corpus_id") ?? "mock-corpus";
        IReadOnlyList<SearchChunk> results = Enumerable.Range(1, Math.Min(topK, 3))
            .Select(i => new SearchChunk(
                $"chunk-mock-{i}",
                $"doc-mock-{i}",
                corpusId,
                $"[MOCK] Search result {i} for query: {query}",
                1.0 - i * 0.1))
            .ToList();
        return Task.FromResult(results);
    }
}
