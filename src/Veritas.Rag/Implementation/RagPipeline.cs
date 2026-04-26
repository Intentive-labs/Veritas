using Veritas.Rag.Contracts;
using Veritas.Rag.Models;

namespace Veritas.Rag.Implementation;

/// <summary>
/// Orchestrates retrieval → answer generation → citation validation.
/// All queries are strictly scoped to the registered corpus (FR-1.30).
/// Refuses when evidence is insufficient (FR-1.31).
/// </summary>
public class RagPipeline : IRagPlugin
{
    private readonly IRetriever _retriever;
    private readonly IAnswerGenerator _answerGenerator;
    private readonly ICitationValidator _citationValidator;
    private readonly ICorpusConnector _corpus;

    public RagPipeline(
        IRetriever retriever,
        IAnswerGenerator answerGenerator,
        ICitationValidator citationValidator,
        ICorpusConnector corpus)
    {
        _retriever = retriever;
        _answerGenerator = answerGenerator;
        _citationValidator = citationValidator;
        _corpus = corpus;
    }

    public async Task<RagResponse> QueryAsync(RagRequest request, CancellationToken ct = default)
    {
        if (request.CorpusId != _corpus.CorpusId)
            throw new InvalidOperationException(
                $"Corpus mismatch: pipeline scoped to '{_corpus.CorpusId}', got '{request.CorpusId}'");

        if (!await _corpus.IsAvailableAsync(ct))
            return new RagResponse(
                "", "low", [], _corpus.Config.Disclaimer, true, "Corpus unavailable");

        var chunks = await _retriever.RetrieveAsync(
            request.Query, _corpus, request.TopK, request.Filters, ct);

        if (chunks.Count == 0)
            return new RagResponse(
                "", "low", [], _corpus.Config.Disclaimer, true,
                "No relevant documents found in corpus");

        var generated = await _answerGenerator.GenerateAsync(
            request.Query, chunks, _corpus.Config, ct);

        if (generated.IsRefused)
            return new RagResponse(
                "", generated.Confidence, [], _corpus.Config.Disclaimer,
                true, generated.RefusalReason);

        var citationResult = _citationValidator.Validate(generated.Text, chunks);

        var sources = chunks
            .Select(c => new RagSource(
                c.DocumentId, c.Title, c.ChunkId,
                c.Text[..Math.Min(200, c.Text.Length)]))
            .ToList();

        return new RagResponse(
            generated.Text, generated.Confidence,
            sources, _corpus.Config.Disclaimer,
            false, null);
    }
}
