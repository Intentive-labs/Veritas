namespace Veritas.Rag.Models;

public record RagRequest(
    string Query,
    string CorpusId,
    Dictionary<string, string>? Filters,
    int TopK = 10
);
