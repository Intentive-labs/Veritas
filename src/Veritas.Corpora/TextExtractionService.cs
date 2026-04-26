using System.Text.Json;
using Veritas.Core.Models;
using Veritas.Storage;

namespace Veritas.Corpora;

// [MOCK] Text extraction uses a stub that returns placeholder data.
// Replace with Azure Content Understanding (Azure AI Document Intelligence) calls:
//   - NuGet: Azure.AI.DocumentIntelligence or Azure.AI.FormRecognizer
//   - Endpoint: IConfiguration["AzureContentUnderstanding:Endpoint"]
//   - Auth: DefaultAzureCredential (managed identity)
//   - Build extraction request using pack ontology fields (see content-understanding-schema.skill)
//   - For MD/TXT: read content directly — no AI needed
//   - For HTML: strip tags first, then optionally pass through Content Understanding
//   - Documents with title/abstract confidence < 0.6 are flagged NeedsReview (FR-1.20)
public class TextExtractionService
{
    private readonly IDocumentStore _store;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public TextExtractionService(IDocumentStore store) => _store = store;

    public async Task<ExtractionResult> ExtractAsync(
        string corpusId, string documentId, string format, CancellationToken ct = default)
    {
        var rawPath = StoragePaths.RawDocument(corpusId, documentId, format.TrimStart('.'));
        var stream = await _store.ReadAsync(rawPath, ct);

        ExtractionResult result;

        if (format is ".md" or ".txt")
        {
            using var reader = new StreamReader(stream);
            var text = await reader.ReadToEndAsync(ct);
            result = CreatePlainTextResult(documentId, text);
        }
        else
        {
            // [MOCK] Replace with real Azure Content Understanding API call.
            // Steps:
            //   1. POST document bytes to Azure Content Understanding endpoint
            //   2. Poll result endpoint until status == "succeeded"
            //   3. Map: title, authors, year, abstract, institutions, document_type → ExtractionField
            //   4. Set NeedsReview = true if title.confidence < 0.6 || abstract.confidence < 0.6
            result = MockAzureContentUnderstanding(documentId);
        }

        await _store.WriteTextAsync(
            StoragePaths.ExtractedText(corpusId, documentId),
            JsonSerializer.Serialize(result, JsonOptions), ct);

        return result;
    }

    private static ExtractionResult CreatePlainTextResult(string documentId, string text) =>
        new(documentId,
            new ExtractionField("Untitled", 0.5),
            new ExtractionField(Array.Empty<string>(), 0.0),
            new ExtractionField(null, 0.0),
            new ExtractionField(text[..Math.Min(500, text.Length)], 0.5),
            new ExtractionField(Array.Empty<string>(), 0.0),
            new ExtractionField("text", 1.0),
            text,
            DateTime.UtcNow,
            NeedsReview: false);

    // [MOCK] Returns synthetic extraction data.
    // Real implementation calls Azure Content Understanding REST API.
    private static ExtractionResult MockAzureContentUnderstanding(string documentId) =>
        new(documentId,
            new ExtractionField("[MOCK] Title from Azure Content Understanding", 0.85),
            new ExtractionField(new[] { "[MOCK] Author 1", "[MOCK] Author 2" }, 0.80),
            new ExtractionField(2024, 0.90),
            new ExtractionField("[MOCK] Abstract from Azure Content Understanding", 0.82),
            new ExtractionField(new[] { "[MOCK] University" }, 0.75),
            new ExtractionField("journal_article", 0.88),
            "[MOCK] Full text from Azure Content Understanding",
            DateTime.UtcNow,
            NeedsReview: false);
}

public record ExtractionField(object? Value, double Confidence);

public record ExtractionResult(
    string DocumentId,
    ExtractionField Title,
    ExtractionField Authors,
    ExtractionField Year,
    ExtractionField Abstract,
    ExtractionField Institutions,
    ExtractionField DocumentType,
    string FullText,
    DateTime ExtractionTimestamp,
    bool NeedsReview
);
