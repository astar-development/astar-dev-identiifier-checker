using System.Text.RegularExpressions;

namespace AStar.Dev.IdScan.Core;

public static class NamingRulesEngine
{
    public static NamingRuleResult Evaluate(Identifier id)
    {
        var result = new NamingRuleResult { Identifier = id };

        // 1. Mutable identifiers should be verb-like
        if(id.LastWrite != null && !LooksLikeVerb(id.Name))
            result.Violations.Add("Identifier is mutable but name is not verb-like.");

        // 2. Immutable identifiers should be noun-like
        if(id.LastWrite == null && LooksLikeVerb(id.Name))
            result.Violations.Add("Identifier is immutable but name looks like a verb.");

        // 3. Escaping identifiers should be descriptive
        if(id.EscapesMethod && id.Name.Length < 3)
            result.Violations.Add("Identifier escapes method but name is too short.");

        // 4. Condition identifiers should be boolean-like
        if(id.IsUsedInCondition && !LooksBooleanish(id.Name))
            result.Violations.Add("Identifier is used in conditions but name is not boolean-like.");

        // 5. Loop identifiers should be plural or collection-like
        if(id.IsUsedInLoop && !LooksPlural(id.Name))
            result.Violations.Add("Identifier is used in loops but name is not plural.");

        // 6. Disposed identifiers should be resource-like
        if(id.IsDisposed && !LooksResourceLike(id.Name))
            result.Violations.Add("Identifier is disposed but name does not look like a resource.");

        // 7. Tuple element naming rules
        if(id.Category != IdentifierCategory.TupleElement)
            return result;
        if(id.Name.Length <= 2)
            result.Violations.Add("Tuple element name is too short.");

        if(!char.IsLower(id.Name[0]))
            result.Violations.Add("Tuple element name should start with a lowercase letter.");

        return result;
    }

    private static bool LooksLikeVerb(string name)
        // crude but effective: verbs often start with "get", "set", "load", "build", etc.
        => Regex.IsMatch(name, @"^(get|set|load|build|create|update|fetch|calculate|compute|resolve)",
            RegexOptions.IgnoreCase);

    private static bool LooksBooleanish(string name)
        => Regex.IsMatch(name, @"^(is|has|should|can|allow|enable|disable)", RegexOptions.IgnoreCase);

    private static bool LooksPlural(string name) => name.EndsWith("s", StringComparison.OrdinalIgnoreCase);

    private static bool LooksResourceLike(string name) => Regex.IsMatch(name,
        @"(stream|connection|scope|reader|writer|client|context)$", RegexOptions.IgnoreCase);
}
