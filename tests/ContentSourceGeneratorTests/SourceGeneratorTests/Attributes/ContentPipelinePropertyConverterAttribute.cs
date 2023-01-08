using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
internal sealed class ContentPipelinePropertyConverterAttribute<TConverter, TReturn> : Attribute
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