namespace AStar.Dev.IdScan.Core;

public static class NamingRecommendationEngine
{
    public static string Recommend(Identifier id, IEnumerable<Identifier> all)
    {
        List<IdentifierSimilarity> similar = SimilarityEngine.FindSimilar(id, all);
        var pattern = NamingPatternEngine.InferPattern(similar);

        // 1. Try pattern-based recommendation
        if(pattern != null)
        {
            var name = ApplyPattern(id.Name, pattern);
            if(name != id.Name)
                return name;
        }

        // 2. Try lifecycle-based suggestion
        var lifecycleSuggestion = NamingSuggestionEngine.Suggest(id);
        if(lifecycleSuggestion != id.Name)
            return lifecycleSuggestion;

        // 3. Try similarity-based prefix/suffix extraction
        var clusterName = InferClusterName(similar, id);
        if(clusterName != id.Name)
            return clusterName;

        // 4. Fallback: append a meaningful suffix
        return id.Name + "_renamed";
    }

    private static string Capitalize(string s)
    {
        return string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];
    }

    private static string ApplyPattern(string name, string pattern)
    {
        if(pattern == "{prefix}Id")
            return name + "Id";

        if(pattern == "is{Noun}")
            return "is" + Capitalize(name);

        return pattern == "{noun}s" ? name + "s" : name;
    }

    private static string InferClusterName(IEnumerable<IdentifierSimilarity> similar, Identifier id)
    {
        var names = similar.Select(s => s.Other.Name).ToList();
        if(names.Count == 0)
            return id.Name;

        // Try to infer a common prefix
        var prefix = LongestCommonPrefix(names);
        if(prefix.Length > 1)
            return prefix + Capitalize(id.Name);

        // Try to infer a common suffix
        var suffix = LongestCommonSuffix(names);
        return suffix.Length > 1 ? id.Name + suffix : id.Name;
    }

    private static string LongestCommonPrefix(List<string> names)
    {
        if(names.Count == 0)
            return "";
        var prefix = names[0];

        foreach(var name in names)
        {
            while(!name.StartsWith(prefix))
            {
                prefix = prefix[..^1];
                if(prefix.Length == 0)
                    return "";
            }
        }

        return prefix;
    }

    private static string LongestCommonSuffix(List<string> names)
    {
        if(names.Count == 0)
            return "";
        var suffix = names[0];

        foreach(var name in names)
        {
            while(!name.EndsWith(suffix))
            {
                suffix = suffix[1..];
                if(suffix.Length == 0)
                    return "";
            }
        }

        return suffix;
    }
}
