namespace AStar.Dev.IdScan.Core;

public class IdentifierUsage
{
    public string File { get; set; } = string.Empty;
    public int Line { get; set; }

    public string? ContainingType { get; set; }
    public string? ContainingMethod { get; set; }

    public string UsageKind { get; set; } = "Unknown"; // Read, Write, Invoke, etc.
}
