namespace Veritas.DomainPacks.Models;

public class DomainPack
{
    public required PackManifest Manifest { get; init; }
    public required PackOntology Ontology { get; init; }
    public required PackClassification Classification { get; init; }
    public required PackValidation Validation { get; init; }
    public required PackNormalization Normalization { get; init; }
    public List<PackHypothesis> Hypotheses { get; init; } = new();
}
