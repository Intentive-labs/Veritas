using System.Text.RegularExpressions;
using Veritas.Core.Contracts;
using Veritas.Core.Models;

namespace Veritas.Corpora;

// [MOCK] Corpus records are stored in-memory.
// Replace with a persistent ICorpusRepository backed by Azure Cosmos DB (recommended)
// or Azure SQL Database. Cosmos DB suits well due to schema flexibility and per-partition
// access control by corpus_id.
// Configure: IConfiguration["CosmosDb:ConnectionString"], database "veritas", container "corpora".
public class CorpusService
{
    // [MOCK] In-memory store. Replace with injected ICorpusRepository.
    private readonly Dictionary<string, Corpus> _corpora = new();

    public Task<Corpus> CreateAsync(
        CreateCorpusRequest request, string owner, CancellationToken ct = default)
    {
        var slug = GenerateSlug(request.Name);
        var corpus = new Corpus
        {
            CorpusId = slug,
            Name = request.Name,
            Owner = owner,
            SourceType = Enum.Parse<CorpusSourceType>(request.SourceType, ignoreCase: true),
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _corpora[slug] = corpus;
        return Task.FromResult(corpus);
    }

    public Task<IReadOnlyList<Corpus>> ListAsync(string owner, CancellationToken ct = default)
    {
        IReadOnlyList<Corpus> result = _corpora.Values
            .Where(c => c.Owner == owner)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<Corpus?> GetAsync(string corpusId, CancellationToken ct = default)
        => Task.FromResult(_corpora.TryGetValue(corpusId, out var c) ? c : null);

    public Task<bool> DeleteAsync(string corpusId, CancellationToken ct = default)
        => Task.FromResult(_corpora.Remove(corpusId));

    public Task UpdateIndexStatusAsync(
        string corpusId, IndexStatus status, CancellationToken ct = default)
    {
        if (_corpora.TryGetValue(corpusId, out var corpus))
        {
            corpus.IndexStatus = status;
            corpus.UpdatedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task IncrementDocumentCountAsync(string corpusId, int delta = 1, CancellationToken ct = default)
    {
        if (_corpora.TryGetValue(corpusId, out var corpus))
        {
            corpus.DocumentCount += delta;
            corpus.UpdatedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    private static string GenerateSlug(string name)
    {
        var slug = Regex.Replace(name.ToLowerInvariant().Replace(' ', '-'), @"[^a-z0-9-]", "");
        return $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";
    }
}
