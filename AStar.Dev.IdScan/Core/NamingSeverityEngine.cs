namespace AStar.Dev.IdScan.Core;

public static class NamingSeverityEngine
{
    public static NamingSeverityResult Evaluate(
        Identifier id,
        IEnumerable<Identifier> allIdentifiers)
    {
        var result = new NamingSeverityResult { Identifier = id };

        double score = 0;

        // 1. Violations from naming rules
        NamingRuleResult ruleResult = NamingRulesEngine.Evaluate(id);
        foreach(var v in ruleResult.Violations)
        {
            result.Reasons.Add(v);
            score += 0.15; // each violation adds weight
        }

        // 2. Similarity-based mismatch
        List<IdentifierSimilarity> similar = SimilarityEngine.FindSimilar(id, allIdentifiers);
        var pattern = NamingPatternEngine.InferPattern(similar);

        if(pattern != null && !MatchesPattern(id.Name, pattern))
        {
            result.Reasons.Add($"Name does not match dominant pattern `{pattern}`.");
            score += 0.25;
        }

        // 3. Lifecycle importance
        if(id.EscapesMethod)
            score += 0.2;

        if(id.Usages.Count > 10)
            score += 0.2;

        if(id.IsUsedInCondition && !id.Name.StartsWith("is"))
            score += 0.1;

        if(id.IsUsedInLoop && !id.Name.EndsWith("s"))
            score += 0.1;

        // 4. Misleading names
        if(id.Name.Length <= 2 && id.EscapesMethod)
        {
            result.Reasons.Add("Name is too short for an escaping identifier.");
            score += 0.3;
        }

        // 5. Cap score at 1.0
        result.Severity = Math.Min(score, 1.0);

        return result;
    }

    private static bool MatchesPattern(string name, string pattern)
    {
        if(pattern == "{prefix}Id")
            return name.EndsWith("Id");

        if(pattern == "is{Noun}")
            return name.StartsWith("is") || name.StartsWith("has");

        if(pattern == "{noun}s")
            return name.EndsWith("s");

        return false;
    }
}
