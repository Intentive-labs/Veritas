namespace Veritas.Core.Contracts;

public record CreateCorpusRequest(
    string Name,
    string SourceType,
    string? Description
);

public record CorpusResponse(
    string CorpusId,
    string Name,
    string Owner,
    string Visibility,
    string SourceType,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int DocumentCount,
    string IndexStatus
);
