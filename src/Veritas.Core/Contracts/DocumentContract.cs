namespace Veritas.Core.Contracts;

public record DocumentUploadRequest(
    string RightsDeclaration,
    string? TitleOverride,
    Dictionary<string, string>? Metadata
);

public record DocumentResponse(
    string DocumentId,
    string CorpusId,
    string OriginalFilename,
    string Format,
    long FileSizeBytes,
    string Sha256Hash,
    string RightsDeclaration,
    DateTime UploadedAt,
    string UploadedBy,
    string? TitleOverride,
    string Status,
    string? ExtractedTitle,
    List<string> ProcessingHistory
);

public record DocumentMetadataSidecar(
    string DocumentId,
    string CorpusId,
    string OriginalFilename,
    string Format,
    long FileSizeBytes,
    string Sha256Hash,
    string RightsDeclaration,
    DateTime UploadedAt,
    string UploadedBy,
    string? TitleOverride,
    Dictionary<string, string> UserMetadata
);
