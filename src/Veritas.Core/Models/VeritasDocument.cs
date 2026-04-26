namespace Veritas.Core.Models;

public enum DocumentProcessingStatus
{
    Uploaded,
    Extracting,
    Indexing,
    Ready,
    Error,
    NeedsReview
}

public enum RightsDeclaration
{
    OwnContent,
    PermissionGranted,
    LicensedForPrivateUse,
    PublicDomain,
    OpenAccess,
    UnknownNeedsReview
}

public class VeritasDocument
{
    public required string DocumentId { get; init; }
    public required string CorpusId { get; init; }
    public required string OriginalFilename { get; init; }
    public required string Format { get; init; }
    public required long FileSizeBytes { get; init; }
    public required string Sha256Hash { get; init; }
    public required RightsDeclaration RightsDeclaration { get; init; }
    public required DateTime UploadedAt { get; init; }
    public required string UploadedBy { get; init; }
    public string? TitleOverride { get; set; }
    public Dictionary<string, string> UserMetadata { get; init; } = new();
    public DocumentProcessingStatus Status { get; set; } = DocumentProcessingStatus.Uploaded;
    public string? ExtractedTitle { get; set; }
    public List<string> ProcessingHistory { get; init; } = new();
}
