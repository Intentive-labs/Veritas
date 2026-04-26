using Microsoft.AspNetCore.Mvc;
using System.Text;
using Veritas.Core.Contracts;

namespace Veritas.Api.Controllers;

[ApiController]
[Route("api/corpora/{corpusId}")]
public class AnalysisController : ControllerBase
{
    /// <summary>
    /// GET /api/corpora/{corpusId}/compare?packs={pack_id_1},{pack_id_2}
    /// Side-by-side multi-pack classification comparison with agreement flag (FR-2.13).
    /// Add Accept: text/csv header to receive a CSV export (FR-2.14).
    /// </summary>
    [HttpGet("compare")]
    public IActionResult CompareAsync(
        string corpusId,
        [FromQuery] string packs,
        [FromHeader(Name = "Accept")] string? accept)
    {
        var packIds = packs?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (packIds.Length < 2)
            return BadRequest("At least two pack IDs required");

        // [MOCK] Real implementation:
        //   1. Load ClassifiedParameters for corpusId per each packId from /classified/ zone
        //   2. For each document: compare outcomes across packs, flag agreement/disagreement
        //   3. List parameter diffs (fields where values differ between pack interpretations)
        var mockResults = Enumerable.Range(1, 5).Select(i => new MultiPackComparisonResponse(
            corpusId,
            $"doc-mock-{i}",
            packIds.Select(p => new ClassificationResult(
                $"doc-mock-{i}", p, "[MOCK] 0.1.0",
                i % 3 == 0 ? "positive" : "inconclusive", 0.65, [])).ToList(),
            Agreement: i % 3 != 0,
            ParameterDiffs: i % 3 != 0 ? [] : ["[MOCK] loading_ratio differs by 0.04"]
        )).ToList();

        // CSV export (FR-2.14)
        if (accept?.Contains("text/csv", StringComparison.OrdinalIgnoreCase) == true)
        {
            var csv = new StringBuilder();
            csv.AppendLine("corpus_id,document_id,pack_id,pack_version,outcome,confidence,agreement,parameter_diffs");
            foreach (var row in mockResults)
            {
                foreach (var cls in row.Results)
                {
                    var diffs = string.Join("|", row.ParameterDiffs).Replace(",", ";");
                    csv.AppendLine($"{row.CorpusId},{row.DocumentId},{cls.PackId},{cls.PackVersion},{cls.Outcome},{cls.OutcomeConfidence},{row.Agreement},{diffs}");
                }
            }
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"compare-{corpusId}.csv");
        }

        return Ok(mockResults);
    }
}
