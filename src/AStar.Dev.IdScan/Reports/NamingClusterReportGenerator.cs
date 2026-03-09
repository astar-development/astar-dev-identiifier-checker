using System.Text;

namespace AStar.Dev.IdScan.Core;

public static class NamingClusterReportGenerator
{
    public static string Generate(
        List<NamingCluster> clusters,
        List<(NamingCluster Cluster, List<Identifier> Outliers)> inconsistencies)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# 🧩 Naming Clusters Report");
        sb.AppendLine();
        sb.AppendLine(
            "This report identifies groups of identifiers that behave similarly but are named inconsistently.");
        sb.AppendLine();

        foreach((NamingCluster cluster, List<Identifier> outliers) in inconsistencies)
        {
            sb.AppendLine($"## Cluster: `{cluster.Key}`");
            sb.AppendLine();

            sb.AppendLine("### Members");
            foreach(Identifier m in cluster.Members)
            {
                sb.AppendLine($"- `{m.Name}` " +
                              $"(in `{m.File}` line {m.Line}, " +
                              $"class `{m.DeclaringType}`, " +
                              $"method `{m.DeclaringMethod}`)");
            }

            sb.AppendLine();

            sb.AppendLine("### Outliers");
            foreach(Identifier o in outliers)
            {
                sb.AppendLine($"- `{o.Name}` " +
                              $"(in `{o.File}` line {o.Line}, " +
                              $"class `{o.DeclaringType}`, " +
                              $"method `{o.DeclaringMethod}`)");
            }

            sb.AppendLine();

            sb.AppendLine("### Suggested Fixes");
            foreach(Identifier o in outliers)
            {
                var suggestion = NamingRecommendationEngine.Recommend(o, cluster.Members);
                sb.AppendLine($"- `{o.Name}` → `{suggestion}`");
            }

            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
