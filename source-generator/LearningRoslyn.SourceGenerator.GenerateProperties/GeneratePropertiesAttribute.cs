namespace LearningRoslyn.SourceGenerator.GenerateProperties;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class GeneratePropertiesAttribute<T> : Attribute
{
    public GeneratePropertiesAttribute(string propertyName, int count)
    {
    }
}
