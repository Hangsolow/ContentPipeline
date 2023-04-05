using ContentPipeline.CodeBuilders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContentPipeline.SourceGenerator;

internal sealed partial class Emitter
{
    internal IEnumerable<CodeSource> GetAttributes()
    {
        yield return new("ContentPipelineIgnoreAttribute.g.cs", CreateContentPipelineIgnoreAttribute());
        yield return new("ContentPipelineModelAttribute.g.cs", CreateContentPipelineModelAttribute());
        yield return new("ContentPipelinePropertyConverterAttribute.g.cs", CreateContentPipelinePropertyConverterAttribute());

        string CreateContentPipelineIgnoreAttribute() =>
            $$"""
                namespace {{SharedNamespace}}.Attributes;

                [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
                public class ContentPipelineIgnoreAttribute : Attribute
                {

                }
                """;

        string CreateContentPipelineModelAttribute() =>
            $$"""
                namespace {{SharedNamespace}}.Attributes;

                /// <summary>
                /// Enables the content for use in the content pipeline
                /// </summary>
                [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
                public sealed class ContentPipelineModelAttribute : Attribute
                {
                    /// <summary>
                    /// the constructor for ContentPipelineModelAttribute
                    /// </summary>
                    /// <param name="group"></param>
                    /// <exception cref="ArgumentException">if group is null or empty</exception>
                    public ContentPipelineModelAttribute(string group = "Common")
                    {
                        if (string.IsNullOrEmpty(group))
                        {
                            throw new ArgumentException($"'{nameof(group)}' cannot be null or empty.", nameof(group));
                        }

                        Group = group;
                    }

                    public string Group { get; }
                }
                """;

        string CreateContentPipelinePropertyConverterAttribute() =>
            $$"""
                namespace {{SharedNamespace}}.Attributes;

                using {{SharedNamespace}}.Interfaces;

                [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
                public sealed class ContentPipelinePropertyConverterAttribute<TConverter> : Attribute where TConverter : IContentPropertyConverter
                {
                    public ContentPipelinePropertyConverterAttribute()
                    {
                        ConverterType = typeof(TConverter);
                    }

                    public Type ConverterType { get; }
                }
                """;
    }
}
