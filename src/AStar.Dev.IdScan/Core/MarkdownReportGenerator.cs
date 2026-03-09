using System.Text;

namespace AStar.Dev.IdScan.Core;

public class MarkdownReportGenerator
{
    public void Generate(IdentifierRegistry registry, string path)
    {
        var sb = new StringBuilder();

        _ = sb.AppendLine("| Name | Type | Category | File | First Seen | Status | Notes |");
        _ = sb.AppendLine("|------|------|----------|------|------------|--------|--------|");

        foreach(IdentifierRegistryEntry id in registry.Identifiers.OrderBy(i => i.Name))
        {
            var notes = id.Similarity.Any()
                ? string.Join(", ", id.Similarity.Select(s => $"{s.Other} ({s.Score:F2})"))
                : "";

            _ = sb.AppendLine(
                $"| {id.Name} | {id.Type} | {id.Category} | {id.File}:{id.Line} | {id.FirstDetected:yyyy-MM-dd} | {id.Status} | {notes} |");
        }

        File.WriteAllText(path, sb.ToString());
    }
}
