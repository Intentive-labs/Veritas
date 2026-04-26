using Microsoft.AspNetCore.Mvc;
using Veritas.Core.Contracts;

namespace Veritas.Api.Controllers;

[ApiController]
[Route("api/corpora/{corpusId}/hypothesis")]
public class HypothesisController : ControllerBase
{
    /// <summary>
    /// POST /api/corpora/{corpusId}/hypothesis/test
    /// Runs hypothesis testing for the given hypothesis + pack version (FR-4.3).
    /// All findings are correlational only — never causal.
    /// </summary>
    [HttpPost("test")]
    public IActionResult TestAsync(string corpusId, [FromBody] HypothesisTestRequest request)
    {
        // [MOCK] Real implementation:
        //   1. Load classified documents for corpusId + packId + packVersion from Data Lake
        //   2. Load hypothesis definition from pack hypotheses/ directory
        //   3. Count: relevant, supporting, contradicting, inconclusive
        //   4. Compute confidence (high/medium/low) based on n and CI
        //   5. Flag publication bias if applicable
        // This requires Phase 4 Python analysis notebooks (FR-4.1) to be run first.
        var response = new HypothesisTestResponse(
            request.HypothesisId,
            request.PackId,
            request.PackVersion,
            new HypothesisCoverage(0, 0, 0.0),
            new HypothesisFindings(0, 0, 0),
            "low",
            null,
            $"[MOCK] All findings are correlational. Specific to corpus [{corpusId}], " +
            $"pack [{request.PackId}] v[{request.PackVersion}] and its stated assumptions."
        );
        return Ok(response);
    }

    /// <summary>
    /// GET /api/corpora/{corpusId}/hypothesis/compare?packs={id1},{id2}
    /// Compare hypothesis results across packs (FR-4.4).
    /// </summary>
    [HttpGet("compare")]
    public IActionResult CompareAsync(string corpusId, [FromQuery] string packs)
    {
        var packIds = packs?.Split(',') ?? [];
        if (packIds.Length < 2)
            return BadRequest("At least two pack IDs required for comparison");

        // [MOCK] Real implementation: load and diff hypothesis test results per pack
        return Ok(new
        {
            CorpusId = corpusId,
            Packs = packIds,
            Results = packIds.Select(p => new
            {
                PackId = p,
                // [MOCK] Placeholder result
                Confidence = "low",
                Note = "[MOCK] Run hypothesis test per pack first"
            })
        });
    }
}
