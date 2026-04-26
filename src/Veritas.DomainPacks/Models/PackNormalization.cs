namespace Veritas.DomainPacks.Models;

public record UnitConversion(string FromUnit, string ToUnit, double Factor, double? Offset);

public record TerminologyMapping(string Variant, string Canonical);

public record PackNormalization(
    List<UnitConversion> UnitConversions,
    List<TerminologyMapping> TerminologyMappings
);
