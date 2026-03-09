namespace AStar.Dev.IdScan.Core;

public static class NamingPatternEngine
{
    public static string? InferPattern(IEnumerable<IdentifierSimilarity> similar)
    {
        var names = similar.Select(s => s.Other.Name).ToList();

        if(names.Count == 0)
            return null;

        // If all end with "Id"
        if(names.All(n => n.EndsWith("Id")))
            return "{prefix}Id";

        // If all start with "is"/"has"
        if(names.All(n => n.StartsWith("is") || n.StartsWith("has")))
            return "is{Noun}";

        // If all are plural
        if(names.All(n => n.EndsWith("s")))
            return "{noun}s";

        // Fallback: use the longest common substring
        var common = LongestCommonSubstring(names);
        return common.Length > 2 ? common + "{suffix}" : null;
    }

    private static string LongestCommonSubstring(List<string> strings)
    {
        if(strings.Count == 0)
            return "";

        var first = strings[0];

        for(var len = first.Length; len > 0; len--)
        {
            for(var start = 0; start + len <= first.Length; start++)
            {
                var substr = first.Substring(start, len);

                if(strings.All(s => s.Contains(substr)))
                    return substr;
            }
        }

        return "";
    }
}
