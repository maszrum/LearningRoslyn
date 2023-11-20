using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynApi.ConsoleApp;

internal static class SemanticAnalysis
{
    public static void Run()
    {
        const string programText =
            """
            using System;
            using System.Collections.Generic;
            using System.Text;

            namespace HelloWorld
            {
                class Program
                {
                    static void Main(string[] args)
                    {
                        Console.WriteLine("Hello, World!");
                    }
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(programText);
        var root = tree.GetCompilationUnitRoot();

        var compilation = CSharpCompilation.Create("HelloWorld")
            .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
            .AddSyntaxTrees(tree);

        var semanticModel = compilation.GetSemanticModel(tree);

        var usingSystem = root.Usings[0];
        var systemName = usingSystem.Name!;

        var nameSymbolInfo = semanticModel.GetSymbolInfo(systemName);

        var systemSymbol = (INamespaceSymbol?) nameSymbolInfo.Symbol;

        Console.WriteLine("All System.* namespaces that are in compilation model:");

        if (systemSymbol?.GetNamespaceMembers() is not null)
        {
            foreach (var ns in systemSymbol.GetNamespaceMembers())
            {
                Console.WriteLine(ns);
            }
        }

        var helloWorldString = root
            .DescendantNodes()
            .OfType<LiteralExpressionSyntax>()
            .First();

        var literalInfo = semanticModel.GetTypeInfo(helloWorldString);
        var stringTypeSymbol = (INamedTypeSymbol?) literalInfo.Type;
        var allMembers = stringTypeSymbol?.GetMembers();
        var methods = allMembers?.OfType<IMethodSymbol>();

        var publicStringReturningMethods = methods?
            .Where(m => SymbolEqualityComparer.Default.Equals(m.ReturnType, stringTypeSymbol) &&
                        m.DeclaredAccessibility == Accessibility.Public);

        var distinctMethods = publicStringReturningMethods?
            .Select(m => m.Name)
            .Distinct()!;

        Console.WriteLine("All public methods that return string type in string type:");

        foreach (var methodName in distinctMethods)
        {
            Console.WriteLine(methodName);
        }
    }
}
