using Veritas.Rag.Models;

namespace Veritas.Rag.Contracts;

public interface IRetriever
{
    Task<IReadOnlyList<RetrievedChunk>> RetrieveAsync(
        string query,
        ICorpusConnector corpus,
        int topK,
        Dictionary<string, string>? filters,
        CancellationToken ct = default);
}
