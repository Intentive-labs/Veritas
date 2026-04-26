using Microsoft.AspNetCore.Mvc;
using Veritas.Core.Contracts;
using Veritas.Core.Models;
using Veritas.Corpora;
using Veritas.Storage;

namespace Veritas.Api.Controllers;

[ApiController]
[Route("api/corpora/{corpusId}/experiments")]
public class ExperimentsController : ControllerBase
{
    private readonly IDocumentStore _store;

    // [MOCK] Experiment records stored in-memory.
    // Replace with persistent IExperimentRepository backed by Cosmos DB.
    private readonly Dictionary<string, ExperimentRecord> _experiments = new();

    public ExperimentsController(IDocumentStore store) => _store = store;

    private string CurrentUser => HttpContext.User.Identity?.Name ?? "mock-user";

    /// <summary>
    /// POST /api/corpora/{corpusId}/experiments — submit an experiment record.
    /// Records are immutable after submission (FR-3.5).
    /// Tagged with pack_id + pack_version at submission time (FR-3.6).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        string corpusId, [FromBody] CreateExperimentRequest request)
    {
        // [MOCK] Validate request.Parameters against lenr-experiment-schema.skill
        // once that schema is authored by Pekka (FR-3.1, requires Pekka session).
        if (string.IsNullOrWhiteSpace(request.PackId))
            return BadRequest("pack_id is required");
        if (string.IsNullOrWhiteSpace(request.HypothesisVersion))
            return BadRequest("hypothesis_version is required");

        var record = new ExperimentRecord
        {
            ExperimentId = Guid.NewGuid().ToString(),
            CorpusId = corpusId,
            PackId = request.PackId,
            PackVersion = request.PackVersion,
            HypothesisVersion = request.HypothesisVersion,
            SubmittedAt = DateTime.UtcNow,
            SubmittedBy = CurrentUser,
            Parameters = request.Parameters,
            Notes = request.Notes
        };

        _experiments[record.ExperimentId] = record;

        await _store.WriteTextAsync(
            StoragePaths.ExperimentRecord(corpusId, record.ExperimentId),
            System.Text.Json.JsonSerializer.Serialize(record));

        return CreatedAtAction(nameof(GetAsync),
            new { corpusId, experimentId = record.ExperimentId },
            ToResponse(record));
    }

    /// <summary>GET /api/corpora/{corpusId}/experiments/{experimentId}</summary>
    [HttpGet("{experimentId}")]
    public IActionResult GetAsync(string corpusId, string experimentId)
    {
        if (!_experiments.TryGetValue(experimentId, out var record)
            || record.CorpusId != corpusId)
            return NotFound();
        return Ok(ToResponse(record));
    }

    /// <summary>
    /// GET /api/corpora/{corpusId}/experiments/{experimentId}/similar
    /// Returns N most similar corpus documents via weighted Euclidean distance (FR-3.7).
    /// Not LLM-based — deterministic similarity.
    /// </summary>
    [HttpGet("{experimentId}/similar")]
    public IActionResult GetSimilarAsync(string corpusId, string experimentId, [FromQuery] int n = 5)
    {
        if (!_experiments.ContainsKey(experimentId)) return NotFound();

        // [MOCK] Real implementation: compute weighted Euclidean distance between
        // experiment parameters and extracted/normalised corpus document parameters.
        // All comparisons use canonical units from normalization pipeline.
        var mockSimilar = Enumerable.Range(1, n).Select(i => new SimilarDocument(
            $"doc-mock-{i}",
            $"[MOCK] Similar document {i}",
            1.0 - i * 0.15));

        return Ok(new SimilarDocumentsResponse(experimentId, mockSimilar.ToList()));
    }

    private static ExperimentResponse ToResponse(ExperimentRecord r) => new(
        r.ExperimentId, r.CorpusId, r.PackId, r.PackVersion,
        r.HypothesisVersion, r.SubmittedAt, r.SubmittedBy,
        r.Parameters, r.Notes);
}
