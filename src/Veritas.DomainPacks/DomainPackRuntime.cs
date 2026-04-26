using Veritas.Core.Models;
using Veritas.DomainPacks.Models;

namespace Veritas.DomainPacks;

public class DomainPackRuntime : IDomainPackRuntime
{
    private readonly DomainPackLoader _loader = new();
    private readonly DomainPackValidator _validator = new();

    public async Task<DomainPack> LoadPackAsync(string packPath)
    {
        var pack = await _loader.LoadAsync(packPath);
        var result = _validator.Validate(pack);
        if (!result.IsValid)
            throw new InvalidOperationException(
                $"Pack validation failed: {string.Join("; ", result.Errors)}");
        return pack;
    }

    public ValidationResult ValidatePack(DomainPack pack) => _validator.Validate(pack);

    public ClassificationResult Classify(ExtractedParameters parameters, DomainPack pack)
    {
        foreach (var rule in pack.Classification.Rules)
        {
            if (parameters.Parameters.TryGetValue(rule.Field, out var field)
                && field.Confidence >= rule.ConfidenceThreshold)
            {
                return new ClassificationResult(
                    rule.Outcome,
                    field.Confidence,
                    rule.Condition,
                    [$"{rule.Field}: {field.RawValue}"]);
            }
        }

        return new ClassificationResult("inconclusive", 0.0, null, []);
    }

    public NormalizationResult Normalize(Dictionary<string, string> rawParameters, DomainPack pack)
    {
        var normalized = new Dictionary<string, object>();
        var unknownUnits = new List<string>();

        foreach (var (key, value) in rawParameters)
        {
            var conversion = pack.Normalization.UnitConversions
                .FirstOrDefault(c => value.EndsWith(c.FromUnit, StringComparison.OrdinalIgnoreCase));

            if (conversion is not null
                && double.TryParse(
                    value[..^conversion.FromUnit.Length].Trim(),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var numeric))
            {
                var converted = numeric * conversion.Factor + (conversion.Offset ?? 0.0);
                normalized[key] = new { value = converted, unit = conversion.ToUnit };
            }
            else
            {
                normalized[key] = value;
                unknownUnits.Add(key);
            }
        }

        return new NormalizationResult(normalized, unknownUnits);
    }

    public bool ValidateParameters(
        ExtractedParameters parameters, DomainPack pack, out List<ValidationIssue> issues)
    {
        issues = new List<ValidationIssue>();

        foreach (var rule in pack.Validation.Rules)
        {
            if (!parameters.Parameters.ContainsKey(rule.Field) && rule.Severity == "error")
            {
                issues.Add(new ValidationIssue(
                    rule.Field, rule.Condition, rule.Severity,
                    $"Required field '{rule.Field}' is missing"));
            }
        }

        return !issues.Any(i => i.Severity == "error");
    }
}
