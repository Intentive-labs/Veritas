using Veritas.Rag.Models;

namespace Veritas.Rag.Implementation;

// LENR-specific search filters.
// Applied on top of the mandatory corpus_id filter to narrow AI Search queries
// to physically relevant parameter ranges.
//
// [MOCK] Real filter values require the physicist ontology session.
// Replace the placeholder field names and ranges with actual LENR parameter names
// once the ontology.json is authored.
//
// In Azure AI Search these become $filter OData expressions:
//   e.g. "corpus_id eq '{id}' and placeholder_parameter_1 ge 0.5"
public static class LenrSearchFilters
{
    // [MOCK] Replace field name with actual LENR parameter from ontology.json
    public const string FieldPlaceholder1 = "placeholder_parameter_1";

    // [MOCK] Replace field name with actual LENR parameter from ontology.json
    public const string FieldPlaceholder2 = "placeholder_parameter_2";

    /// <summary>
    /// Builds base filters for a LENR corpus query.
    /// Always includes corpus_id; adds any caller-supplied filters.
    /// </summary>
    public static Dictionary<string, string> BuildFilters(
        string corpusId,
        Dictionary<string, string>? callerFilters = null)
    {
        var filters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["corpus_id"] = corpusId,
            // [MOCK] Add default LENR domain filters here after physicist session.
            // Example when real parameters are known:
            //   [FieldPlaceholder1] = "ge:0.5"
        };

        if (callerFilters is not null)
            foreach (var (key, value) in callerFilters)
                filters[key] = value;

        return filters;
    }
}
