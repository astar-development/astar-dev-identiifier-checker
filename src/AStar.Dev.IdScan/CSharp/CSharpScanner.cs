using AStar.Dev.IdScan.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AStar.Dev.IdScan.CSharp;

public class CSharpScanner
{
    public List<Identifier> Scan(string rootPath)
    {
        var identifiers = new List<Identifier>();

        var files = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories);

        var syntaxTrees = files
            .Select(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f), path: f))
            .ToList();

        // Load references
        var references = new List<MetadataReference>();
        references.AddRange(ReferenceLoader.LoadFrameworkReferences());
        references.AddRange(ReferenceLoader.LoadLocalDlls(rootPath));

        var compilation = CSharpCompilation.Create(
            "IdScan",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        foreach(SyntaxTree tree in syntaxTrees)
        {
            SemanticModel model = compilation.GetSemanticModel(tree, true);
            var walker = new IdentifierWalker(model, identifiers);
            walker.Visit(tree.GetRoot());
        }

        return identifiers;
    }

    private class IdentifierWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel _model;
        private readonly List<Identifier> _output;
        private IMethodSymbol? _currentMethod;
        private INamedTypeSymbol? _currentType;

        public IdentifierWalker(SemanticModel model, List<Identifier> output)
        {
            _model = model;
            _output = output;
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            INamedTypeSymbol? symbol = _model.GetDeclaredSymbol(node);
            _currentType = symbol;

            base.VisitClassDeclaration(node);

            _currentType = null;
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            INamedTypeSymbol? symbol = _model.GetDeclaredSymbol(node);
            _currentType = symbol;

            base.VisitStructDeclaration(node);

            _currentType = null;
        }

        public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
        {
            INamedTypeSymbol? symbol = _model.GetDeclaredSymbol(node);
            _currentType = symbol;

            base.VisitRecordDeclaration(node);

            _currentType = null;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            IMethodSymbol? symbol = _model.GetDeclaredSymbol(node);
            _currentMethod = symbol;

            base.VisitMethodDeclaration(node);

            _currentMethod = null;
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            // Track constructor as current method context
            IMethodSymbol? symbol = _model.GetDeclaredSymbol(node);
            _currentMethod = symbol;

            // Capture constructor parameters as identifiers
            foreach(ParameterSyntax p in node.ParameterList.Parameters)
                Add(p.Identifier.Text, p, IdentifierCategory.ConstructorParameter);

            base.VisitConstructorDeclaration(node);

            // Clear method context
            _currentMethod = null;
        }

        public override void VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            IMethodSymbol? symbol = _model.GetDeclaredSymbol(node);
            _currentMethod = symbol;

            base.VisitOperatorDeclaration(node);

            _currentMethod = null;
        }

        public override void VisitParameter(ParameterSyntax node)
        {
            Add(node.Identifier.Text, node, IdentifierCategory.Parameter);
            base.VisitParameter(node);
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            Add(node.Identifier.Text, node, IdentifierCategory.LocalVariable);
            base.VisitVariableDeclarator(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            Add(node.Identifier.Text, node, IdentifierCategory.Property);
            base.VisitPropertyDeclaration(node);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            foreach(VariableDeclaratorSyntax v in node.Declaration.Variables)
                Add(v.Identifier.Text, v, IdentifierCategory.Field);

            base.VisitFieldDeclaration(node);
        }

        public override void VisitDeclarationExpression(DeclarationExpressionSyntax node)
        {
            if(node.Designation is ParenthesizedVariableDesignationSyntax tuple)
            {
                foreach(VariableDesignationSyntax variable in tuple.Variables)
                {
                    if(variable is SingleVariableDesignationSyntax single)
                        Add(single.Identifier.Text, single, IdentifierCategory.TupleElement);
                }
            }

            base.VisitDeclarationExpression(node);
        }

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            Add(node.Parameter.Identifier.Text, node.Parameter, IdentifierCategory.Parameter);
            base.VisitSimpleLambdaExpression(node);
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            foreach(ParameterSyntax p in node.ParameterList.Parameters)
                Add(p.Identifier.Text, p, IdentifierCategory.Parameter);

            base.VisitParenthesizedLambdaExpression(node);
        }

        public override void VisitDeclarationPattern(DeclarationPatternSyntax node)
        {
            Add(node.Designation.ToString(), node, IdentifierCategory.LocalVariable);
            base.VisitDeclarationPattern(node);
        }

        public override void VisitSwitchExpressionArm(SwitchExpressionArmSyntax node)
        {
            if(node.Pattern is DeclarationPatternSyntax dp &&
               dp.Designation is SingleVariableDesignationSyntax sv)
            {
                Add(sv.Identifier.Text, sv, IdentifierCategory.LocalVariable);
            }

            base.VisitSwitchExpressionArm(node);
        }

        private void Add(string name, SyntaxNode node, IdentifierCategory category)
        {
            ISymbol? symbol = _model.GetDeclaredSymbol(node);
            TypeInfo typeInfo = _model.GetTypeInfo(node);
            ITypeSymbol? typeSymbol = typeInfo.Type ?? typeInfo.ConvertedType;

            var typeName = typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                           ?? "unknown";

            FileLinePositionSpan location = node.GetLocation().GetLineSpan();
            var file = location.Path;
            var line = location.StartLinePosition.Line + 1;

            var id = new Identifier
            {
                Name = name,
                Type = typeName,
                Category = category,
                File = file,
                Line = line,

                // Symbol IQ
                SymbolKind = symbol?.Kind.ToString() ?? "Unknown",
                IsStatic = symbol?.IsStatic ?? false,
                IsReadOnly = symbol is IFieldSymbol fs && fs.IsReadOnly,
                IsConst = symbol is IFieldSymbol cs && cs.IsConst,
                IsImplicitlyTyped = typeSymbol is IErrorTypeSymbol,
                IsCompilerGenerated = symbol?.IsImplicitlyDeclared ?? false,
                ContainingType = symbol?.ContainingType?.ToDisplayString(),
                ContainingNamespace = symbol?.ContainingNamespace?.ToDisplayString(),
                NullableAnnotation = typeInfo.Nullability.Annotation.ToString(),

                // Method context
                DeclaringMethod = _currentMethod?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                DeclaringMethodReturnType = _currentMethod?.ReturnType.ToDisplayString(),
                DeclaringMethodIsAsync = _currentMethod?.IsAsync ?? false,
                DeclaringMethodIsIterator = _currentMethod?.IsIterator ?? false,

                // Type context
                DeclaringType = _currentType?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                DeclaringNamespace = _currentType?.ContainingNamespace?.ToDisplayString(),
                DeclaringBaseType = _currentType?.BaseType?.ToDisplayString(),
                DeclaringInterfaces =
                    _currentType?.Interfaces.Select(i => i.ToDisplayString()).ToList() ?? new List<string>(),
                DeclaringTypeIsRecord = _currentType?.IsRecord ?? false,
                DeclaringTypeIsStatic = _currentType?.IsStatic ?? false,
                DeclaringTypeIsAbstract = _currentType?.IsAbstract ?? false,
                DeclaringTypeIsSealed = _currentType?.IsSealed ?? false,
                DeclaringTypeIsPartial = _currentType?.DeclaringSyntaxReferences
                                             .Select(r => r.GetSyntax())
                                             .OfType<TypeDeclarationSyntax>()
                                             .Any(t => t.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                                         ?? false
            };

            _output.Add(id);
        }
    }

    private class UsageWalker : CSharpSyntaxWalker
    {
        private readonly List<Identifier> _identifiers;
        private readonly SemanticModel _model;
        private IMethodSymbol? _currentMethod;
        private INamedTypeSymbol? _currentType;

        public UsageWalker(SemanticModel model, List<Identifier> identifiers)
        {
            _model = model;
            _identifiers = identifiers;
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            _currentType = _model.GetDeclaredSymbol(node);
            base.VisitClassDeclaration(node);
            _currentType = null;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            _currentMethod = _model.GetDeclaredSymbol(node);
            base.VisitMethodDeclaration(node);
            _currentMethod = null;
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            ISymbol? symbol = _model.GetSymbolInfo(node).Symbol;

            if(symbol != null)
            {
                Identifier? match = _identifiers.FirstOrDefault(id =>
                    id.Name == symbol.Name &&
                    id.ContainingType == symbol.ContainingType?.ToDisplayString());

                if(match != null)
                {
                    FileLinePositionSpan location = node.GetLocation().GetLineSpan();
                    var file = location.Path;
                    var line = location.StartLinePosition.Line + 1;

                    var usage = new IdentifierUsage
                    {
                        File = file,
                        Line = line,
                        ContainingType = _currentType?.ToDisplayString(),
                        ContainingMethod = _currentMethod?.ToDisplayString(),
                        UsageKind = InferUsageKind(node)
                    };

                    match.Usages.Add(usage);

                    UpdateLifecycle(match, usage);
                }
            }

            base.VisitIdentifierName(node);
        }

        private void UpdateLifecycle(Identifier id, IdentifierUsage usage)
        {
            // First usage
            id.FirstUsage ??= usage;

            // Last usage
            id.LastUsage = usage;

            // Writes
            if(usage.UsageKind == "Write")
            {
                id.FirstWrite ??= usage;

                id.LastWrite = usage;
            }

            // Escapes method (passed as argument)
            if(usage.UsageKind == "Argument")
                id.IsPassedAsArgument = true;

            // Returned
            if(usage.ContainingMethod != null &&
               usage.ContainingMethod.Contains("return"))
            {
                id.IsReturned = true;
            }

            // Captured by lambda
            if(usage.ContainingMethod != null &&
               usage.ContainingMethod.Contains("=>"))
            {
                id.IsCapturedByLambda = true;
            }

            // Stored in field
            if(usage.UsageKind == "Write" &&
               usage.ContainingType != id.DeclaringType)
            {
                id.IsStoredInField = true;
            }

            // Used in condition
            if(usage.UsageKind == "Read" &&
               usage.ContainingMethod?.Contains("if") == true)
            {
                id.IsUsedInCondition = true;
            }

            // Used in loop
            if(usage.ContainingMethod?.Contains("for") == true ||
               usage.ContainingMethod?.Contains("foreach") == true ||
               usage.ContainingMethod?.Contains("while") == true)
            {
                id.IsUsedInLoop = true;
            }

            // Disposed
            if(usage.UsageKind == "Invoke" &&
               usage.ContainingMethod?.Contains("Dispose") == true)
            {
                id.IsDisposed = true;
            }
        }

        private string InferUsageKind(IdentifierNameSyntax node)
        {
            SyntaxNode? parent = node.Parent;

            return parent switch
            {
                AssignmentExpressionSyntax a when a.Left == node => "Write",
                AssignmentExpressionSyntax a when a.Right == node => "Read",
                InvocationExpressionSyntax => "Invoke",
                MemberAccessExpressionSyntax => "Access",
                ArgumentSyntax => "Argument",
                _ => "Read"
            };
        }
    }
}
