namespace AStar.Dev.IdScan.Core;

public static class NamingHeatmapEngine
{
    public static string HeatLevel(double avg)
    {
        if(avg >= 0.75)
            return "🔥 Critical";
        if(avg >= 0.50)
            return "⚠️ High";
        if(avg >= 0.25)
            return "🟡 Medium";
        return avg > 0.00 ? "🟢 Low" : "⚪ Clean";
    }
}
