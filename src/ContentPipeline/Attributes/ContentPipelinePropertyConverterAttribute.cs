namespace ContentPipeline.Attributes;

using ContentPipeline.Interfaces;
using Microsoft.CodeAnalysis;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ContentPipelinePropertyConverterAttribute<TConverter> : Attribute where TConverter : IContentPropertyConverter
{
    public ContentPipelinePropertyConverterAttribute()
    {
        ConverterType = typeof(TConverter);
    }

    public Type ConverterType { get; }
}