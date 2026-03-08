namespace AStar.Dev.IdScan.Core;

public class NamingSeverityResult
{
    public Identifier Identifier { get; set; } = default!;
    public double Severity { get; set; }
    public List<string> Reasons { get; set; } = new();
}
