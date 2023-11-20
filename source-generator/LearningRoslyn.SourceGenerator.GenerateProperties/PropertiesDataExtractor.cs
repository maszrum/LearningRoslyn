using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LearningRoslyn.SourceGenerator.GenerateProperties;

internal class PropertiesDataExtractor
{
    private static readonly string GeneratePropertiesAttributeName = typeof(GeneratePropertiesAttribute<>).FullName!;

    public IEnumerable<PropertyGenerationData> GetPropertiesToGenerate(Compilation compilation)
    {
        var attributeSymbol = compilation.GetTypeByMetadataName(GeneratePropertiesAttributeName)!;

        var syntaxTrees = compilation.SyntaxTrees;

        foreach (var syntaxTree in syntaxTrees)
        {
            var root = syntaxTree.GetRoot();

            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var classDeclarations = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>();

            foreach (var classDeclaration in classDeclarations)
            {
                var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

                if (classSymbol is null)
                {
                    continue;
                }

                var propertiesData =
                    GetAttributesData(classSymbol, attributeSymbol)
                        .Select(ExtractPropertyDataFromAttribute);

                foreach (var data in propertiesData)
                {
                    yield return data;
                }
            }
        }
    }

    private static IEnumerable<(AttributeData, INamedTypeSymbol)> GetAttributesData(
        INamedTypeSymbol classSymbol,
        ISymbol attributeSymbol)
    {
        return classSymbol.GetAttributes()
            .Where(a =>
                a.AttributeClass?.ContainingNamespace.ToString() == attributeSymbol.ContainingNamespace.ToString() &&
                a.AttributeClass.MetadataName == attributeSymbol.MetadataName)
            .Select(a => (a, symbol: classSymbol));
    }

    private static PropertyGenerationData ExtractPropertyDataFromAttribute(
        (AttributeData, INamedTypeSymbol) attributeDataAndClassSymbol)
    {
        var (attributeData, classSymbol) = attributeDataAndClassSymbol;

        var propertyType = attributeData.AttributeClass!.TypeArguments[0].ToString();

        var propertyName = (string) attributeData.ConstructorArguments[0].Value!;
        var count = (int) attributeData.ConstructorArguments[1].Value!;

        return new PropertyGenerationData(propertyType, propertyName, count, classSymbol);
    }
}
