using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LearningRoslyn.SourceGenerator.GenerateProperties;

[Generator]
internal class GeneratePropertiesGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var propertiesToGenerate = new PropertiesDataExtractor()
            .GetPropertiesToGenerate(context.Compilation)
            .GroupBy(
                keySelector: d => d.ClassSymbol,
                elementSelector: d => d,
                comparer: SymbolEqualityComparer.Default);

        foreach (var propertiesData in propertiesToGenerate)
        {
            var classSymbol = propertiesData.Key!;

            var generatedProperties = propertiesData
                .SelectMany(p =>
                    Enumerable
                        .Range(1, p.Count)
                        .Select(i => $"        public {p.Type} {p.Name}{i} {{ get; set; }}"));

            var generatedPropertiesJoined = string.Join("\n\n", generatedProperties);

            var classCode =
                $$"""
                  namespace {{classSymbol.ContainingNamespace}}
                  {
                      {{SyntaxFacts.GetText(classSymbol.DeclaredAccessibility)}} partial class {{classSymbol.Name}}
                      {
                  {{generatedPropertiesJoined}}
                      }
                  }
                  """;

            context.AddSource($"{classSymbol.Name}.g.cs", classCode);
        }
    }
}
