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

    private class IdentifierWalker(SemanticModel model, List<Identifier> output) : CSharpSyntaxWalker
    {
        private IMethodSymbol? _currentMethod;
        private INamedTypeSymbol? _currentType;

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            INamedTypeSymbol? symbol = model.GetDeclaredSymbol(node);
            _currentType = symbol;

            base.VisitClassDeclaration(node);

            _currentType = null;
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            INamedTypeSymbol? symbol = model.GetDeclaredSymbol(node);
            _currentType = symbol;

            base.VisitStructDeclaration(node);

            _currentType = null;
        }

        public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
        {
            INamedTypeSymbol? symbol = model.GetDeclaredSymbol(node);
            _currentType = symbol;

            base.VisitRecordDeclaration(node);

            _currentType = null;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            IMethodSymbol? symbol = model.GetDeclaredSymbol(node);
            _currentMethod = symbol;

            base.VisitMethodDeclaration(node);

            _currentMethod = null;
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            // Track constructor as current method context
            IMethodSymbol? symbol = model.GetDeclaredSymbol(node);
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
            IMethodSymbol? symbol = model.GetDeclaredSymbol(node);
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
                foreach(SingleVariableDesignationSyntax single in tuple.Variables.OfType<SingleVariableDesignationSyntax>())
                {
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
            if(node.Pattern is DeclarationPatternSyntax { Designation: SingleVariableDesignationSyntax sv })
                Add(sv.Identifier.Text, sv, IdentifierCategory.LocalVariable);

            base.VisitSwitchExpressionArm(node);
        }

        private void Add(string name, SyntaxNode node, IdentifierCategory category)
        {
            ISymbol? symbol = model.GetDeclaredSymbol(node);
            TypeInfo typeInfo = model.GetTypeInfo(node);
            ITypeSymbol? typeSymbol = typeInfo.Type ?? typeInfo.ConvertedType;

            var typeName = typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "unknown";

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
                IsReadOnly = symbol is IFieldSymbol { IsReadOnly: true },
                IsConst = symbol is IFieldSymbol { IsConst: true },
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

            output.Add(id);
        }
    }
}
