using System.Text.Json;

namespace AStar.Dev.IdScan.Core;

public class IdentifierRegistry
{
    public List<IdentifierRegistryEntry> Identifiers { get; set; } = new();

    public static IdentifierRegistry Load(string path)
    {
        if(!File.Exists(path))
            return new IdentifierRegistry();

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<IdentifierRegistry>(json)
               ?? new IdentifierRegistry();
    }

    public void Save(string path)
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(path, json);
    }
}

public class IdentifierRegistryEntry
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public IdentifierCategory Category { get; set; }
    public string File { get; set; } = string.Empty;
    public int Line { get; set; }
    public DateTime FirstDetected { get; set; }
    public string Status { get; set; } = "To Check";
    public List<IdentifierMatch> Similarity { get; set; } = new();
}
