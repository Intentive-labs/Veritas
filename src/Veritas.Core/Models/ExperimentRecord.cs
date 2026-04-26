namespace Veritas.Core.Models;

public class ExperimentRecord
{
    public required string ExperimentId { get; init; }
    public required string CorpusId { get; init; }
    public required string PackId { get; init; }
    public required string PackVersion { get; init; }

    // References the hypothesis declared in the active pack (see PackHypothesis.HypothesisId)
    public required string HypothesisVersion { get; init; }
    public required DateTime SubmittedAt { get; init; }
    public required string SubmittedBy { get; init; }

    // [MOCK] Parameter schema validated against lenr-experiment-schema.skill (requires physicist session)
    // Replace Dictionary<string,object> with a strongly-typed experiment parameter model
    // once the LENR experiment schema is authored by physicist.
    public Dictionary<string, object> Parameters { get; init; } = new();
    public string? Notes { get; set; }

    // Experiments are immutable — corrections go through POST /experiments/{id}/corrections
    public bool IsImmutable { get; set; } = true;
}
