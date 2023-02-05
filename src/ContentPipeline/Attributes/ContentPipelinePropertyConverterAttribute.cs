namespace ContentPipeline.Attributes;

using Microsoft.CodeAnalysis;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ContentPipelinePropertyConverterAttribute<TConverter> : Attribute
{
    public ContentPipelinePropertyConverterAttribute(NullableAnnotation nullable = NullableAnnotation.Annotated)
    {
        ConverterType = typeof(TConverter);
        NullableAnnotation = nullable;
    }

    public Type ConverterType { get; }

    public NullableAnnotation NullableAnnotation { get; }
}