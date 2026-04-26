namespace Veritas.Core.Models;

public enum CorpusVisibility { Private }

public enum CorpusSourceType { UserUpload, ApiImport, Connector }

public enum IndexStatus { Pending, Indexing, Ready, Error }

public class Corpus
{
    public required string CorpusId { get; init; }
    public required string Name { get; set; }
    public required string Owner { get; init; }
    public CorpusVisibility Visibility { get; init; } = CorpusVisibility.Private;
    public required CorpusSourceType SourceType { get; init; }
    public string? Description { get; set; }
    public required DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }
    public int DocumentCount { get; set; }
    public IndexStatus IndexStatus { get; set; } = IndexStatus.Pending;
}
