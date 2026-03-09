using System.Text;

namespace AStar.Dev.IdScan.Core;

public static class NamingClusterReportGenerator
{
    public static string Generate(
        List<NamingCluster> clusters,
        List<(NamingCluster Cluster, List<Identifier> Outliers)> inconsistencies)
    {
        var sb = new StringBuilder();

        _ = sb.AppendLine("# 🧩 Naming Clusters Report");
        _ = sb.AppendLine();
        _ = sb.AppendLine(
            "This report identifies groups of identifiers that behave similarly but are named inconsistently.");
        _ = sb.AppendLine();

        foreach((NamingCluster cluster, List<Identifier> outliers) in inconsistencies)
        {
            _ = sb.AppendLine($"## Cluster: `{cluster.Key}`");
            _ = sb.AppendLine();

            _ = sb.AppendLine("### Members");
            foreach(Identifier m in cluster.Members)
            {
                _ = sb.AppendLine($"- `{m.Name}` " +
                              $"(in `{m.File}` line {m.Line}, " +
                              $"class `{m.DeclaringType}`, " +
                              $"method `{m.DeclaringMethod}`)");
            }

            _ = sb.AppendLine();

            _ = sb.AppendLine("### Outliers");
            foreach(Identifier o in outliers)
            {
                _ = sb.AppendLine($"- `{o.Name}` " +
                              $"(in `{o.File}` line {o.Line}, " +
                              $"class `{o.DeclaringType}`, " +
                              $"method `{o.DeclaringMethod}`)");
            }

            _ = sb.AppendLine();

            _ = sb.AppendLine("### Suggested Fixes");
            foreach(Identifier o in outliers)
            {
                var suggestion = NamingRecommendationEngine.Recommend(o, cluster.Members);
                _ = sb.AppendLine($"- `{o.Name}` → `{suggestion}`");
            }

            _ = sb.AppendLine();
            _ = sb.AppendLine("---");
            _ = sb.AppendLine();
        }

        return sb.ToString();
    }
}
