using System.Security.Cryptography;
using System.Text.Json;
using Veritas.Core.Contracts;
using Veritas.Core.Models;
using Veritas.Storage;

namespace Veritas.Corpora;

// [MOCK] Document records are stored in-memory.
// Replace with persistent IDocumentRepository backed by Azure Cosmos DB.
// Binary files go to DataLakeDocumentStore (swap to real Azure Data Lake in production).
public class DocumentIngestionService
{
    private readonly IDocumentStore _store;

    // [MOCK] In-memory document records — replace with injected IDocumentRepository.
    private readonly Dictionary<string, VeritasDocument> _documents = new();

    // [MOCK] Idempotency index: corpusId → (sha256 → documentId).
    // Replace with a database unique index on (corpus_id, sha256_hash).
    private readonly Dictionary<string, Dictionary<string, string>> _hashIndex = new();

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static readonly HashSet<string> SupportedFormats =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".docx", ".md", ".txt", ".html" };

    public DocumentIngestionService(IDocumentStore store) => _store = store;

    public static bool IsFormatSupported(string filename)
        => SupportedFormats.Contains(Path.GetExtension(filename));

    public static IReadOnlyCollection<string> GetSupportedFormats() => SupportedFormats;

    /// <summary>
    /// Ingests a document into the corpus. Idempotent: re-uploading the same file (SHA256 match)
    /// to the same corpus returns the existing document with isExisting=true (FR-1.12).
    /// </summary>
    public async Task<(VeritasDocument Document, bool IsExisting)> IngestAsync(
        string corpusId,
        Stream fileContent,
        string filename,
        string rightsDeclaration,
        string uploadedBy,
        string? titleOverride,
        Dictionary<string, string>? metadata,
        CancellationToken ct = default)
    {
        var ext = Path.GetExtension(filename).ToLowerInvariant();
        if (!SupportedFormats.Contains(ext))
            throw new NotSupportedException(
                $"Format '{ext}' not supported. Supported: {string.Join(", ", SupportedFormats)}");

        if (!RightsDeclarationValidator.IsValid(rightsDeclaration))
            throw new ArgumentException($"Invalid rights declaration: {rightsDeclaration}");

        using var ms = new MemoryStream();
        await fileContent.CopyToAsync(ms, ct);
        var bytes = ms.ToArray();
        var hash = ComputeSha256(bytes);

        if (!_hashIndex.TryGetValue(corpusId, out var hashMap))
            _hashIndex[corpusId] = hashMap = new();

        if (hashMap.TryGetValue(hash, out var existingId))
            return (_documents[existingId], true);

        var documentId = Guid.NewGuid().ToString();
        var rawPath = StoragePaths.RawDocument(corpusId, documentId, ext.TrimStart('.'));
        await _store.WriteAsync(rawPath, new MemoryStream(bytes), ct);

        var doc = new VeritasDocument
        {
            DocumentId = documentId,
            CorpusId = corpusId,
            OriginalFilename = filename,
            Format = ext,
            FileSizeBytes = bytes.Length,
            Sha256Hash = hash,
            RightsDeclaration = RightsDeclarationValidator.Parse(rightsDeclaration),
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy,
            TitleOverride = titleOverride,
            UserMetadata = metadata ?? new(),
            Status = DocumentProcessingStatus.Uploaded
        };
        doc.ProcessingHistory.Add($"{doc.UploadedAt:O}: uploaded");

        var sidecar = new DocumentMetadataSidecar(
            documentId, corpusId, filename, ext, bytes.Length, hash,
            rightsDeclaration, doc.UploadedAt, uploadedBy, titleOverride,
            metadata ?? new());

        await _store.WriteTextAsync(
            StoragePaths.RawSidecar(corpusId, documentId),
            JsonSerializer.Serialize(sidecar, JsonOptions), ct);

        _documents[documentId] = doc;
        hashMap[hash] = documentId;
        return (doc, false);
    }

    public Task<IReadOnlyList<VeritasDocument>> ListAsync(
        string corpusId, CancellationToken ct = default)
    {
        IReadOnlyList<VeritasDocument> result =
            _documents.Values.Where(d => d.CorpusId == corpusId).ToList();
        return Task.FromResult(result);
    }

    public Task<VeritasDocument?> GetAsync(string documentId, CancellationToken ct = default)
        => Task.FromResult(_documents.TryGetValue(documentId, out var d) ? d : null);

    public async Task DeleteAsync(
        string corpusId, string documentId, CancellationToken ct = default)
    {
        if (_documents.TryGetValue(documentId, out var doc))
        {
            _documents.Remove(documentId);
            if (_hashIndex.TryGetValue(corpusId, out var hashMap))
                hashMap.Remove(doc.Sha256Hash);
        }
        await _store.DeleteContainerAsync(
            $"/raw/corpora/{corpusId}/documents/{documentId}/", ct);
        await _store.DeleteContainerAsync(
            $"/extracted/corpora/{corpusId}/{documentId}/", ct);
    }

    private static string ComputeSha256(byte[] data)
        => Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();
}
