using System.Text;
using AStar.Dev.IdScan.Core;

namespace AStar.Dev.IdScan.Reports;

public static class LifecycleReportGenerator
{
    public static string Generate(IEnumerable<Identifier> identifiers)
    {
        var sb = new StringBuilder();

        _ = sb.AppendLine("# Identifier Lifecycle Summary");
        _ = sb.AppendLine();

        foreach(Identifier id in identifiers.OrderBy(i => i.File).ThenBy(i => i.Line))
        {
            _ = sb.AppendLine($"## `{id.Name}` ({id.Category})");
            _ = sb.AppendLine();
            _ = sb.AppendLine($"**Declared in:** `{id.File}` line {id.Line}");
            _ = sb.AppendLine($"**Type:** `{id.Type}`");
            _ = sb.AppendLine($"**Symbol Kind:** `{id.SymbolKind}`");
            _ = sb.AppendLine();

            // Declaring context
            _ = sb.AppendLine("### Declaring Context");
            _ = sb.AppendLine($"- **Declaring Type:** `{id.DeclaringType}`");
            _ = sb.AppendLine($"- **Declaring Namespace:** `{id.DeclaringNamespace}`");
            _ = sb.AppendLine($"- **Declaring Method:** `{id.DeclaringMethod}`");
            _ = sb.AppendLine();

            // Lifecycle
            _ = sb.AppendLine("### Lifecycle");
            _ = sb.AppendLine($"- **First Usage:** {FormatUsage(id.FirstUsage)}");
            _ = sb.AppendLine($"- **Last Usage:** {FormatUsage(id.LastUsage)}");
            _ = sb.AppendLine($"- **First Write:** {FormatUsage(id.FirstWrite)}");
            _ = sb.AppendLine($"- **Last Write:** {FormatUsage(id.LastWrite)}");
            _ = sb.AppendLine();

            // Behaviour flags
            _ = sb.AppendLine("### Behaviour");
            _ = sb.AppendLine($"- Escapes Method: **{id.EscapesMethod}**");
            _ = sb.AppendLine($"- Returned: **{id.IsReturned}**");
            _ = sb.AppendLine($"- Passed as Argument: **{id.IsPassedAsArgument}**");
            _ = sb.AppendLine($"- Captured by Lambda: **{id.IsCapturedByLambda}**");
            _ = sb.AppendLine($"- Stored in Field: **{id.IsStoredInField}**");
            _ = sb.AppendLine($"- Used in Condition: **{id.IsUsedInCondition}**");
            _ = sb.AppendLine($"- Used in Loop: **{id.IsUsedInLoop}**");
            _ = sb.AppendLine($"- Disposed: **{id.IsDisposed}**");
            _ = sb.AppendLine();

            // Usage table
            if(id.Usages.Count > 0)
            {
                _ = sb.AppendLine("### Usage Timeline");
                _ = sb.AppendLine();
                _ = sb.AppendLine("| File | Line | Method | Usage |");
                _ = sb.AppendLine("|------|------|--------|--------|");

                foreach(IdentifierUsage u in id.Usages.OrderBy(u => u.File).ThenBy(u => u.Line))
                    _ = sb.AppendLine($"| `{u.File}` | {u.Line} | `{u.ContainingMethod}` | {u.UsageKind} |");

                _ = sb.AppendLine();
            }
            else
            {
                _ = sb.AppendLine("### Usage Timeline");
                _ = sb.AppendLine();
                _ = sb.AppendLine("_No usages found — this identifier may be dead code._");
                _ = sb.AppendLine();
            }

            _ = sb.AppendLine("---");
            _ = sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string FormatUsage(IdentifierUsage? usage)
        => usage == null ? "_none_" : $"`{usage.File}` line {usage.Line} ({usage.UsageKind})";
}
