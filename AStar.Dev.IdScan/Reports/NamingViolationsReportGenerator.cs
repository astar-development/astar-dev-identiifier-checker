using System.Text;
using AStar.Dev.IdScan.Core;

namespace AStar.Dev.IdScan.Reports;

public static class NamingViolationsReportGenerator
{
    public static string Generate(IEnumerable<NamingRuleResult> results, IEnumerable<Identifier> allIdentifiers)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Naming Violations Report");
        sb.AppendLine();

        foreach(NamingRuleResult r in results.Where(r => r.Violations.Any()))
        {
            Identifier id = r.Identifier;

            sb.AppendLine($"## `{id.Name}` ({id.Category})");
            sb.AppendLine();
            sb.AppendLine($"Declared in `{id.File}` line {id.Line}");
            sb.AppendLine($"Type: `{id.Type}`");
            sb.AppendLine();

            sb.AppendLine("### Violations");
            foreach(var v in r.Violations)
                sb.AppendLine($"- {v}");

            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();

            sb.AppendLine("### Suggested Name");
            sb.AppendLine($"`{NamingSuggestionEngine.Suggest(id)}`");
            sb.AppendLine();

            sb.AppendLine("### Similar Identifiers");
            foreach(IdentifierSimilarity sim in SimilarityEngine.FindSimilar(id, allIdentifiers))
                sb.AppendLine($"- `{sim.Other.Name}` (score {sim.Score:F2})");

            sb.AppendLine();

            sb.AppendLine("### Recommended Name");
            sb.AppendLine($"`{NamingRecommendationEngine.Recommend(id, allIdentifiers)}`");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
