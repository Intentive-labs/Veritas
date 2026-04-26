using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Veritas.Rag.Contracts;
using Veritas.Rag.Implementation;
using Veritas.Rag.Models;

namespace Veritas.Api.Controllers;

/// <summary>
/// RAG query endpoints (FR-1.29).
/// </summary>
[ApiController]
[Route("api/corpora/{corpusId}/rag")]
public class RagController(IRagPlugin rag, ILogger<RagController> logger) : ControllerBase
{
    private readonly IRagPlugin _rag = rag;

    /// <summary>
    /// POST /api/corpora/{corpusId}/rag/query
    /// Runs a grounded RAG query scoped to the corpus (FR-1.27–1.32).
    /// Returns answer + sources + disclaimer. Refuses if no evidence found.
    /// </summary>
    [HttpPost("query")]
    public async Task<IActionResult> QueryAsync(
        string corpusId,
        [FromBody] RagQueryRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return BadRequest("query is required");

        RagResponse response;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            response = await _rag.QueryAsync(
                new RagRequest(request.Query, corpusId, request.Filters, request.TopK ?? 10),
                ct);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("RAG query rejected: corpus={CorpusId} reason={Reason}", corpusId, ex.Message);
            return BadRequest(ex.Message);
        }
        sw.Stop();
        logger.LogInformation(
            "RAG query: corpus={CorpusId} sources={SourceCount} refused={Refused} latency={LatencyMs}ms",
            corpusId, response.Sources.Count, response.IsRefused, sw.ElapsedMilliseconds);

        return Ok(new
        {
            answer = response.Answer,
            confidence = response.Confidence,
            sources = response.Sources,
            disclaimer = response.Disclaimer,
            is_refused = response.IsRefused,
            refusal_reason = response.RefusalReason
        });
    }

    /// <summary>
    /// POST /api/corpora/{corpusId}/rag/stream
    /// Streaming RAG query — returns Server-Sent Events.
    /// [MOCK] Returns single chunked response. Replace with Azure OpenAI streaming SDK
    /// once AzureSearchIndexBackend and GroundedAnswerGenerator are wired up.
    /// Real implementation: use IAnswerGenerator with streaming=true + yield chunks.
    /// </summary>
    [HttpPost("stream")]
    public async Task StreamAsync(
        string corpusId,
        [FromBody] RagQueryRequest request,
        CancellationToken ct)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";

        // [MOCK] Run full query then emit as single SSE event
        RagResponse response;
        try
        {
            response = await _rag.QueryAsync(
                new RagRequest(request.Query ?? "", corpusId, request.Filters, request.TopK ?? 10),
                ct);
        }
        catch
        {
            await Response.WriteAsync("event: error\ndata: query failed\n\n", ct);
            return;
        }

        // Emit answer in one event (real implementation would stream tokens)
        await Response.WriteAsync($"event: chunk\ndata: {System.Text.Json.JsonSerializer.Serialize(new { text = response.Answer })}\n\n", ct);
        await Response.WriteAsync($"event: done\ndata: {System.Text.Json.JsonSerializer.Serialize(new { sources = response.Sources, disclaimer = response.Disclaimer, is_refused = response.IsRefused })}\n\n", ct);
    }

    /// <summary>
    /// POST /api/corpora/{corpusId}/rag/search
    /// Keyword/semantic search — returns ranked document list without an LLM answer (FR-1.29).
    /// [MOCK] Returns mock results until AzureSearchIndexBackend is implemented.
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> SearchAsync(
        string corpusId,
        [FromBody] RagSearchRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return BadRequest("query is required");

        // [MOCK] Real implementation: call IIndexBackend.SearchAsync directly (no LLM answer)
        // Returns ranked chunks with score, document metadata, and excerpt.
        // NuGet: Azure.Search.Documents — configure endpoint + key in appsettings.
        await Task.CompletedTask;
        var mockResults = Enumerable.Range(1, request.TopK ?? 10).Select(i => new
        {
            rank = i,
            document_id = $"[MOCK] doc-{i:000}",
            corpus_id = corpusId,
            title = $"[MOCK] Document {i}",
            excerpt = $"[MOCK] Excerpt matching '{request.Query}' from document {i}",
            score = Math.Round(1.0 - (i - 1) * 0.08, 3),
            chunk_id = $"[MOCK] chunk-{i}-0"
        });

        return Ok(new { query = request.Query, corpus_id = corpusId, results = mockResults });
    }

    /// <summary>
    /// GET /api/corpora/{corpusId}/rag/documents/{documentId}
    /// Returns document metadata and available chunks from the index (FR-1.29).
    /// [MOCK] Returns static placeholder until AzureSearchIndexBackend is implemented.
    /// </summary>
    [HttpGet("documents/{documentId}")]
    public async Task<IActionResult> GetDocumentAsync(
        string corpusId, string documentId, CancellationToken ct)
    {
        // [MOCK] Real implementation: query IIndexBackend for all chunks belonging to
        // documentId + corpusId, return document metadata + chunk list with embeddings omitted.
        await Task.CompletedTask;
        return Ok(new
        {
            document_id = documentId,
            corpus_id = corpusId,
            title = "[MOCK] Document title",
            chunk_count = 4,
            chunks = Enumerable.Range(0, 4).Select(i => new
            {
                chunk_id = $"[MOCK] {documentId}-chunk-{i}",
                text = $"[MOCK] Chunk {i} text placeholder",
                start_char = i * 500,
                end_char = (i + 1) * 500
            }),
            note = "[MOCK] Replace with real IIndexBackend.GetDocumentChunksAsync call"
        });
    }

    /// <summary>
    /// GET /api/corpora/{corpusId}/rag/status
    /// Returns index health for the corpus.
    /// [MOCK] Returns static "available" until AzureSearchIndexBackend is implemented.
    /// </summary>
    [HttpGet("status")]
    public IActionResult Status(string corpusId)
    {
        // [MOCK] Real implementation: call IIndexBackend.IsHealthyAsync(corpusId)
        // and return index document count, last indexed timestamp, etc.
        return Ok(new
        {
            corpus_id = corpusId,
            index_status = "available",
            note = "[MOCK] Replace with real index health check via AzureSearchIndexBackend"
        });
    }
}

public record RagSearchRequest(
    string? Query,
    Dictionary<string, string>? Filters = null,
    int? TopK = 10
);

public record RagQueryRequest(
    string? Query,
    Dictionary<string, string>? Filters = null,
    int? TopK = 10
);
