using System.Text;
using AStar.Dev.IdScan.Core;

namespace AStar.Dev.IdScan.Reports;

public static class LifecycleReportGenerator
{
    public static string Generate(IEnumerable<Identifier> identifiers)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Identifier Lifecycle Summary");
        sb.AppendLine();

        foreach (var id in identifiers.OrderBy(i => i.File).ThenBy(i => i.Line))
        {
            sb.AppendLine($"## `{id.Name}` ({id.Category})");
            sb.AppendLine();
            sb.AppendLine($"**Declared in:** `{id.File}` line {id.Line}");
            sb.AppendLine($"**Type:** `{id.Type}`");
            sb.AppendLine($"**Symbol Kind:** `{id.SymbolKind}`");
            sb.AppendLine();

            // Declaring context
            sb.AppendLine("### Declaring Context");
            sb.AppendLine($"- **Declaring Type:** `{id.DeclaringType}`");
            sb.AppendLine($"- **Declaring Namespace:** `{id.DeclaringNamespace}`");
            sb.AppendLine($"- **Declaring Method:** `{id.DeclaringMethod}`");
            sb.AppendLine();

            // Lifecycle
            sb.AppendLine("### Lifecycle");
            sb.AppendLine($"- **First Usage:** {FormatUsage(id.FirstUsage)}");
            sb.AppendLine($"- **Last Usage:** {FormatUsage(id.LastUsage)}");
            sb.AppendLine($"- **First Write:** {FormatUsage(id.FirstWrite)}");
            sb.AppendLine($"- **Last Write:** {FormatUsage(id.LastWrite)}");
            sb.AppendLine();

            // Behaviour flags
            sb.AppendLine("### Behaviour");
            sb.AppendLine($"- Escapes Method: **{id.EscapesMethod}**");
            sb.AppendLine($"- Returned: **{id.IsReturned}**");
            sb.AppendLine($"- Passed as Argument: **{id.IsPassedAsArgument}**");
            sb.AppendLine($"- Captured by Lambda: **{id.IsCapturedByLambda}**");
            sb.AppendLine($"- Stored in Field: **{id.IsStoredInField}**");
            sb.AppendLine($"- Used in Condition: **{id.IsUsedInCondition}**");
            sb.AppendLine($"- Used in Loop: **{id.IsUsedInLoop}**");
            sb.AppendLine($"- Disposed: **{id.IsDisposed}**");
            sb.AppendLine();

            // Usage table
            if (id.Usages.Count > 0)
            {
                sb.AppendLine("### Usage Timeline");
                sb.AppendLine();
                sb.AppendLine("| File | Line | Method | Usage |");
                sb.AppendLine("|------|------|--------|--------|");

                foreach (var u in id.Usages.OrderBy(u => u.File).ThenBy(u => u.Line))
                {
                    sb.AppendLine($"| `{u.File}` | {u.Line} | `{u.ContainingMethod}` | {u.UsageKind} |");
                }

                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("### Usage Timeline");
                sb.AppendLine();
                sb.AppendLine("_No usages found — this identifier may be dead code._");
                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string FormatUsage(IdentifierUsage? usage)
    {
        if (usage == null)
            return "_none_";

        return $"`{usage.File}` line {usage.Line} ({usage.UsageKind})";
    }
}
