using System.Reflection;
using Microsoft.CodeAnalysis;

namespace AStar.Dev.IdScan.CSharp;

public static class ReferenceLoader
{
    public static IEnumerable<MetadataReference> LoadFrameworkReferences()
    {
        Assembly[] assemblies = new[]
        {
            typeof(object).Assembly, // System.Private.CoreLib
            typeof(Console).Assembly, // System.Console
            typeof(Enumerable).Assembly, // System.Linq
            typeof(List<>).Assembly, // System.Collections
            typeof(Task).Assembly, // System.Threading.Tasks
            typeof(Uri).Assembly, // System.Runtime
            typeof(IAsyncEnumerable<>).Assembly // System.Runtime.Extensions
        };

        return assemblies
            .Distinct()
            .Select(a => MetadataReference.CreateFromFile(a.Location));
    }

    public static IEnumerable<MetadataReference> LoadLocalDlls(string rootPath)
    {
        var dlls = Directory.GetFiles(rootPath, "*.dll", SearchOption.AllDirectories);

        foreach(var dll in dlls)
        {
            MetadataReference? reference = null;

            try
            {
                reference = MetadataReference.CreateFromFile(dll);
            }
            catch
            {
                // ignore native DLLs
            }

            if(reference != null)
                yield return reference;
        }
    }
}
