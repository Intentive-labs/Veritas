using Microsoft.AspNetCore.Mvc;
using Veritas.Core.Contracts;

namespace Veritas.Api.Controllers;

[ApiController]
[Route("api/corpora/{corpusId}")]
public class AnalysisController : ControllerBase
{
    /// <summary>
    /// GET /api/corpora/{corpusId}/compare?packs={pack_id_1},{pack_id_2}
    /// Side-by-side multi-pack classification comparison with agreement flag (FR-2.13).
    /// </summary>
    [HttpGet("compare")]
    public IActionResult CompareAsync(string corpusId, [FromQuery] string packs)
    {
        var packIds = packs?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (packIds.Length < 2)
            return BadRequest("At least two pack IDs required");

        // [MOCK] Real implementation:
        //   1. Load ClassifiedParameters for corpusId per each packId from /classified/ zone
        //   2. For each document: compare outcomes across packs, flag agreement/disagreement
        //   3. List parameter diffs (fields where values differ between pack interpretations)
        //   4. Return filterable, CSV-exportable result (FR-2.14)
        var mockResults = Enumerable.Range(1, 3).Select(i => new MultiPackComparisonResponse(
            corpusId,
            $"doc-mock-{i}",
            packIds.Select(p => new ClassificationResult(
                $"doc-mock-{i}", p, "[MOCK] 0.1.0",
                "[MOCK] inconclusive", 0.0, [])).ToList(),
            Agreement: false,
            ParameterDiffs: ["[MOCK] parameter diff placeholder"]
        ));

        return Ok(mockResults);
    }
}
