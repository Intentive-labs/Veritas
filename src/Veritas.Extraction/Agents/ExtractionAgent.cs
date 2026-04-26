using Veritas.Core.Models;
using Veritas.DomainPacks.Models;

namespace Veritas.Extraction.Agents;

// Instructions: .github/agents/extraction.md
// Skills: content-understanding-schema.skill, domain-pack-schema.skill
//
// Extracts structured parameters from a document using pack ontology.
// Does NOT interpret, normalise, or classify — raw extraction only.
public class ExtractionAgent
{
    public Task<ExtractedParameters> RunAsync(
        string documentId, string corpusId,
        DomainPack pack, CancellationToken ct = default)
    {
        // [MOCK] Real implementation:
        //   1. Load pack.Ontology.Parameters to build the extraction field list
        //   2. POST document to Azure Content Understanding endpoint with field list
        //   3. Map API response to ExtractedParameters (value + confidence per field)
        //   4. Never infer missing values — use null with confidence 0.0
        // See content-understanding-schema.skill for the full API contract.
        var mockParameters = new Dictionary<string, ExtractedField>();
        foreach (var param in pack.Ontology.Parameters)
        {
            mockParameters[param.Name] = new ExtractedField
            {
                RawValue = $"[MOCK] Extracted value for {param.Name}",
                Confidence = 0.75,
                SourceText = $"[MOCK] Source text snippet for {param.Name}"
            };
        }

        return Task.FromResult(new ExtractedParameters
        {
            DocumentId = documentId,
            CorpusId = corpusId,
            PackId = pack.Manifest.PackId,
            PackVersion = pack.Manifest.Version,
            ExtractedAt = DateTime.UtcNow,
            Parameters = mockParameters
        });
    }
}
