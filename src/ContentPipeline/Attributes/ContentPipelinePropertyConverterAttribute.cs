namespace ContentPipeline.Attributes;

using Microsoft.CodeAnalysis;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ContentPipelinePropertyConverterAttribute<TConverter, TReturn> : Attribute
{
    public ContentPipelinePropertyConverterAttribute(NullableAnnotation nullable = NullableAnnotation.Annotated)
    {
        ConverterType = typeof(TConverter);
        ReturnType = typeof(TReturn);
        NullableAnnotation = nullable;
    }

    public Type ConverterType { get; }

    public Type ReturnType { get; }

    public NullableAnnotation NullableAnnotation { get; }
}