using ContentPipeline.CodeBuilders;

namespace ContentPipeline.SourceGenerator;

internal partial class Emitter
{
    internal IEnumerable<CodeSource> GetServiceCodeSources(IEnumerable<ContentClass> contentClasses)
    {
        yield return new CodeSource("PipelineArgs.g.cs", CreatePipelineArgsSource());
        yield return new CodeSource("BaseContentPipelineService.g.cs", CreateBasePipelineService());
        yield return new CodeSource("ContentPipelineService.g.cs", CreateContentPipelineService());
        yield return new CodeSource("DefaultContentPipeline.g.cs", CreateContentPipeline());

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
                .Line($"public ContentPipelineService(\n\t\t\t{string.Join(", \n\t\t\t", contentClasses.Select(c => $"IContentPipeline<{c.FullyQualifiedName}, {GetPipelineModelFullName(c)}> {GetContentPipelineName(c)}"))})")
                .CodeBlock(block => block.Tab().Foreach(contentClasses, 
                    (b, contentClass) => 
                    b.Line($"this.{GetContentPipelineName(contentClass)} = {GetContentPipelineName(contentClass)};")))

                .Foreach(contentClasses, (pBuilder, contentClass) => pBuilder.Property(GetContentPipelineName(contentClass), $"IContentPipeline<{contentClass.FullyQualifiedName}, {GetPipelineModelFullName(contentClass)}>", isPublic: false))
                .Method(
                    $"protected override IContentPipelineModel RunPipelineForContent(IContentData content, IContentPipelineContext context)",
                    methodBuilder => methodBuilder
                        .Line("return content switch", 1)
                        .CodeBlock(end: "};")
                        .Tab()
                        .Foreach(contentClasses,
                            (b, contentClass) =>
                                b.Line(
                                    $"{contentClass.FullyQualifiedName} castContent => {GetContentPipelineName(contentClass)}.Run(castContent, context),"))
                        .Line($"_ => new {SharedNamespace}.Models.ContentPipelineModel()"))
                .NewLine()
                .Build();
        string GetPipelineModelFullName(ContentClass contentClass) => $"{SharedNamespace}.Models.{contentClass.Group}.{contentClass.Name}PipelineModel";
        string GetContentPipelineName(ContentClass contentClass) => $"{contentClass.Group}{contentClass.Guid.Substring(0, 8)}{contentClass.Name}"; 
        string CreateContentPipeline() =>
            $$"""
            using ContentPipeline.Interfaces;
            using ContentPipeline.Models;
            using EPiServer.Core;

            namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Services;

            internal class DefaultContentPipeline<TContent, TPipelineModel> : IContentPipeline<TContent, TPipelineModel> where TContent : IContentData where TPipelineModel : IContentPipelineModel, new()
            {
                public DefaultContentPipeline(IEnumerable<IContentPipelineStep<TContent, TPipelineModel>> contentPipelineSteps, IEnumerable<IContentPipelineStep<IContent, ContentPipelineModel>> sharedPipelineSteps)
                {
                    ContentPipelineSteps = contentPipelineSteps.OrderBy(ps => ps.Order);
                    SharedPipelineSteps = sharedPipelineSteps.OrderBy(ps => ps.Order);
                }

                private IEnumerable<IContentPipelineStep<TContent, TPipelineModel>> ContentPipelineSteps { get; }

                private IEnumerable<IContentPipelineStep<IContent, ContentPipelineModel>> SharedPipelineSteps { get; }

                public TPipelineModel Run(TContent content, IContentPipelineContext pipelineContext)
                {
                    TPipelineModel pipelineModel = new();
                    if (content is IContent contentModel && pipelineModel is ContentPipelineModel sharedPipelineModel)
                    {
                        foreach (var sharedPipelineStep in SharedPipelineSteps)
                        {
                            sharedPipelineStep.Execute(contentModel, sharedPipelineModel, pipelineContext);
                        }
                    }

                    foreach (var step in ContentPipelineSteps)
                    {
                        step.Execute(content, pipelineModel, pipelineContext);
                    }

                    return pipelineModel;
                }
            }
            """;
    }
}