using Microsoft.CodeAnalysis.CSharp.Testing.NUnit;
using NUnit.Framework;

namespace MakeConst.CSharp.Tests;

[TestFixture]
public class MakeConstAnalyzerTests : AnalyzerVerifier<MakeConstAnalyzer>
{
    [Test]
    public Task LocalIntCouldBeConstant_Diagnostic()
    {
        const string source =
            """
            using System;

            class Program
            {
                static void Main()
                {
                    [|int i = 0;|]
                    Console.WriteLine(i);
                }
            }
            """;

        return VerifyAnalyzerAsync(source);
    }
}
