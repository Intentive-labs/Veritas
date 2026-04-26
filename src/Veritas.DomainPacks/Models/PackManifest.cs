namespace Veritas.DomainPacks.Models;

public record PackManifest(
    string PackId,
    string Version,
    string Name,
    string Field,
    string Owner,
    string Status,
    List<string> Assumptions,
    List<string> Ignores,
    string Source,
    string SchemaVersion
);
