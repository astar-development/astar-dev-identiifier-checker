using System.Text;

namespace AStar.Dev.IdScan.CLI;

public static class CommandLineParser
{
    public static CommandLineOptions Parse(string[] args)
    {
        var options = new CommandLineOptions();

        for(var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            // --help
            if(arg is "--help" or "-h")
            {
                options.ShowHelp = true;
                return options;
            }

            // --option=value
            if(arg.StartsWith("--") && arg.Contains('='))
            {
                var parts = arg.Split('=', 2);
                SetOption(options, parts[0], parts[1]);
                continue;
            }

            // --option value
            if(!arg.StartsWith("--"))
                continue;
            string? value = null;

            // If next arg exists and is not another flag, treat as value
            if(i + 1 < args.Length && !args[i + 1].StartsWith("--"))
            {
                value = args[i + 1];
                i++;
            }

            SetOption(options, arg, value);
        }

        return options;
    }

    private static void SetOption(CommandLineOptions opts, string name, string? value)
    {
        switch(name)
        {
            case "--csharp":
                opts.CSharpPath = value;
                break;

            case "--typescript":
                opts.TypeScriptPath = value;
                break;

            case "--out-csharp":
                if(!string.IsNullOrWhiteSpace(value))
                    opts.OutCSharp = value;
                break;

            case "--out-typescript":
                if(!string.IsNullOrWhiteSpace(value))
                    opts.OutTypeScript = value;
                break;

            case "--report":
                if(!string.IsNullOrWhiteSpace(value))
                    opts.Report = value;
                break;

            default:
                Console.WriteLine($"Unknown option: {name}");
                break;
        }
    }

    public static void PrintHelp()
    {
        var sb = new StringBuilder();

        _ = sb.AppendLine("Usage:");
        _ = sb.AppendLine("  idscan --csharp <path> [--out-csharp file] [--report file]");
        _ = sb.AppendLine();
        _ = sb.AppendLine("Options:");
        _ = sb.AppendLine("  --csharp <path>           Path to C# source root");
        _ = sb.AppendLine("  --typescript <path>       Path to TS/JS source root (future)");
        _ = sb.AppendLine("  --out-csharp <file>       Output registry file (default: identifier-registry.csharp.json)");
        _ = sb.AppendLine(
            "  --out-typescript <file>   Output TS registry file (default: identifier-registry.typescript.json)");
        _ = sb.AppendLine("  --report <file>           Output Markdown report (default: identifier-report.md)");
        _ = sb.AppendLine("  --help                    Show this help");

        Console.WriteLine(sb.ToString());
    }
}
