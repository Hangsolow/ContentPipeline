using ContentPipeline.CodeBuilders;

namespace ContentPipeline.SourceGenerator;

internal partial class Emitter
{
    internal IEnumerable<CodeSource> GetServiceCodeSources(IEnumerable<ContentClass> contentClasses)
    {
        yield return new CodeSource("PipelineArgs.g.cs", CreatePipelineArgsSource());
        yield return new CodeSource("BaseContentPipelineService.g.cs", CreateBasePipelineService());
        yield return new CodeSource("ContentPipelineService.g.cs", CreateContentPipelineService());

        string CreatePipelineArgsSource() =>
            $$"""
            #nullable enable
            namespace {{SharedNamespace}}.Entities;
            
            using System.Globalization;
            using Microsoft.AspNetCore.Http;
            using EPiServer.Core;

            public partial record PipelineArgs
            {
                public required HttpContext HttpContext { get; init; }

                public required IContentData Content { get; init; }

                public CultureInfo? Language { get; init; }
            }
            """;

        string CreateBasePipelineService() =>
            $$"""
            #nullable enable
            namespace {{SharedNamespace}}.Services;
            
            using EPiServer.Core;
            using {{SharedNamespace}}.Interfaces;
            using {{SharedNamespace}}.Entities;
 
            public abstract partial class BaseContentPipelineService : IContentPipelineService
            {
                protected abstract TContentPipelineModel RunPipeline<TContent, TContentPipelineModel>(TContent content, IContentPipelineContext context)
                    where TContent : IContentData
                    where TContentPipelineModel : IContentPipelineModel, new();

                protected abstract IContentPipelineModel RunPipelineForContent(IContentData contentData,
                    IContentPipelineContext pipelineContext);

                public abstract IContentPipelineModel ExecutePipeline(IContentData content, IContentPipelineContext pipelineContext);
                
                public abstract IContentPipelineModel ExecutePipeline(PipelineArgs pipelineArgs);
            }
            """;

        string CreateContentPipelineService() =>
            CSharpCodeBuilder.Create()
                .Line("#nullable enable")
                .Using("EPiServer.Core")
                .Using("System")
                .Using("System.Collections.Generic")
                .Using("System.Linq")
                .Using("System.Threading.Tasks")
                .Using($"{SharedNamespace}.Interfaces")
                .Using("Microsoft.Extensions.DependencyInjection")
                .Namespace($"{SharedNamespace}.Services")
                .Class("public partial class ContentPipelineService : BaseContentPipelineService")
                .Tab()
                .NewLine()
                .Method(
                    $"protected override IContentPipelineModel RunPipelineForContent(IContentData content, IContentPipelineContext context)",
                    methodBuilder => methodBuilder
                        .Line("return content switch", 1)
                        .CodeBlock(end: "};")
                        .Tab()
                        .Foreach(contentClasses,
                            (b, contentClass) =>
                                b.Line(
                                    $"{contentClass.FullyQualifiedName} castContent => RunPipeline<{contentClass.FullyQualifiedName}, {SharedNamespace}.Models.{contentClass.Group}.{contentClass.Name}PipelineModel>(castContent, context),"))
                        .Line($"_ => new {SharedNamespace}.Models.ContentPipelineModel()"))
                .NewLine()
                .Build();
    }
}