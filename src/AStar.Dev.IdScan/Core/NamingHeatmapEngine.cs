namespace AStar.Dev.IdScan.Core;

public static class NamingHeatmapEngine
{
    public static string HeatLevel(double avg)
        => avg switch
        {
            >= 0.75 => "🔥 Critical",
            >= 0.50 => "⚠️ High",
            >= 0.25 => "🟡 Medium",
            _ => avg > 0.00 ? "🟢 Low" : "⚪ Clean",
        };
}
