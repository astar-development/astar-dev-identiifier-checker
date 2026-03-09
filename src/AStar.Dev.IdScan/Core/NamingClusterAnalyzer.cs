namespace AStar.Dev.IdScan.Core;

public static class NamingClusterAnalyzer
{
    public static List<(NamingCluster Cluster, List<Identifier> Outliers)> FindInconsistencies(
        List<NamingCluster> clusters)
    {
        var results = new List<(NamingCluster, List<Identifier>)>();

        foreach(NamingCluster cluster in clusters)
        {
            var names = cluster.Members.Select(m => m.Name).ToList();

            // Determine dominant pattern
            var dominantPrefix = MostCommonPrefix(names);
            var dominantSuffix = MostCommonSuffix(names);

            var outliers = new List<Identifier>();
            if(cluster.Key.StartsWith("TupleElement"))
            {
                // Tuple elements should be noun-like and descriptive
                outliers.AddRange(cluster.Members.Where(id => id.Name.Length <= 2));
            }

            foreach(Identifier id in cluster.Members)
            {
                var prefixMismatch = dominantPrefix.Length > 0 && !id.Name.StartsWith(dominantPrefix);
                var suffixMismatch = dominantSuffix.Length > 0 && !id.Name.EndsWith(dominantSuffix);

                if(prefixMismatch || suffixMismatch)
                    outliers.Add(id);
            }

            if(outliers.Count > 0)
                results.Add((cluster, outliers));
        }

        return results;
    }

    private static string MostCommonPrefix(List<string> names)
    {
        var prefixes = names
            .Select(n => n.Length > 2 ? n[..2] : "")
            .GroupBy(p => p)
            .OrderByDescending(g => g.Count())
            .First().Key;

        return prefixes;
    }

    private static string MostCommonSuffix(List<string> names)
    {
        var suffixes = names
            .Select(n => n.Length > 2 ? n[^2..] : "")
            .GroupBy(s => s)
            .OrderByDescending(g => g.Count())
            .First().Key;

        return suffixes;
    }
}
