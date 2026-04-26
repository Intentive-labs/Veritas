namespace Veritas.Core.Contracts;

public record ClassificationResult(
    string DocumentId,
    string PackId,
    string PackVersion,
    string Outcome,
    double OutcomeConfidence,
    List<string> SupportingEvidence
);

public record MultiPackComparisonResponse(
    string CorpusId,
    string DocumentId,
    List<ClassificationResult> Results,
    bool Agreement,
    List<string> ParameterDiffs
);
