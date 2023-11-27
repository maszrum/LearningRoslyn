using Microsoft.CodeAnalysis;

namespace LearningRoslyn.IncrementalGenerator.SampleGenerator;

[Generator]
public class MyFirstIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        // get the additional text provider
        var additionalTexts = initContext.AdditionalTextsProvider;

        // apply a 1-to-1 transform on each text, extracting the path
        var transformed = additionalTexts.Select(static (text, _) => text.Path);

        // collect the paths into a batch
        var collected = transformed.Collect();

        // take the file paths from the above batch and make some user visible syntax
        initContext.RegisterSourceOutput(collected, static (sourceProductionContext, filePaths) =>
        {
            sourceProductionContext.AddSource(
                "AdditionalFiles.cs",
                $$"""
                namespace Generated
                {
                    public class AdditionalTextList
                    {
                        public static void PrintTexts()
                        {
                            System.Console.WriteLine(@"Additional Texts were: {{string.Join(", ", filePaths)}}");
                        }
                    }
                }
                """);
        });
    }
}
