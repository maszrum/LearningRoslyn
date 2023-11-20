using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynApi.ConsoleApp;

internal static class SyntaxTransformationFactoryMethods
{
    public static void Run()
    {
        NameSyntax name = SyntaxFactory.IdentifierName("System");
        name = SyntaxFactory.QualifiedName(name, SyntaxFactory.IdentifierName("Collections"));
        name = SyntaxFactory.QualifiedName(name, SyntaxFactory.IdentifierName("Generic"));

        const string sampleCode =
            """
            using System;
            using System.Collections;
            using System.Linq;
            using System.Text;

            namespace HelloWorld
            {
                class Program
                {
                    static void Main(string[] args)
                    {
                        Console.WriteLine(""Hello, World!"");
                    }
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(sampleCode);
        var root = tree.GetCompilationUnitRoot();

        var usingToReplace = root.Usings
            .Single(u => u.Name?.ToString() == "System.Collections");

        var newUsing = usingToReplace.WithName(name);

        root = root.ReplaceNode(usingToReplace, newUsing);

        Console.WriteLine(root.ToString());
    }
}
