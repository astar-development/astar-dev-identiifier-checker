namespace AStar.Dev.IdScan.Core;

public class IdentifierMatch
{
    public string Other { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Reason { get; set; } = string.Empty;
}
