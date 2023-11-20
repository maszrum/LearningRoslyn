using Another.Namespace;
using LearningRoslyn.SourceGenerator.ClientApp;

var sampleClassInstance = new SampleClass
{
    IntProperty1 = 1,
    IntProperty2 = 2,
    IntProperty3 = 3,
    IntProperty4 = 4,
    IntProperty5 = 5,
    IntProperty6 = 6,
    IntProperty7 = 7,
    IntProperty8 = 8,
    IntProperty9 = 9,
    IntProperty10 = 10,
    InnerObject1 = new ClassInAnotherNamespace("Hello"),
    InnerObject2 = new ClassInAnotherNamespace("source"),
    InnerObject3 = new ClassInAnotherNamespace("generators!")
};

Console.WriteLine(sampleClassInstance);
