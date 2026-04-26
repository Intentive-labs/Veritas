using Microsoft.AspNetCore.Mvc;
using Veritas.Corpora;

namespace Veritas.Api.Controllers;

[ApiController]
[Route("api/corpora/{corpusId}/validation")]
public class ValidationController : ControllerBase
{
    private readonly DocumentIngestionService _ingestion;

    public ValidationController(DocumentIngestionService ingestion) => _ingestion = ingestion;

    /// <summary>
    /// GET /api/corpora/{corpusId}/validation/queue
    /// Returns documents awaiting human review, ordered by extraction confidence ascending (FR-2.9).
    /// </summary>
    [HttpGet("queue")]
    public async Task<IActionResult> GetValidationQueueAsync(string corpusId)
    {
        var docs = await _ingestion.ListAsync(corpusId);
        var needsReview = docs
            .Where(d => d.Status == Core.Models.DocumentProcessingStatus.NeedsReview)
            .OrderBy(d => d.UploadedAt) // [MOCK] Order by confidence once extraction confidence is stored
            .Select(d => new
            {
                d.DocumentId,
                d.OriginalFilename,
                d.Status,
                d.ExtractedTitle,
                d.UploadedAt
            });
        return Ok(needsReview);
    }
}
