using Veritas.Core.Models;
using Veritas.DomainPacks;
using Veritas.DomainPacks.Models;

namespace Veritas.Extraction.Agents;

// Instructions: .github/agents/validation.md
// Skills: domain-pack-schema.skill
//
// Applies physical plausibility rules from pack.validation.json.
// Reports issues — does NOT correct values.
public class ValidationAgent
{
    private readonly IDomainPackRuntime _runtime;

    public ValidationAgent(IDomainPackRuntime runtime) => _runtime = runtime;

    public Task<ValidationAgentResult> RunAsync(
        ExtractedParameters parameters, DomainPack pack, CancellationToken ct = default)
    {
        _runtime.ValidateParameters(parameters, pack, out var issues);
        return Task.FromResult(new ValidationAgentResult(
            parameters.DocumentId,
            pack.Manifest.PackId,
            pack.Manifest.Version,
            DateTime.UtcNow,
            issues,
            HasErrors: issues.Any(i => i.Severity == "error")));
    }
}

public record ValidationAgentResult(
    string DocumentId,
    string PackId,
    string PackVersion,
    DateTime ValidatedAt,
    List<ValidationIssue> Issues,
    bool HasErrors
);
