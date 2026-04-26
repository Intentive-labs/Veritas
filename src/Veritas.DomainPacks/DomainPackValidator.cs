using System.Text.RegularExpressions;
using Veritas.DomainPacks.Models;

namespace Veritas.DomainPacks;

public class DomainPackValidator
{
    private static readonly string[] ValidStatuses = ["official", "experimental", "community"];

    public ValidationResult Validate(DomainPack pack)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(pack.Manifest.PackId))
            errors.Add("manifest.pack_id is required");
        else if (!Regex.IsMatch(pack.Manifest.PackId, @"^[a-z0-9-]+$"))
            errors.Add("manifest.pack_id must match ^[a-z0-9-]+$");

        if (string.IsNullOrWhiteSpace(pack.Manifest.Version) ||
            !Regex.IsMatch(pack.Manifest.Version, @"^\d+\.\d+\.\d+$"))
            errors.Add("manifest.version must be semver (e.g. 1.0.0)");

        if (!ValidStatuses.Contains(pack.Manifest.Status))
            errors.Add("manifest.status must be official, experimental, or community");

        if (pack.Manifest.Assumptions is not { Count: > 0 })
            errors.Add("manifest.assumptions must have at least one entry");

        if (pack.Manifest.Ignores is not { Count: > 0 })
            errors.Add("manifest.ignores must have at least one entry");

        if (pack.Ontology.Parameters is not { Count: > 0 })
            errors.Add("ontology.parameters must have at least one entry");

        return new ValidationResult(errors.Count == 0, errors);
    }
}
