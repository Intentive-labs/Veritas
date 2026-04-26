namespace Veritas.Storage;

public interface IDocumentStore
{
    Task WriteAsync(string path, Stream content, CancellationToken ct = default);
    Task WriteTextAsync(string path, string content, CancellationToken ct = default);
    Task<Stream> ReadAsync(string path, CancellationToken ct = default);
    Task<string> ReadTextAsync(string path, CancellationToken ct = default);
    Task<bool> ExistsAsync(string path, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
    Task DeleteContainerAsync(string prefix, CancellationToken ct = default);
}
