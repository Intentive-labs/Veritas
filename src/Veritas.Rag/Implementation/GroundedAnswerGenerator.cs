using Veritas.Rag.Contracts;
using Veritas.Rag.Models;

namespace Veritas.Rag.Implementation;

// [MOCK] Grounded answer generator stub — returns a mock answer.
// Replace with Azure OpenAI GPT-4o chat completion call:
//   - NuGet: Azure.AI.OpenAI
//   - Endpoint: IConfiguration["AzureOpenAI:Endpoint"]  
//   - Deployment: IConfiguration["AzureOpenAI:DeploymentName"] (e.g. "gpt-4o")
//   - Auth: DefaultAzureCredential (managed identity — FR-NFR-8)
//
// System prompt MUST instruct the model to:
//   1. Only use information from the provided retrieved chunks
//   2. Cite every claim with [chunk-id] inline (e.g. "The field was 2.5 T [chunk-abc123]")
//   3. Refuse with IsRefused=true if chunks do not support an answer
//   4. Never fabricate citations or invent content
// See rag-plugin-contract.skill for full grounding contract.
public class GroundedAnswerGenerator : IAnswerGenerator
{
    public Task<GeneratedAnswer> GenerateAsync(
        string query, IReadOnlyList<RetrievedChunk> chunks,
        CorpusConfig config, CancellationToken ct = default)
    {
        if (chunks.Count == 0)
            return Task.FromResult(new GeneratedAnswer(
                "", "low", true,
                "No relevant documents found in corpus to answer this query"));

        // [MOCK] Replace with Azure OpenAI chat completion call.
        var chunkRefs = string.Join(", ", chunks.Select(c => $"[{c.ChunkId}]"));
        var mockAnswer =
            $"[MOCK] Based on {chunks.Count} retrieved documents from corpus '{config.CorpusId}': " +
            $"{query} (Citations: {chunkRefs})";

        return Task.FromResult(new GeneratedAnswer(mockAnswer, "medium", false, null));
    }
}
