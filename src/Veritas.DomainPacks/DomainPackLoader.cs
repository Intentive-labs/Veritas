using System.Text.Json;
using Veritas.DomainPacks.Models;

namespace Veritas.DomainPacks;

public class DomainPackLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<DomainPack> LoadAsync(string packDirectory)
    {
        var manifest = await LoadFileAsync<PackManifest>(packDirectory, "manifest.json");
        var ontology = await LoadFileAsync<PackOntology>(packDirectory, "ontology.json");
        var classification = await LoadFileAsync<PackClassification>(packDirectory, "classification.json");
        var validation = await LoadFileAsync<PackValidation>(packDirectory, "validation.json");
        var normalization = await LoadFileAsync<PackNormalization>(packDirectory, "normalization.json");

        var hypotheses = new List<PackHypothesis>();
        var hypothesesDir = Path.Combine(packDirectory, "hypotheses");
        if (Directory.Exists(hypothesesDir))
        {
            foreach (var file in Directory.GetFiles(hypothesesDir, "*.json"))
            {
                var h = await LoadFileAsync<PackHypothesis>(hypothesesDir, Path.GetFileName(file));
                hypotheses.Add(h);
            }
        }

        return new DomainPack
        {
            Manifest = manifest,
            Ontology = ontology,
            Classification = classification,
            Validation = validation,
            Normalization = normalization,
            Hypotheses = hypotheses
        };
    }

    private static async Task<T> LoadFileAsync<T>(string directory, string filename)
    {
        var path = Path.Combine(directory, filename);
        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<T>(json, JsonOptions)
               ?? throw new InvalidOperationException($"Failed to deserialize {filename}");
    }
}
