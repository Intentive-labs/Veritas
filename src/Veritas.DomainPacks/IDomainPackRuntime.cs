using Veritas.Core.Models;
using Veritas.DomainPacks.Models;

namespace Veritas.DomainPacks;

public record ValidationResult(bool IsValid, List<string> Errors);

public record ClassificationResult(
    string Outcome,
    double Confidence,
    string? AppliedRule,
    List<string> SupportingEvidence
);

public record NormalizationResult(
    Dictionary<string, object> NormalizedValues,
    List<string> UnknownUnits
);

public record ValidationIssue(string Field, string Rule, string Severity, string Message);

public interface IDomainPackRuntime
{
    Task<DomainPack> LoadPackAsync(string packPath);
    ValidationResult ValidatePack(DomainPack pack);
    ClassificationResult Classify(ExtractedParameters parameters, DomainPack pack);
    NormalizationResult Normalize(Dictionary<string, string> rawParameters, DomainPack pack);
    bool ValidateParameters(ExtractedParameters parameters, DomainPack pack, out List<ValidationIssue> issues);
}
