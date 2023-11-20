using Another.Namespace;
using LearningRoslyn.SourceGenerator.GenerateProperties;

namespace LearningRoslyn.SourceGenerator.ClientApp;

[GenerateProperties<int>("IntProperty", 10)]
[GenerateProperties<ClassInAnotherNamespace>("InnerObject", 3)]
public partial class SampleClass;
