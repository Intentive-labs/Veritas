using Veritas.Core.Models;
using Veritas.DomainPacks.Models;

namespace Veritas.Extraction;

/// <summary>
/// Validates that an extraction output covers the required fields defined in the
/// active domain pack ontology (FR-2.8).
///
/// A "gap" is a required ontology field that is either absent from the extraction
/// or has confidence below the minimum threshold.
/// </summary>
public class ContractCoverageValidator
{
    /// <summary>Minimum confidence for a field to be considered "covered".</summary>
    public double MinConfidence { get; init; } = 0.5;

    /// <summary>
    /// Validates extracted parameters against the pack ontology.
    /// Returns a <see cref="CoverageReport"/> with per-field results and summary metrics.
    /// </summary>
    public CoverageReport Validate(ExtractedParameters extraction, DomainPack pack)
    {
        ArgumentNullException.ThrowIfNull(extraction);
        ArgumentNullException.ThrowIfNull(pack);

        var gaps = new List<CoverageGap>();
        int coveredRequired = 0;
        int totalRequired = 0;
        int coveredOptional = 0;
        int totalOptional = 0;

        foreach (var param in pack.Ontology.Parameters)
        {
            bool isRequired = param.Required;
            if (isRequired) totalRequired++;
            else totalOptional++;

            if (!extraction.Parameters.TryGetValue(param.Name, out var field))
            {
                gaps.Add(new CoverageGap(
                    param.Name,
                    isRequired ? GapSeverity.Error : GapSeverity.Warning,
                    $"Field '{param.Name}' absent from extraction output."));
                continue;
            }

            if (field.Confidence < MinConfidence)
            {
                gaps.Add(new CoverageGap(
                    param.Name,
                    isRequired ? GapSeverity.Warning : GapSeverity.Info,
                    $"Field '{param.Name}' confidence {field.Confidence:F2} below threshold {MinConfidence:F2}."));
                // Still count as partially covered — not a hard gap
            }

            if (isRequired) coveredRequired++;
            else coveredOptional++;
        }

        // Check for unknown fields — fields in extraction not defined in ontology
        foreach (var extractedKey in extraction.Parameters.Keys)
        {
            if (!pack.Ontology.Parameters.Any(p => p.Name == extractedKey))
            {
                gaps.Add(new CoverageGap(
                    extractedKey,
                    GapSeverity.Info,
                    $"Field '{extractedKey}' not defined in pack ontology (unexpected field)."));
            }
        }

        bool hasGaps = gaps.Any(g => g.Severity == GapSeverity.Error);
        double coveragePercent = totalRequired > 0
            ? Math.Round((double)coveredRequired / totalRequired * 100, 1)
            : 100.0;

        return new CoverageReport(
            DocumentId: extraction.DocumentId,
            PackId: extraction.PackId,
            PackVersion: extraction.PackVersion,
            HasGaps: hasGaps,
            CoveragePercent: coveragePercent,
            CoveredRequired: coveredRequired,
            TotalRequired: totalRequired,
            CoveredOptional: coveredOptional,
            TotalOptional: totalOptional,
            Gaps: gaps
        );
    }
}

public record CoverageReport(
    string DocumentId,
    string PackId,
    string PackVersion,
    bool HasGaps,
    double CoveragePercent,
    int CoveredRequired,
    int TotalRequired,
    int CoveredOptional,
    int TotalOptional,
    IReadOnlyList<CoverageGap> Gaps
);

public record CoverageGap(
    string FieldName,
    GapSeverity Severity,
    string Message
);

public enum GapSeverity { Info, Warning, Error }
