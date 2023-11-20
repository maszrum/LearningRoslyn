using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynApi.ConsoleApp;

internal static class SyntaxWalkers
{
    public static void Run()
    {
        const string programText =
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;

            namespace TopLevel
            {
                using Microsoft;
                using System.ComponentModel;
            
                namespace Child1
                {
                    using Microsoft.Win32;
                    using System.Runtime.InteropServices;
            
                    class Foo { }
                }
            
                namespace Child2
                {
                    using System.CodeDom;
                    using Microsoft.CSharp;
            
                    class Bar { }
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(programText);
        var root = tree.GetCompilationUnitRoot();

        var collector = new UsingCollector();
        collector.Visit(root);

        Console.WriteLine("List of found using namespace directives:");

        foreach (var directive in collector.Usings)
        {
            Console.WriteLine(directive.Name);
        }
    }
}

internal class UsingCollector : CSharpSyntaxWalker
{
    public ICollection<UsingDirectiveSyntax> Usings { get; } = new List<UsingDirectiveSyntax>();

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        var nodeName = node.Name!.ToString();
        if (nodeName != "System" && !nodeName.StartsWith("System."))
        {
            Usings.Add(node);
        }
    }
}
