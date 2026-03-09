using System.Text;

namespace AStar.Dev.IdScan.Core;

public static class TopNamingOffendersReportGenerator
{
    public static string Generate(IEnumerable<NamingSeverityResult> results)
    {
        var sb = new StringBuilder();

        _ = sb.AppendLine("# 🚨 Top Naming Offenders");
        _ = sb.AppendLine();
        _ = sb.AppendLine("This report highlights the most problematic identifiers in the codebase, ranked by severity.");
        _ = sb.AppendLine();

        var sorted = results
            .Where(r => r.Severity > 0)
            .OrderByDescending(r => r.Severity)
            .Take(20) // top 20 offenders
            .ToList();

        var totalDebt = results.Sum(r => r.Severity);
        var avgDebt = results.Average(r => r.Severity);

        _ = sb.AppendLine("## 📊 Naming Debt Summary");
        _ = sb.AppendLine($"- **Total Naming Debt:** {totalDebt:F2}");
        _ = sb.AppendLine($"- **Average Severity:** {avgDebt:F2}");
        _ = sb.AppendLine($"- **Worst Offender Severity:** {sorted.FirstOrDefault()?.Severity:F2}");
        _ = sb.AppendLine();

        _ = sb.AppendLine("## 🔥 Worst Offenders");
        _ = sb.AppendLine();

        foreach(NamingSeverityResult r in sorted)
        {
            Identifier id = r.Identifier;

            _ = sb.AppendLine($"### `{id.Name}` — Severity {r.Severity:F2}");
            _ = sb.AppendLine();
            _ = sb.AppendLine($"Declared in `{id.File}` line {id.Line}");
            _ = sb.AppendLine($"Type: `{id.Type}`");
            _ = sb.AppendLine();

            _ = sb.AppendLine("#### Crimes");
            foreach(var reason in r.Reasons)
                _ = sb.AppendLine($"- {reason}");
            _ = sb.AppendLine();

            _ = sb.AppendLine("#### Recommended Name");
            _ = sb.AppendLine($"`{NamingRecommendationEngine.Recommend(id, results.Select(x => x.Identifier))}`");
            _ = sb.AppendLine();

            _ = sb.AppendLine("---");
            _ = sb.AppendLine();
        }

        return sb.ToString();
    }
}
