namespace Veritas.Core.Models;

public class ExtractedField
{
    public string? RawValue { get; init; }
    public double Confidence { get; init; }
    public string? SourceText { get; init; }
}

public class ExtractedParameters
{
    public required string DocumentId { get; init; }
    public required string CorpusId { get; init; }
    public required string PackId { get; init; }
    public required string PackVersion { get; init; }
    public required DateTime ExtractedAt { get; init; }
    public Dictionary<string, ExtractedField> Parameters { get; init; } = new();
}
