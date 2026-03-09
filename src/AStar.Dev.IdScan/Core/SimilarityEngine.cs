namespace AStar.Dev.IdScan.Core;

public static class SimilarityEngine
{
    public static List<IdentifierSimilarity> FindSimilar(Identifier target, IEnumerable<Identifier> all)
    {
        var results = new List<IdentifierSimilarity>();

        foreach(Identifier other in all.Where(other => other != target))
        {
            double score = 0;

            // 1. Same type
            if(other.Type == target.Type)
                score += 0.4;

            // 2. Same lifecycle patterns
            if(other.LastWrite != null == (target.LastWrite != null))
                score += 0.2;

            if(other.IsUsedInCondition == target.IsUsedInCondition)
                score += 0.1;

            if(other.IsUsedInLoop == target.IsUsedInLoop)
                score += 0.1;

            if(other.EscapesMethod == target.EscapesMethod)
                score += 0.1;

            // 3. Same context
            if(other.DeclaringNamespace == target.DeclaringNamespace)
                score += 0.1;

            if(score > 0)
                results.Add(new IdentifierSimilarity { Other = other, Score = score });
        }

        return
        [
            .. results
                .OrderByDescending(r => r.Score)
                .Take(5)
        ];
    }
}
