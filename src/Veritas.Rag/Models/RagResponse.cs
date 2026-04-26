namespace Veritas.Rag.Models;

public record RagSource(
    string DocumentId,
    string Title,
    string ChunkId,
    string Excerpt
);

public record RagResponse(
    string Answer,
    string Confidence,
    IReadOnlyList<RagSource> Sources,
    string Disclaimer,
    bool IsRefused,
    string? RefusalReason
);
