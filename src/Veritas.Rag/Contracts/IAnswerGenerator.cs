using Veritas.Rag.Models;

namespace Veritas.Rag.Contracts;

public interface IAnswerGenerator
{
    Task<GeneratedAnswer> GenerateAsync(
        string query,
        IReadOnlyList<RetrievedChunk> chunks,
        CorpusConfig config,
        CancellationToken ct = default);
}
