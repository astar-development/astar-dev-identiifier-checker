using System.Text;

namespace AStar.Dev.IdScan.Core;

public static class NamingHeatmapReportGenerator
{
    public static string Generate(List<NamingMetrics> metrics)
    {
        var sb = new StringBuilder();

        _ = sb.AppendLine("# 🌡️ Naming Heatmap");
        _ = sb.AppendLine();
        _ = sb.AppendLine("This heatmap shows naming quality across files, classes, and namespaces.");
        _ = sb.AppendLine();

        var globalDebt = metrics.Sum(m => m.SeveritySum);
        var globalAvg = metrics.Average(m => m.Average);

        _ = sb.AppendLine("## 📊 Global Naming Health");
        _ = sb.AppendLine($"- **Total Naming Debt:** {globalDebt:F2}");
        _ = sb.AppendLine($"- **Average Naming Severity:** {globalAvg:F2}");
        _ = sb.AppendLine($"- **Overall Health:** {NamingHeatmapEngine.HeatLevel(globalAvg)}");
        _ = sb.AppendLine();

        _ = sb.AppendLine("## 🔥 File/Class Heatmap");
        _ = sb.AppendLine();
        _ = sb.AppendLine("| File | Class | Namespace | Avg Severity | Heat |");
        _ = sb.AppendLine("|------|--------|-----------|--------------|-------|");

        foreach(NamingMetrics m in metrics.OrderByDescending(m => m.Average))
        {
            _ = sb.AppendLine(
                $"| `{m.File}` | `{m.Class}` | `{m.Namespace}` | {m.Average:F2} | {NamingHeatmapEngine.HeatLevel(m.Average)} |"
            );
        }

        _ = sb.AppendLine();
        return sb.ToString();
    }
}
