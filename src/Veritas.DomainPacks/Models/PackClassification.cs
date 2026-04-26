namespace Veritas.DomainPacks.Models;

public record OutcomeClass(string Name, string Description);

public record ClassificationRule(
    string Field,
    string Condition,
    string Outcome,
    double ConfidenceThreshold
);

public record PackClassification(
    List<OutcomeClass> OutcomeClasses,
    List<ClassificationRule> Rules
);
