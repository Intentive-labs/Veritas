using Veritas.Rag.Models;

namespace Veritas.Rag.Contracts;

public interface ICitationValidator
{
    CitationValidationResult Validate(string answer, IReadOnlyList<RetrievedChunk> chunks);
}
