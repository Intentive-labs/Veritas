namespace Veritas.DomainPacks.Models;

public record OntologyParameter(
    string Name,
    string Type,
    string Unit,
    string Description,
    bool Required,
    List<string>? CanonicalValues
);

public record PackOntology(List<OntologyParameter> Parameters);
