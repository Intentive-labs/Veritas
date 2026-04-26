using System.Text.RegularExpressions;
using Veritas.Rag.Contracts;
using Veritas.Rag.Models;

namespace Veritas.Rag.Implementation;

/// <summary>
/// Verifies every [chunk-id] citation in the answer exists in the retrieved chunks.
/// Never fabricates citations — if a citation is not in retrieved set, it is flagged.
/// </summary>
public class CitationValidator : ICitationValidator
{
    private static readonly Regex CitationPattern = new(@"\[([^\]]+)\]", RegexOptions.Compiled);

    public CitationValidationResult Validate(string answer, IReadOnlyList<RetrievedChunk> chunks)
    {
        var chunkIds = chunks.Select(c => c.ChunkId).ToHashSet();
        var ungrounded = new List<string>();

        foreach (Match match in CitationPattern.Matches(answer))
        {
            var cited = match.Groups[1].Value;
            if (!chunkIds.Contains(cited))
                ungrounded.Add(cited);
        }

        return new CitationValidationResult(ungrounded.Count == 0, ungrounded);
    }
}
