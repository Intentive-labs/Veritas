using Veritas.Rag.Models;

namespace Veritas.Rag.Contracts;

public interface ICorpusConnector
{
    string CorpusId { get; }
    CorpusConfig Config { get; }
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}
