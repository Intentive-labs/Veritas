using Veritas.Extraction.Pipeline;

namespace Veritas.Extraction.Pipeline;

// [MOCK] In-memory job queue and store.
// Replace with Azure Service Bus (durable messaging) + Azure Cosmos DB (job state):
//   - Enqueue: ServiceBusClient.CreateSender().SendMessageAsync()
//   - Dequeue: ServiceBusProcessor or ServiceBusReceiver.ReceiveMessageAsync()
//   - Job state: Cosmos DB container "extraction-jobs", keyed by job_id
//   - Configure: IConfiguration["ServiceBus:ConnectionString"]
public class ExtractionQueue
{
    // [MOCK] In-memory job store — replace with Cosmos DB repository.
    private readonly Dictionary<string, PipelineJob> _jobs = new();
    private readonly Queue<string> _pending = new();

    public Task<PipelineJob> EnqueueAsync(
        string documentId, string corpusId,
        string packId, string packVersion,
        CancellationToken ct = default)
    {
        var job = new PipelineJob
        {
            JobId = Guid.NewGuid().ToString(),
            DocumentId = documentId,
            CorpusId = corpusId,
            PackId = packId,
            PackVersion = packVersion,
            CreatedAt = DateTime.UtcNow
        };
        _jobs[job.JobId] = job;
        _pending.Enqueue(job.JobId);
        return Task.FromResult(job);
    }

    public Task<PipelineJob?> DequeueAsync(CancellationToken ct = default)
    {
        if (_pending.TryDequeue(out var jobId) && _jobs.TryGetValue(jobId, out var job))
            return Task.FromResult<PipelineJob?>(job);
        return Task.FromResult<PipelineJob?>(null);
    }

    public Task<PipelineJob?> GetJobAsync(string jobId, CancellationToken ct = default)
        => Task.FromResult(_jobs.TryGetValue(jobId, out var job) ? job : null);

    public Task UpdateJobAsync(PipelineJob job, CancellationToken ct = default)
    {
        _jobs[job.JobId] = job;
        return Task.CompletedTask;
    }
}
