namespace AStar.Dev.IdScan.Core;

public class Identifier
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public IdentifierCategory Category { get; set; }
    public string File { get; set; } = string.Empty;
    public int Line { get; set; }

    // Symbol-based IQ
    public string SymbolKind { get; set; } = string.Empty;
    public bool IsStatic { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsConst { get; set; }
    public bool IsImplicitlyTyped { get; set; }
    public bool IsCompilerGenerated { get; set; }
    public string? ContainingType { get; set; }
    public string? ContainingNamespace { get; set; }
    public string? NullableAnnotation { get; set; }

    public string? DeclaringMethod { get; set; }
    public string? DeclaringMethodReturnType { get; set; }
    public bool DeclaringMethodIsAsync { get; set; }
    public bool DeclaringMethodIsIterator { get; set; }

    public string? DeclaringType { get; set; }
    public string? DeclaringNamespace { get; set; }
    public string? DeclaringBaseType { get; set; }
    public List<string> DeclaringInterfaces { get; set; } = [];
    public bool DeclaringTypeIsRecord { get; set; }
    public bool DeclaringTypeIsStatic { get; set; }
    public bool DeclaringTypeIsAbstract { get; set; }
    public bool DeclaringTypeIsSealed { get; set; }
    public bool DeclaringTypeIsPartial { get; set; }
    public List<IdentifierUsage> Usages { get; set; } = [];
    public IdentifierUsage? FirstUsage { get; set; }
    public IdentifierUsage? LastUsage { get; set; }

    public IdentifierUsage? FirstWrite { get; set; }
    public IdentifierUsage? LastWrite { get; set; }

    public bool EscapesMethod { get; set; }
    public bool IsReturned { get; set; }
    public bool IsPassedAsArgument { get; set; }
    public bool IsCapturedByLambda { get; set; }
    public bool IsStoredInField { get; set; }
    public bool IsUsedInCondition { get; set; }
    public bool IsUsedInLoop { get; set; }
    public bool IsDisposed { get; set; }
}
