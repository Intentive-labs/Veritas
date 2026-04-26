namespace Veritas.Core.Contracts;

public record HypothesisTestRequest(
    string HypothesisId,
    string PackId,
    string PackVersion
);

public record HypothesisTestResponse(
    string HypothesisId,
    string PackId,
    string PackVersion,
    HypothesisCoverage Coverage,
    HypothesisFindings Findings,
    string Confidence,
    string? BiasWarning,
    string Disclaimer
);

public record HypothesisCoverage(int Relevant, int Total, double Percent);

public record HypothesisFindings(int Supporting, int Contradicting, int Inconclusive);
