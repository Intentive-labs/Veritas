namespace Veritas.Storage;

// [MOCK] In-memory implementation of IDocumentStore.
// Replace with Azure Data Lake Gen2 (Azure.Storage.Files.DataLake NuGet package):
//   - Use DataLakeServiceClient authenticated via DefaultAzureCredential (managed identity)
//   - Configure: IConfiguration["DataLake:AccountName"] → "https://{account}.dfs.core.windows.net"
//   - Each corpus_id maps to a separate filesystem (container) for access isolation
//   - Apply Azure RBAC: corpus owner gets "Storage Blob Data Contributor" on their filesystem
//   - See FRD FR-NFR-6, FR-NFR-7 for security requirements
public class DataLakeDocumentStore : IDocumentStore
{
    // [MOCK] Key = virtual path, Value = file bytes. Not thread-safe for concurrent writes.
    // Replace with DataLakeFileClient.UploadAsync / OpenReadAsync calls.
    private readonly Dictionary<string, byte[]> _store = new();

    public Task WriteAsync(string path, Stream content, CancellationToken ct = default)
    {
        using var ms = new MemoryStream();
        content.CopyTo(ms);
        _store[path] = ms.ToArray();
        return Task.CompletedTask;
    }

    public Task WriteTextAsync(string path, string content, CancellationToken ct = default)
    {
        _store[path] = System.Text.Encoding.UTF8.GetBytes(content);
        return Task.CompletedTask;
    }

    public Task<Stream> ReadAsync(string path, CancellationToken ct = default)
    {
        if (!_store.TryGetValue(path, out var bytes))
            throw new FileNotFoundException($"Path not found: {path}");
        return Task.FromResult<Stream>(new MemoryStream(bytes));
    }

    public Task<string> ReadTextAsync(string path, CancellationToken ct = default)
    {
        if (!_store.TryGetValue(path, out var bytes))
            throw new FileNotFoundException($"Path not found: {path}");
        return Task.FromResult(System.Text.Encoding.UTF8.GetString(bytes));
    }

    public Task<bool> ExistsAsync(string path, CancellationToken ct = default)
        => Task.FromResult(_store.ContainsKey(path));

    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        _store.Remove(path);
        return Task.CompletedTask;
    }

    public Task DeleteContainerAsync(string prefix, CancellationToken ct = default)
    {
        var keys = _store.Keys.Where(k => k.StartsWith(prefix)).ToList();
        foreach (var key in keys) _store.Remove(key);
        return Task.CompletedTask;
    }
}
