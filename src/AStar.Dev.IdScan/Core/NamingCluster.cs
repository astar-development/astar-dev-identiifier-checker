namespace AStar.Dev.IdScan.Core;

public class NamingCluster
{
    public string Key { get; set; } = string.Empty;
    public List<Identifier> Members { get; set; } = new();
}
