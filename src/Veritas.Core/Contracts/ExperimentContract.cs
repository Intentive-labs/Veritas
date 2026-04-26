namespace Veritas.Core.Contracts;

public record CreateExperimentRequest(
    string PackId,
    string PackVersion,
    string HypothesisVersion,
    // [MOCK] Dictionary used as placeholder. Replace with a typed model once
    // lenr-experiment-schema.skill is authored by Pekka (cill-ab/lenr-pack Phase 2).
    Dictionary<string, object> Parameters,
    string? Notes
);

public record ExperimentResponse(
    string ExperimentId,
    string CorpusId,
    string PackId,
    string PackVersion,
    string HypothesisVersion,
    DateTime SubmittedAt,
    string SubmittedBy,
    Dictionary<string, object> Parameters,
    string? Notes
);

public record SimilarDocumentsResponse(
    string ExperimentId,
    List<SimilarDocument> SimilarDocuments
);

public record SimilarDocument(
    string DocumentId,
    string Title,
    double SimilarityScore
);
