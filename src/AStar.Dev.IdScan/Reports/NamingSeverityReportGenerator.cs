using System.Text;
using AStar.Dev.IdScan.Core;

namespace AStar.Dev.IdScan.Reports;

public static class NamingSeverityReportGenerator
{
    public static string Generate(IEnumerable<NamingSeverityResult> results)
    {
        var sb = new StringBuilder();

        _ = sb.AppendLine("# Naming Severity Report");
        _ = sb.AppendLine();

        foreach(NamingSeverityResult r in results.OrderByDescending(r => r.Severity))
        {
            Identifier id = r.Identifier;

            _ = sb.AppendLine($"## `{id.Name}` — Severity {r.Severity:F2}");
            _ = sb.AppendLine();
            _ = sb.AppendLine($"Declared in `{id.File}` line {id.Line}");
            _ = sb.AppendLine($"Type: `{id.Type}`");
            _ = sb.AppendLine();

            _ = sb.AppendLine("### Reasons");
            foreach(var reason in r.Reasons)
                _ = sb.AppendLine($"- {reason}");

            _ = sb.AppendLine();

            _ = sb.AppendLine("### Recommended Name");
            _ = sb.AppendLine($"`{NamingRecommendationEngine.Recommend(id, results.Select(r => r.Identifier))}`");
            _ = sb.AppendLine();

            _ = sb.AppendLine("---");
            _ = sb.AppendLine();
        }

        return sb.ToString();
    }
}
