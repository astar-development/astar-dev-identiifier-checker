namespace AStar.Dev.IdScan.Core;

public class NamingMetrics
{
    public string File { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string Class { get; set; } = "";
    public double SeveritySum { get; set; }
    public int Count { get; set; }
    public double Average => Count == 0 ? 0 : SeveritySum / Count;
}

public static class NamingMetricsEngine
{
    public static List<NamingMetrics> Compute(IEnumerable<NamingSeverityResult> results)
    {
        var metrics = new Dictionary<string, NamingMetrics>();

        foreach(NamingSeverityResult r in results)
        {
            Identifier? id = r.Identifier;
            var key = $"{id?.File ?? "Unknown"}|{id?.DeclaringNamespace ?? "Unknown"}|{id?.DeclaringType ?? "Unknown"}";

            if(!metrics.TryGetValue(key, out NamingMetrics? m))
            {
                m = new NamingMetrics
                {
                    File = id?.File ?? "Unknown",
                    Namespace = id?.DeclaringNamespace ?? "Unknown",
                    Class = id?.DeclaringType ?? "Unknown"
                };
                metrics[key] = m;
            }

            m.SeveritySum += r.Severity;
            m.Count++;
        }

        return metrics.Values.ToList();
    }
}
