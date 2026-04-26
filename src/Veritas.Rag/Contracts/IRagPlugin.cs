using Veritas.Rag.Models;

namespace Veritas.Rag.Contracts;

public interface IRagPlugin
{
    Task<RagResponse> QueryAsync(RagRequest request, CancellationToken ct = default);
}
