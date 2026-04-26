using Microsoft.AspNetCore.Mvc;
using Veritas.Core.Contracts;
using Veritas.Core.Models;
using Veritas.Corpora;

namespace Veritas.Api.Controllers;

[ApiController]
[Route("api/corpora")]
public class CorporaController : ControllerBase
{
    private readonly CorpusService _corporaService;

    public CorporaController(CorpusService corporaService) => _corporaService = corporaService;

    // [MOCK] Owner resolved from "mock-user". Replace with HttpContext.User.Identity.Name
    // or a proper claims-based identity once Azure AD auth is configured (FR-NFR-6).
    private string CurrentUser => HttpContext.User.Identity?.Name ?? "mock-user";

    /// <summary>POST /api/corpora — create a new private corpus.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateCorpusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("name is required");

        if (!Enum.TryParse<CorpusSourceType>(request.SourceType, ignoreCase: true, out _))
            return BadRequest($"Invalid source_type '{request.SourceType}'. " +
                              "Valid values: user_upload, api_import, connector");

        var corpus = await _corporaService.CreateAsync(request, CurrentUser);
        return CreatedAtAction(nameof(GetAsync), new { corpusId = corpus.CorpusId },
            ToResponse(corpus));
    }

    /// <summary>GET /api/corpora — list all corpora for the current user.</summary>
    [HttpGet]
    public async Task<IActionResult> ListAsync()
    {
        var corpora = await _corporaService.ListAsync(CurrentUser);
        return Ok(corpora.Select(ToResponse));
    }

    /// <summary>GET /api/corpora/{corpusId} — get corpus metadata.</summary>
    [HttpGet("{corpusId}")]
    public async Task<IActionResult> GetAsync(string corpusId)
    {
        var corpus = await _corporaService.GetAsync(corpusId);
        if (corpus is null) return NotFound();
        return Ok(ToResponse(corpus));
    }

    /// <summary>
    /// DELETE /api/corpora/{corpusId}?confirm=true — delete corpus and all associated data.
    /// Irreversible. Requires explicit confirm=true parameter (FR-1.4).
    /// </summary>
    [HttpDelete("{corpusId}")]
    public async Task<IActionResult> DeleteAsync(string corpusId, [FromQuery] bool confirm = false)
    {
        if (!confirm)
            return BadRequest("Deletion requires confirm=true. This operation is irreversible.");

        var corpus = await _corporaService.GetAsync(corpusId);
        if (corpus is null) return NotFound();

        await _corporaService.DeleteAsync(corpusId);
        return NoContent();
    }

    private static CorpusResponse ToResponse(Corpus c) => new(
        c.CorpusId, c.Name, c.Owner,
        c.Visibility.ToString().ToLowerInvariant(),
        c.SourceType.ToString().ToLowerInvariant(),
        c.Description,
        c.CreatedAt, c.UpdatedAt,
        c.DocumentCount,
        c.IndexStatus.ToString().ToLowerInvariant());
}
