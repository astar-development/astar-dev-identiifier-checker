namespace AStar.Dev.IdScan.Core;

public static class NamingSuggestionEngine
{
    public static string Suggest(Identifier id)
    {
        // 1. If escaping, suggest descriptive names
        if(id.EscapesMethod)
            return SuggestDescriptive(id);

        // 2. If boolean-like
        if(id.IsUsedInCondition)
            return SuggestBoolean(id);

        // 3. If plural-like
        if(id.IsUsedInLoop)
            return SuggestPlural(id);

        // 4. If resource-like
        if(id.IsDisposed)
            return SuggestResource(id);

        // 5. Mutable → verb-like
        if(id.LastWrite != null)
            return SuggestVerb(id);

        // 6. Immutable → noun-like
        return SuggestNoun(id);
    }

    private static string SuggestDescriptive(Identifier id)
    {
        if(id.Type.EndsWith("String"))
            return "value";

        if(id.Type.EndsWith("Int32") || id.Type.EndsWith("Int64"))
            return "count";

        if(id.Type.Contains("User"))
            return "user";

        if(id.Type.Contains("Request"))
            return "request";

        // Tuple element heuristic
        if(id.Category == IdentifierCategory.TupleElement)
        {
            // If it's short, expand it
            if(id.Name.Length <= 2)
                return "item" + Capitalize(id.Name);

            // Otherwise keep it noun-like
            return id.Name;
        }

        return $"{id.Type.Split('.').Last().ToLower()}";
    }

    private static string SuggestBoolean(Identifier id) => "is" + Capitalize(id.Name);

    private static string SuggestPlural(Identifier id)
    {
        if(id.Name.EndsWith("s"))
            return id.Name;

        return id.Name + "s";
    }

    private static string SuggestResource(Identifier id) => id.Name + "Resource";

    private static string SuggestVerb(Identifier id) => "update" + Capitalize(id.Name);

    private static string SuggestNoun(Identifier id) => id.Name;

    private static string Capitalize(string s)
    {
        if(string.IsNullOrEmpty(s)) return s;
        return char.ToUpper(s[0]) + s.Substring(1);
    }
}
