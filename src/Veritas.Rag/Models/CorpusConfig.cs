namespace Veritas.Rag.Models;

public record CorpusConfig
{
    public required string CorpusId { get; init; }
    public required string IndexName { get; init; }

    // [MOCK] Chunking parameters — values below are placeholders only.
    // The search expert must determine: ChunkingStrategy, ChunkSize, ChunkOverlap,
    // EmbeddingModel, and scoring profiles before these are used in production.
    // See FRD FR-1.23 and the search expert session document.
    public required string ChunkingStrategy { get; init; }
    public required int ChunkSize { get; init; }
    public required int ChunkOverlap { get; init; }
    public required string EmbeddingModel { get; init; }

    public required string Disclaimer { get; init; }
    public bool IsPublic { get; init; }
    public Dictionary<string, string> DefaultFilters { get; init; } = new();
}
