using System.Text;
using AStar.Dev.IdScan.Core;

namespace AStar.Dev.IdScan.Reports;

public static class NamingViolationsReportGenerator
{
    public static string Generate(IEnumerable<NamingRuleResult> results, IEnumerable<Identifier> allIdentifiers)
    {
        var sb = new StringBuilder();

        _ = sb.AppendLine("# Naming Violations Report");
        _ = sb.AppendLine();

        foreach(NamingRuleResult r in results.Where(r => r.Violations.Any()))
        {
            Identifier id = r.Identifier;

            _ = sb.AppendLine($"## `{id.Name}` ({id.Category})");
            _ = sb.AppendLine();
            _ = sb.AppendLine($"Declared in `{id.File}` line {id.Line}");
            _ = sb.AppendLine($"Type: `{id.Type}`");
            _ = sb.AppendLine();

            _ = sb.AppendLine("### Violations");
            foreach(var v in r.Violations)
                _ = sb.AppendLine($"- {v}");

            _ = sb.AppendLine();
            _ = sb.AppendLine("---");
            _ = sb.AppendLine();

            _ = sb.AppendLine("### Suggested Name");
            _ = sb.AppendLine($"`{NamingSuggestionEngine.Suggest(id)}`");
            _ = sb.AppendLine();

            _ = sb.AppendLine("### Similar Identifiers");
            foreach(IdentifierSimilarity sim in SimilarityEngine.FindSimilar(id, allIdentifiers))
                _ = sb.AppendLine($"- `{sim.Other.Name}` (score {sim.Score:F2})");

            _ = sb.AppendLine();

            _ = sb.AppendLine("### Recommended Name");
            _ = sb.AppendLine($"`{NamingRecommendationEngine.Recommend(id, allIdentifiers)}`");
            _ = sb.AppendLine();
        }

        return sb.ToString();
    }
}
