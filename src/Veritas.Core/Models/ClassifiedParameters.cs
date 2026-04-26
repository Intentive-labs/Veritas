namespace Veritas.Core.Models;

public class ClassifiedParameters
{
    public required string DocumentId { get; init; }
    public required string PackId { get; init; }
    public required string PackVersion { get; init; }
    public required DateTime ClassifiedAt { get; init; }
    public required string Outcome { get; init; }
    public double OutcomeConfidence { get; init; }
    public List<string> SupportingEvidence { get; init; } = new();
    public string? ClassificationRule { get; init; }
}
