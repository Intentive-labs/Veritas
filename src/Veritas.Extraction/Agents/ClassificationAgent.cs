using Veritas.Core.Models;
using Veritas.DomainPacks;
using Veritas.DomainPacks.Models;

namespace Veritas.Extraction.Agents;

// Instructions: .github/agents/classification.md
// Skills: domain-pack-schema.skill
//
// Classifies experimental outcomes per pack classification.json rules.
// Every output is tagged with pack_id + pack_version (required by all downstream consumers).
// Classification rule changes are BREAKING (require pack major version bump).
public class ClassificationAgent
{
    private readonly IDomainPackRuntime _runtime;

    public ClassificationAgent(IDomainPackRuntime runtime) => _runtime = runtime;

    public Task<ClassifiedParameters> RunAsync(
        ExtractedParameters parameters, DomainPack pack, CancellationToken ct = default)
    {
        var result = _runtime.Classify(parameters, pack);

        return Task.FromResult(new ClassifiedParameters
        {
            DocumentId = parameters.DocumentId,
            PackId = pack.Manifest.PackId,
            PackVersion = pack.Manifest.Version,
            ClassifiedAt = DateTime.UtcNow,
            Outcome = result.Outcome,
            OutcomeConfidence = result.Confidence,
            SupportingEvidence = result.SupportingEvidence,
            ClassificationRule = result.AppliedRule
        });
    }
}
