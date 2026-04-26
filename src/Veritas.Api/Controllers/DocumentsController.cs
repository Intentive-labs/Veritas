using Microsoft.AspNetCore.Mvc;
using Veritas.Core.Contracts;
using Veritas.Core.Models;
using Veritas.Corpora;

namespace Veritas.Api.Controllers;

[ApiController]
[Route("api/corpora/{corpusId}/documents")]
public class DocumentsController : ControllerBase
{
    private readonly DocumentIngestionService _ingestion;
    private readonly TextExtractionService _extraction;
    private readonly IndexingService _indexing;

    public DocumentsController(
        DocumentIngestionService ingestion,
        TextExtractionService extraction,
        IndexingService indexing)
    {
        _ingestion = ingestion;
        _extraction = extraction;
        _indexing = indexing;
    }

    // [MOCK] Replace with proper user identity from Azure AD claims.
    private string CurrentUser => HttpContext.User.Identity?.Name ?? "mock-user";

    /// <summary>
    /// POST /api/corpora/{corpusId}/documents — upload a document.
    /// Accepts multipart/form-data: file (required), rights_declaration (required),
    /// title (optional), metadata JSON (optional).
    /// Idempotent: re-uploading same file returns existing document_id (FR-1.12).
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(52_428_800)] // 50 MB (FR-NFR-3)
    public async Task<IActionResult> UploadAsync(
        string corpusId,
        IFormFile file,
        [FromForm] string rights_declaration,
        [FromForm] string? title = null,
        [FromForm] string? metadata = null)
    {
        if (file is null || file.Length == 0)
            return BadRequest("file is required");

        if (!DocumentIngestionService.IsFormatSupported(file.FileName))
            return StatusCode(415,
                $"Unsupported format. Supported: " +
                string.Join(", ", DocumentIngestionService.GetSupportedFormats()));

        Dictionary<string, string>? metaDict = null;
        if (!string.IsNullOrWhiteSpace(metadata))
        {
            try
            {
                metaDict = System.Text.Json.JsonSerializer
                    .Deserialize<Dictionary<string, string>>(metadata);
            }
            catch
            {
                return BadRequest("metadata must be a JSON object of string key/value pairs");
            }
        }

        (VeritasDocument doc, bool isExisting) result;
        try
        {
            result = await _ingestion.IngestAsync(
                corpusId, file.OpenReadStream(), file.FileName,
                rights_declaration, CurrentUser, title, metaDict);
        }
        catch (NotSupportedException ex) { return StatusCode(415, ex.Message); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }

        if (result.isExisting)
            return Ok(ToResponse(result.doc));

        // Kick off async extraction pipeline (fire-and-forget in this mock)
        // [MOCK] Replace with enqueue to ExtractionQueue / Azure Service Bus
        _ = Task.Run(async () =>
        {
            result.doc.Status = DocumentProcessingStatus.Extracting;
            var extraction = await _extraction.ExtractAsync(
                corpusId, result.doc.DocumentId, result.doc.Format);
            if (extraction.NeedsReview)
                result.doc.Status = DocumentProcessingStatus.NeedsReview;
            else
            {
                result.doc.Status = DocumentProcessingStatus.Indexing;
                await _indexing.IndexDocumentAsync(corpusId, result.doc.DocumentId, extraction);
                result.doc.Status = DocumentProcessingStatus.Ready;
            }
            result.doc.ProcessingHistory.Add($"{DateTime.UtcNow:O}: extraction complete");
        });

        return CreatedAtAction(nameof(GetDocumentAsync),
            new { corpusId, documentId = result.doc.DocumentId },
            ToResponse(result.doc));
    }

    /// <summary>GET /api/corpora/{corpusId}/documents — list all documents in corpus.</summary>
    [HttpGet]
    public async Task<IActionResult> ListAsync(string corpusId)
    {
        var docs = await _ingestion.ListAsync(corpusId);
        return Ok(docs.Select(ToResponse));
    }

    /// <summary>GET /api/corpora/{corpusId}/documents/{documentId} — document metadata.</summary>
    [HttpGet("{documentId}")]
    public async Task<IActionResult> GetDocumentAsync(string corpusId, string documentId)
    {
        var doc = await _ingestion.GetAsync(documentId);
        if (doc is null || doc.CorpusId != corpusId) return NotFound();
        return Ok(ToResponse(doc));
    }

    /// <summary>DELETE /api/corpora/{corpusId}/documents/{documentId} — remove document.</summary>
    [HttpDelete("{documentId}")]
    public async Task<IActionResult> DeleteAsync(string corpusId, string documentId)
    {
        var doc = await _ingestion.GetAsync(documentId);
        if (doc is null || doc.CorpusId != corpusId) return NotFound();

        await _ingestion.DeleteAsync(corpusId, documentId);
        await _indexing.RemoveDocumentAsync(corpusId, documentId);
        return NoContent();
    }

    /// <summary>POST /api/corpora/{corpusId}/documents/{documentId}/reprocess — re-queue extraction.</summary>
    [HttpPost("{documentId}/reprocess")]
    public async Task<IActionResult> ReprocessAsync(string corpusId, string documentId)
    {
        var doc = await _ingestion.GetAsync(documentId);
        if (doc is null || doc.CorpusId != corpusId) return NotFound();

        // [MOCK] Real implementation: enqueue to ExtractionQueue / Azure Service Bus
        doc.Status = DocumentProcessingStatus.Extracting;
        doc.ProcessingHistory.Add($"{DateTime.UtcNow:O}: reprocess requested");
        return Accepted();
    }

    private static DocumentResponse ToResponse(VeritasDocument d) => new(
        d.DocumentId, d.CorpusId, d.OriginalFilename, d.Format,
        d.FileSizeBytes, d.Sha256Hash,
        d.RightsDeclaration.ToString().ToLowerInvariant(),
        d.UploadedAt, d.UploadedBy, d.TitleOverride,
        d.Status.ToString().ToLowerInvariant(),
        d.ExtractedTitle, d.ProcessingHistory);
}
