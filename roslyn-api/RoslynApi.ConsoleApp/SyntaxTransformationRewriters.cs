using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynApi.ConsoleApp;

// use var instead of explicit type when declaring variable with initialization

internal static class SyntaxTransformationRewriters
{
    public static void Run()
    {
        var compilation = CreateTestCompilation();

        foreach (var sourceTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(sourceTree);

            var rewriter = new TypeInterfaceRewriter(semanticModel);

            var newSource = rewriter.Visit(sourceTree.GetRoot());

            if (newSource != sourceTree.GetRoot())
            {
                Console.WriteLine(newSource.ToFullString());
            }
        }
    }

    private static Compilation CreateTestCompilation()
    {
        const string code =
            """
            namespace SyntaxRewriter
            {
                public class SomeClass
                {
                    public string GetSomeString()
                    {
                        string firstString = "Hello";
                        string secondString = "World!";
                        int number = 10;
                        
                        return $"{firstString}, {secondString} {number}";
                    }
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(code);

        var compilation = CSharpCompilation
            .Create("SyntaxRewriter")
            .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
            .AddSyntaxTrees(tree);

        return compilation;
    }
}

internal class TypeInterfaceRewriter(SemanticModel semanticModel) : CSharpSyntaxRewriter
{
    public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
    {
        if (node.Declaration.Variables.Count > 1)
        {
            return node;
        }

        if (node.Declaration.Variables[0].Initializer is null)
        {
            return node;
        }

        var declarator = node.Declaration.Variables[0];
        var variableTypeName = node.Declaration.Type;

        var variableType = (ITypeSymbol?) semanticModel
            .GetSymbolInfo(variableTypeName)
            .Symbol;

        var initializerInfo = semanticModel.GetTypeInfo(declarator.Initializer!.Value);

        if (!SymbolEqualityComparer.Default.Equals(variableType, initializerInfo.Type))
        {
            return node;
        }

        var varTypeName = SyntaxFactory.IdentifierName("var")
            .WithLeadingTrivia(variableTypeName.GetLeadingTrivia())
            .WithTrailingTrivia(variableTypeName.GetTrailingTrivia());

        return node.ReplaceNode(variableTypeName, varTypeName);
    }
}
