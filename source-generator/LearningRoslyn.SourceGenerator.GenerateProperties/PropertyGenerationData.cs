using Microsoft.CodeAnalysis;

namespace LearningRoslyn.SourceGenerator.GenerateProperties;

internal readonly struct PropertyGenerationData(
    string type,
    string name,
    int count,
    INamedTypeSymbol classSymbol)
{
    public string Type { get; } = type;

    public string Name { get; } = name;

    public int Count { get; } = count;

    public INamedTypeSymbol ClassSymbol { get; } = classSymbol;
}
