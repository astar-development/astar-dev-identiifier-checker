using System.Text;

namespace AStar.Dev.IdScan.Core;

public static class NamingHeatmapReportGenerator
{
    public static string Generate(List<NamingMetrics> metrics)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# 🌡️ Naming Heatmap");
        sb.AppendLine();
        sb.AppendLine("This heatmap shows naming quality across files, classes, and namespaces.");
        sb.AppendLine();

        double globalDebt = metrics.Sum(m => m.SeveritySum);
        double globalAvg = metrics.Average(m => m.Average);

        sb.AppendLine("## 📊 Global Naming Health");
        sb.AppendLine($"- **Total Naming Debt:** {globalDebt:F2}");
        sb.AppendLine($"- **Average Naming Severity:** {globalAvg:F2}");
        sb.AppendLine($"- **Overall Health:** {NamingHeatmapEngine.HeatLevel(globalAvg)}");
        sb.AppendLine();

        sb.AppendLine("## 🔥 File/Class Heatmap");
        sb.AppendLine();
        sb.AppendLine("| File | Class | Namespace | Avg Severity | Heat |");
        sb.AppendLine("|------|--------|-----------|--------------|-------|");

        foreach (var m in metrics.OrderByDescending(m => m.Average))
        {
            sb.AppendLine(
                $"| `{m.File}` | `{m.Class}` | `{m.Namespace}` | {m.Average:F2} | {NamingHeatmapEngine.HeatLevel(m.Average)} |"
            );
        }

        sb.AppendLine();
        return sb.ToString();
    }
}
