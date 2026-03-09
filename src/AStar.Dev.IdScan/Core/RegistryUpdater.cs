namespace AStar.Dev.IdScan.Core;

public class RegistryUpdater
{
    public void UpdateRegistry(IdentifierRegistry registry, List<Identifier> scanned)
    {
        List<IdentifierRegistryEntry> existing = registry.Identifiers;

        // Remove missing
        existing.RemoveAll(e =>
            !scanned.Any(s => s.Name == e.Name && s.File == e.File && s.Line == e.Line));

        // Add or update
        foreach(Identifier s in scanned)
        {
            IdentifierRegistryEntry? match = existing.FirstOrDefault(e =>
                e.Name == s.Name &&
                e.File == s.File &&
                e.Line == s.Line);

            if(match == null)
            {
                registry.Identifiers.Add(new IdentifierRegistryEntry
                {
                    Name = s.Name,
                    Type = s.Type,
                    Category = s.Category,
                    File = s.File,
                    Line = s.Line,
                    FirstDetected = DateTime.UtcNow,
                    Status = "To Check"
                });
            }
            else
            {
                match.Type = s.Type;
                match.Category = s.Category;
                match.File = s.File;
                match.Line = s.Line;
            }
        }
    }
}
