using AStar.Dev.IdScan.CLI;
using AStar.Dev.IdScan.Core;
using AStar.Dev.IdScan.CSharp;
using AStar.Dev.IdScan.Reports;

namespace AStar.Dev.IdScan;

public class Program
{
    public static int Main(string[] args)
    {
        CommandLineOptions options = CommandLineParser.Parse(args);

        if(options.ShowHelp)
        {
            CommandLineParser.PrintHelp();
            return 0;
        }

        var identifiers = new List<Identifier>();

        // C# scanning
        if(!string.IsNullOrWhiteSpace(options.CSharpPath))
        {
            if(!Directory.Exists(options.CSharpPath))
                Console.WriteLine($"❌ C# path does not exist: {options.CSharpPath}");
            else
            {
                Console.WriteLine($"🔍 Scanning C# source: {options.CSharpPath}");
                var scanner = new CSharpScanner();
                identifiers.AddRange(scanner.Scan(options.CSharpPath));
            }
        }

        // TS scanning (future)
        if(!string.IsNullOrWhiteSpace(options.TypeScriptPath))
            Console.WriteLine("⚠ TypeScript scanning not implemented yet.");

        // Load registry
        var registry = IdentifierRegistry.Load(options.OutCSharp);

        // Update registry
        var updater = new RegistryUpdater();
        updater.UpdateRegistry(registry, identifiers);

        // Save registry
        registry.Save(options.OutCSharp);
        Console.WriteLine($"💾 Saved registry: {options.OutCSharp}");

        // Generate report
        var reportGen = new MarkdownReportGenerator();
        reportGen.Generate(registry, options.Report);
        Console.WriteLine($"📄 Generated report: {options.Report}");

        var lifecycleReport = LifecycleReportGenerator.Generate(identifiers);
        File.WriteAllText("identifier-lifecycle.md", lifecycleReport);
        Console.WriteLine("🧬 Generated lifecycle report: identifier-lifecycle.md");

        var namingResults = identifiers
            .Select(NamingRulesEngine.Evaluate)
            .ToList();

        var namingReport = NamingViolationsReportGenerator.Generate(namingResults, identifiers);
        File.WriteAllText("naming-violations.md", namingReport);

        Console.WriteLine("🔤 Generated naming violations report: naming-violations.md");

        var severityResults = identifiers
            .Select(id => NamingSeverityEngine.Evaluate(id, identifiers))
            .ToList();

        var severityReport = NamingSeverityReportGenerator.Generate(severityResults);
        File.WriteAllText("naming-severity.md", severityReport);

        Console.WriteLine("🔥 Generated naming severity report: naming-severity.md");

        var topOffendersReport = TopNamingOffendersReportGenerator.Generate(severityResults);
        File.WriteAllText("naming-top-offenders.md", topOffendersReport);

        Console.WriteLine("🚨 Generated top naming offenders report: naming-top-offenders.md");

        List<NamingCluster> clusters = NamingClusterEngine.BuildClusters(identifiers);
        List<(NamingCluster Cluster, List<Identifier> Outliers)> inconsistencies =
            NamingClusterAnalyzer.FindInconsistencies(clusters);

        var clusterReport = NamingClusterReportGenerator.Generate(clusters, inconsistencies);
        File.WriteAllText("naming-clusters.md", clusterReport);

        Console.WriteLine("🧩 Generated naming clusters report: naming-clusters.md");

        List<NamingMetrics> metrics = NamingMetricsEngine.Compute(severityResults);
        var heatmap = NamingHeatmapReportGenerator.Generate(metrics);
        File.WriteAllText("naming-heatmap.md", heatmap);

        Console.WriteLine("🌡️ Generated naming heatmap: naming-heatmap.md");

        return 0;
    }
}
