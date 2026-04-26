namespace Veritas.DomainPacks.Models;

public record HypothesisVariable(string Name, string Role, string Description);

public record PackHypothesis(
    string HypothesisId,
    string Version,
    string Title,
    string Description,
    List<HypothesisVariable> Variables,
    string ExpectedDirection,
    double ConfidenceThreshold
);
