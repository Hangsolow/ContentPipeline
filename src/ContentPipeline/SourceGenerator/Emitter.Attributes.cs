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
                    /// <param name="order"></param>
                    /// <exception cref="ArgumentException">if group is null or empty</exception>
                    public ContentPipelineModelAttribute(string group = "Common", int order = 0)
                    {
                        if (string.IsNullOrEmpty(group))
                        {
                            throw new ArgumentException($"'{nameof(group)}' cannot be null or empty.", nameof(group));
                        }
                        
                        if (char.IsLower(group[0]))
                        {
                            group = string.Concat(group[0].ToString().ToUpper(), group[1..]);    
                        }
                        
                        Group = group;
                        Order = order;
                    }

                    public string Group { get; }
                    
                    public int Order { get; }
                }
                """;

        string CreateContentPipelinePropertyConverterAttribute() =>
            $$"""
                namespace {{SharedNamespace}}.Attributes;

                using {{SharedNamespace}}.Interfaces;

                [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
                public sealed class ContentPipelinePropertyConverterAttribute<TConverter> : Attribute, IContentPipelinePropertyConverterAttribute<TConverter> 
                    where TConverter : IContentPropertyConverter
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
