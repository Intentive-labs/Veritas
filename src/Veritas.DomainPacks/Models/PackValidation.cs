namespace Veritas.DomainPacks.Models;

public record ValidationRule(string Field, string Condition, string Severity, string Message);

public record PackValidation(List<ValidationRule> Rules);
