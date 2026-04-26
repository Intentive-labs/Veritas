namespace Veritas.Rag.Models;

public record SearchChunk(
    string ChunkId,
    string DocumentId,
    string CorpusId,
    string Text,
    double Score
);

public record RetrievedChunk(
    string ChunkId,
    string DocumentId,
    string CorpusId,
    string Title,
    string Text,
    double RelevanceScore,
    int Year,
    string[] Authors
);

public record GeneratedAnswer(
    string Text,
    string Confidence,
    bool IsRefused,
    string? RefusalReason
);

public record CitationValidationResult(
    bool AllCitationsGrounded,
    List<string> UngroundedClaims
);
