using System.Text;

namespace AStar.Dev.IdScan.Core;

public static class TopNamingOffendersReportGenerator
{
    public static string Generate(IEnumerable<NamingSeverityResult> results)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# 🚨 Top Naming Offenders");
        sb.AppendLine();
        sb.AppendLine("This report highlights the most problematic identifiers in the codebase, ranked by severity.");
        sb.AppendLine();

        var sorted = results
            .Where(r => r.Severity > 0)
            .OrderByDescending(r => r.Severity)
            .Take(20) // top 20 offenders
            .ToList();

        var totalDebt = results.Sum(r => r.Severity);
        var avgDebt = results.Average(r => r.Severity);

        sb.AppendLine("## 📊 Naming Debt Summary");
        sb.AppendLine($"- **Total Naming Debt:** {totalDebt:F2}");
        sb.AppendLine($"- **Average Severity:** {avgDebt:F2}");
        sb.AppendLine($"- **Worst Offender Severity:** {sorted.FirstOrDefault()?.Severity:F2}");
        sb.AppendLine();

        sb.AppendLine("## 🔥 Worst Offenders");
        sb.AppendLine();

        foreach(NamingSeverityResult r in sorted)
        {
            Identifier id = r.Identifier;

            sb.AppendLine($"### `{id.Name}` — Severity {r.Severity:F2}");
            sb.AppendLine();
            sb.AppendLine($"Declared in `{id.File}` line {id.Line}");
            sb.AppendLine($"Type: `{id.Type}`");
            sb.AppendLine();

            sb.AppendLine("#### Crimes");
            foreach(var reason in r.Reasons)
                sb.AppendLine($"- {reason}");
            sb.AppendLine();

            sb.AppendLine("#### Recommended Name");
            sb.AppendLine($"`{NamingRecommendationEngine.Recommend(id, results.Select(x => x.Identifier))}`");
            sb.AppendLine();

            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
