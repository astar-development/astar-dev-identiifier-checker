using System.Text;
using AStar.Dev.IdScan.Core;

namespace AStar.Dev.IdScan.Reports;

public static class NamingSeverityReportGenerator
{
    public static string Generate(IEnumerable<NamingSeverityResult> results)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Naming Severity Report");
        sb.AppendLine();

        foreach(NamingSeverityResult r in results.OrderByDescending(r => r.Severity))
        {
            Identifier id = r.Identifier;

            sb.AppendLine($"## `{id.Name}` — Severity {r.Severity:F2}");
            sb.AppendLine();
            sb.AppendLine($"Declared in `{id.File}` line {id.Line}");
            sb.AppendLine($"Type: `{id.Type}`");
            sb.AppendLine();

            sb.AppendLine("### Reasons");
            foreach(var reason in r.Reasons)
                sb.AppendLine($"- {reason}");

            sb.AppendLine();

            sb.AppendLine("### Recommended Name");
            sb.AppendLine($"`{NamingRecommendationEngine.Recommend(id, results.Select(r => r.Identifier))}`");
            sb.AppendLine();

            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
