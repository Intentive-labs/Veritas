using Veritas.Core.Models;
using Veritas.DomainPacks;
using Veritas.DomainPacks.Models;

namespace Veritas.Extraction.Agents;

// Instructions: .github/agents/normalization.md
// Skills: normalization-rules.skill, domain-pack-schema.skill
//
// Converts extracted parameter values to canonical units per pack normalization.json.
// Uses unit_unknown where unit cannot be determined — never guesses.
public class NormalizationAgent
{
    private readonly IDomainPackRuntime _runtime;

    public NormalizationAgent(IDomainPackRuntime runtime) => _runtime = runtime;

    public Task<NormalizationAgentResult> RunAsync(
        ExtractedParameters parameters, DomainPack pack, CancellationToken ct = default)
    {
        var rawValues = parameters.Parameters
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.RawValue ?? string.Empty);
        var result = _runtime.Normalize(rawValues, pack);

        return Task.FromResult(new NormalizationAgentResult(
            parameters.DocumentId,
            pack.Manifest.PackId,
            pack.Manifest.Version,
            DateTime.UtcNow,
            result.NormalizedValues,
            result.UnknownUnits));
    }
}

public record NormalizationAgentResult(
    string DocumentId,
    string PackId,
    string PackVersion,
    DateTime NormalizedAt,
    Dictionary<string, object> Parameters,
    List<string> UnknownUnits
);
