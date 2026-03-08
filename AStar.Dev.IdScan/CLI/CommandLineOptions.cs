namespace AStar.Dev.IdScan.CLI;

public class CommandLineOptions
{
    public string? CSharpPath { get; set; }
    public string? TypeScriptPath { get; set; }

    public string OutCSharp { get; set; } = "identifier-registry.csharp.json";
    public string OutTypeScript { get; set; } = "identifier-registry.typescript.json";
    public string Report { get; set; } = "identifier-report.md";

    public bool ShowHelp { get; set; }
}
