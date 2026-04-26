namespace Veritas.Core.Contracts;

public record ExtractionRequest(
    string DocumentId,
    string CorpusId,
    string PackId,
    string PackVersion
);

public record ExtractionStatusResponse(
    string DocumentId,
    string Status,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? Error,
    List<string> CompletedSteps
);
