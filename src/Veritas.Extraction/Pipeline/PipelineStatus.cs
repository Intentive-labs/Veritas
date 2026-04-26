namespace Veritas.Extraction.Pipeline;

public enum ExtractionPipelineStatus
{
    Queued,
    Extracting,
    Validating,
    Normalizing,
    Classifying,
    AwaitingHumanReview,
    Validated,
    Failed
}

public class PipelineJob
{
    public required string JobId { get; init; }
    public required string DocumentId { get; init; }
    public required string CorpusId { get; init; }
    public required string PackId { get; init; }
    public required string PackVersion { get; init; }
    public ExtractionPipelineStatus Status { get; set; } = ExtractionPipelineStatus.Queued;
    public required DateTime CreatedAt { get; init; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Error { get; set; }
    public List<string> CompletedSteps { get; init; } = new();
}
