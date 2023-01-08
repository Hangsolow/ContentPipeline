using ContentPipeline.CodeBuilders;

namespace ContentPipeline.SourceGenerator
{
    internal sealed partial class Emitter
    {
        internal string GetPipeline(ContentClass contentClass)
        {
            CancellationToken.ThrowIfCancellationRequested();
            var contentPipelineModelName = $"{SharedNamespace}.Models.{contentClass.Name}PipelineModel";
            return string.Empty;
            // return CSharpCodeBuilder.Create()
            //     .Line("#nullable enable")
            //     .Using("System")
            //     .Using("System.Collections.Generic")
            //     .Using($"{SharedNamespace}.Interfaces")
            //     .Using("EPiServer.Core")
            //     .Namespace($"{SharedNamespace}.Pipelines.Steps")
            //     .Class($"public class {contentClass.Name}PipelineStep : IContentConverterPipelineStep<{contentClass.FullyQualifiedName}, {contentPipelineModelName}>")
            //     .Tab()
            //     .NewLine()
            //     .Line($"public {contentClass.Name}PipelineStep({string.Join(",", converters.Select(p => $"{p.Type} {p.ShortName}"))})")
            //     .CodeBlock(b => b
            //         .Tab()
            //         .Foreach(converters, (b, p) => b.Line($"this.{p.ShortName} = {p.ShortName};")))
            //     .Foreach(converters, (b, p) => b.Property(p.ShortName, p.Type, isPublic: false))
            //     .Line("public int Order => 1000;")
            //     .NewLine()
            //     .Method($"public bool TryExecute({contentClassName} content, {contentApiModelClass} contentApiModel, IContentConverterPipelineContext pipelineContext)", methodBuilder => methodBuilder
            //         .Tab()
            //         .Foreach(contentClassInfo.Properties, (b, p) => b.Line($"contentApiModel.{p.PropertyName} = {GetConverter(p)};"))
            //         .Line("return true;"))
            //     .Build();
            
        }
    }
}
