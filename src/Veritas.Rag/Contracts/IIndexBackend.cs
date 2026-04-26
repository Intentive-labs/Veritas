using Veritas.Rag.Models;

namespace Veritas.Rag.Contracts;

public interface IIndexBackend
{
    Task<IReadOnlyList<SearchChunk>> SearchAsync(
        string query,
        string indexName,
        Dictionary<string, string>? filters,
        int topK,
        CancellationToken ct = default);
}
