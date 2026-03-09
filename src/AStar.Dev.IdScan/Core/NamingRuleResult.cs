namespace AStar.Dev.IdScan.Core;

public class NamingRuleResult
{
    public Identifier Identifier { get; set; } = default!;
    public List<string> Violations { get; set; } = new();
}
