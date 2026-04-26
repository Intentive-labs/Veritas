namespace Veritas.Storage;

/// <summary>
/// Centralised Data Lake path builder.
/// All zone paths go through here — never construct paths inline.
/// See FRD section 1.5 for zone definitions.
/// </summary>
public static class StoragePaths
{
    public static string RawDocument(string corpusId, string documentId, string ext)
        => $"/raw/corpora/{corpusId}/documents/{documentId}/original.{ext}";

    public static string RawSidecar(string corpusId, string documentId)
        => $"/raw/corpora/{corpusId}/documents/{documentId}/metadata.json";

    public static string ExtractedText(string corpusId, string documentId)
        => $"/extracted/corpora/{corpusId}/{documentId}/text.json";

    public static string ExtractionStep(string corpusId, string documentId, int step)
        => $"/extracted/corpora/{corpusId}/{documentId}/step_{step}.json";

    public static string ValidatedCorrections(string corpusId, string documentId)
        => $"/validated/corpora/{corpusId}/{documentId}/corrections.json";

    public static string ClassifiedDocument(
        string corpusId, string packId, string version, string documentId)
        => $"/classified/corpora/{corpusId}/{packId}/{version}/{documentId}.json";

    public static string ExperimentRecord(string corpusId, string experimentId)
        => $"/experiments/{corpusId}/{experimentId}/record.json";

    public static string ExperimentMeasurementsDir(string corpusId, string experimentId)
        => $"/experiments/{corpusId}/{experimentId}/measurements/";

    public static string AnalysisDir(string corpusId, string packId, string version)
        => $"/analysis/corpora/{corpusId}/{packId}/{version}/";
}
