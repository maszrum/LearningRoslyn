# Learning Roslyn API

My experiments with Roslyn API that I created when learning.

## Source generator

Add ``GenerateProperties<T>(string propertyName, int count)`` attribute to generate properties with specified type and name.

For example:

```csharp
[GenerateProperties<int>("IntProperty", 3)]
public partial class SampleClass;
```

will generate:

```csharp
public partial class SampleClass
{
    public int IntProperty1 { get; set; }

    public int IntProperty2 { get; set; }

    public int IntProperty3 { get; set; }
}
```

I wrote this source generator for educational purposes.

## Analyzer

TODO

## Useful links:

- Microsoft documentation about Roslyn API https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/
- Roslyn Quoter https://roslynquoter.azurewebsites.net/
- How to debug source generators in Rider https://blog.jetbrains.com/dotnet/2023/07/13/debug-source-generators-in-jetbrains-rider/
- Source generators cookbook https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md
