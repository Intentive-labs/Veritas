using System.Text.Json;
using Veritas.Core.Models;
using Veritas.DomainPacks;
using Veritas.Extraction.Pipeline;
using Veritas.Storage;

namespace Veritas.Extraction.Agents;

// Instructions: .github/agents/orchestrator.md
// Skills: domain-pack-schema.skill
//
// Coordinates: ExtractionAgent → ValidationAgent → NormalizationAgent → ClassificationAgent.
// Retries each step max 2 times before marking the job failed.
// Writes intermediate state after each step — pipeline is resumable.
// Every output tagged with pack_id + pack_version.
public class OrchestratorAgent
{
    private readonly ExtractionAgent _extraction;
    private readonly ValidationAgent _validation;
    private readonly NormalizationAgent _normalization;
    private readonly ClassificationAgent _classification;
    private readonly IDocumentStore _store;
    private readonly IDomainPackRuntime _packRuntime;
    private const int MaxRetries = 2;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public OrchestratorAgent(
        ExtractionAgent extraction,
        ValidationAgent validation,
        NormalizationAgent normalization,
        ClassificationAgent classification,
        IDocumentStore store,
        IDomainPackRuntime packRuntime)
    {
        _extraction = extraction;
        _validation = validation;
        _normalization = normalization;
        _classification = classification;
        _store = store;
        _packRuntime = packRuntime;
    }

    public async Task<bool> RunAsync(
        PipelineJob job, string packDirectory, CancellationToken ct = default)
    {
        job.StartedAt = DateTime.UtcNow;

        DomainPacks.Models.DomainPack pack;
        try
        {
            pack = await _packRuntime.LoadPackAsync(packDirectory);
        }
        catch (Exception ex)
        {
            job.Status = ExtractionPipelineStatus.Failed;
            job.Error = $"Pack load failed: {ex.Message}";
            return false;
        }

        // Step 1: Extraction
        job.Status = ExtractionPipelineStatus.Extracting;
        var extracted = await RunWithRetryAsync(
            () => _extraction.RunAsync(job.DocumentId, job.CorpusId, pack, ct),
            job, "extraction", ct);
        if (extracted is null) return false;

        await WriteIntermediateAsync(job, 1, extracted, ct);
        job.CompletedSteps.Add("extraction");

        // Step 2: Validation
        job.Status = ExtractionPipelineStatus.Validating;
        var validated = await RunWithRetryAsync(
            () => _validation.RunAsync(extracted, pack, ct),
            job, "validation", ct);
        if (validated is null) return false;

        await WriteIntermediateAsync(job, 2, validated, ct);
        job.CompletedSteps.Add("validation");

        if (validated.HasErrors)
        {
            job.Status = ExtractionPipelineStatus.AwaitingHumanReview;
            return false;
        }

        // Step 3: Normalization
        job.Status = ExtractionPipelineStatus.Normalizing;
        var normalized = await RunWithRetryAsync(
            () => _normalization.RunAsync(extracted, pack, ct),
            job, "normalization", ct);
        if (normalized is null) return false;

        await WriteIntermediateAsync(job, 3, normalized, ct);
        job.CompletedSteps.Add("normalization");

        // Step 4: Classification
        job.Status = ExtractionPipelineStatus.Classifying;
        var classified = await RunWithRetryAsync(
            () => _classification.RunAsync(extracted, pack, ct),
            job, "classification", ct);
        if (classified is null) return false;

        await _store.WriteTextAsync(
            StoragePaths.ClassifiedDocument(job.CorpusId, job.PackId, job.PackVersion, job.DocumentId),
            JsonSerializer.Serialize(classified, JsonOptions), ct);

        job.CompletedSteps.Add("classification");
        job.Status = ExtractionPipelineStatus.Validated;
        job.CompletedAt = DateTime.UtcNow;
        return true;
    }

    private async Task<T?> RunWithRetryAsync<T>(
        Func<Task<T>> action, PipelineJob job, string stepName, CancellationToken ct) where T : class
    {
        for (var attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try { return await action(); }
            catch (Exception ex) when (attempt < MaxRetries)
            {
                job.Error = $"{stepName} attempt {attempt + 1} failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                job.Status = ExtractionPipelineStatus.Failed;
                job.Error = $"{stepName} failed after {MaxRetries + 1} attempts: {ex.Message}";
                return null;
            }
        }
        return null;
    }

    private async Task WriteIntermediateAsync(PipelineJob job, int step, object data, CancellationToken ct)
    {
        await _store.WriteTextAsync(
            StoragePaths.ExtractionStep(job.CorpusId, job.DocumentId, step),
            JsonSerializer.Serialize(data, JsonOptions), ct);
    }
}
