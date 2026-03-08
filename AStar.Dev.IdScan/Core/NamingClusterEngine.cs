using System.Text.RegularExpressions;

namespace AStar.Dev.IdScan.Core;

public static class NamingClusterEngine
{
    public static List<NamingCluster> BuildClusters(IEnumerable<Identifier> identifiers)
    {
        var clusters = new Dictionary<string, NamingCluster>();

        foreach (var id in identifiers)
        {
            var key = ComputeClusterKey(id);

            if (!clusters.TryGetValue(key, out var cluster))
            {
                cluster = new NamingCluster { Key = key };
                clusters[key] = cluster;
            }

            cluster.Members.Add(id);
        }

        return clusters
            .Where(c => c.Value.Members.Count > 1)
            .Select(c => c.Value)
            .ToList();
    }

    private static string ComputeClusterKey(Identifier id)
    {
        // 1. Type-based clustering
        var typeKey = id.Type.Split('.').Last();

        // 2. Lifecycle-based clustering
        var lifecycleKey =
            (id.LastWrite != null ? "mutable" : "immutable") + "_" +
            (id.IsUsedInCondition ? "bool" : "value") + "_" +
            (id.IsUsedInLoop ? "loop" : "single");

        // 3. Prefix/suffix clustering
        var prefix = ExtractPrefix(id.Name);
        var suffix = ExtractSuffix(id.Name);

        if (id.Category == IdentifierCategory.TupleElement)
        {
            return $"TupleElement|{id.DeclaringMethod}|{id.DeclaringType}";
        }

        return $"{typeKey}|{lifecycleKey}|{prefix}|{suffix}";
    }

    private static string ExtractPrefix(string name)
    {
        var match = Regex.Match(name, @"^(is|has|should|get|set|load|update|create|fetch)");
        return match.Success ? match.Value : "";
    }

    private static string ExtractSuffix(string name)
    {
        var match = Regex.Match(name, @"(Id|Dto|List|Collection|Manager|Service)$");
        return match.Success ? match.Value : "";
    }
}
