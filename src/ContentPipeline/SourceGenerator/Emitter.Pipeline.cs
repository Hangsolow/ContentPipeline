using ContentPipeline.CodeBuilders;

namespace ContentPipeline.SourceGenerator;

internal sealed partial class Emitter
{
    internal string GetPipeline(ContentClass contentClass)
    {
        CancellationToken.ThrowIfCancellationRequested();
        var contentPipelineModelName = $"{SharedNamespace}.Models.{contentClass.Group}.{contentClass.Name}PipelineModel";
        var converters = GetConverters(contentClass.ContentProperties).Distinct().ToArray();

        return CSharpCodeBuilder.Create()
            .Line("#nullable enable")
            .Using("System")
            .Using("System.Collections.Generic")
            .Using($"{SharedNamespace}.Interfaces")
            .Using("EPiServer.Core")
            .Namespace($"{SharedNamespace}.Pipelines.{contentClass.Group}.Steps")
            .Class(
                $"internal class {contentClass.Name}PipelineStep : IContentPipelineStep<{contentClass.FullyQualifiedName}, {contentPipelineModelName}>")
            .Tab()
            .NewLine()
            .Line(
                $"public {contentClass.Name}PipelineStep({string.Join(",", converters.Select(p => $"{p.Type} {p.ShortName}"))})")
            .CodeBlock(b => b
                .Tab()
                .Foreach(converters, (b, p) => b.Line($"this.{p.ShortName} = {p.ShortName};")))
            .Foreach(converters, (b, p) => b.Property(p.ShortName, p.Type, isPublic: false))
            .Line("public int Order => 1000;")
            .NewLine()
            .Method(
                $"public void Execute({contentClass.FullyQualifiedName} content, {contentPipelineModelName} contentPipelineModel, IContentPipelineContext pipelineContext)",
                methodBuilder => methodBuilder
                    .Tab()
                    .Foreach(contentClass.ContentProperties,
                        (b, p) => b.Line($"contentPipelineModel.{p.Name} = {GetConverter(p)};"))
                    .Line(""))
            .Build();

        static IEnumerable<(string Type, string ShortName)> GetConverters(
            IEnumerable<ContentProperty> contentProperties)
        {
            foreach (var property in contentProperties.Where(p =>
                         !p.ConverterType.Equals("none", StringComparison.OrdinalIgnoreCase)))
            {
                var type = property.ConverterType;
                var shortName = GetShortName(type);

                yield return (type, shortName);
            }
        }

        static string GetShortName(string type)
        {
            var shortName = type;
            if (shortName.Contains('<'))
            {
                shortName = type
                                .Replace("<", "")
                                .Replace(">", "");
            }
            var index = shortName.LastIndexOf('.');

            if (index > -1)
            {
                shortName = shortName.Substring(index + 1);
            }

            return shortName;
        }

        string GetConverter(ContentProperty property)
        {
            if (property.ConverterType.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                return $"content.{property.Name}";
            }

            return $"{GetShortName(property.ConverterType)}.GetValue(content.{property.Name}, content, nameof(content.{property.Name}), pipelineContext)";
        }
    }
}