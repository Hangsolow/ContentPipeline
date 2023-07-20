using ContentPipeline.CodeBuilders;

namespace ContentPipeline.SourceGenerator;

internal partial class Emitter
{
    internal IEnumerable<CodeSource> GetServiceCodeSources(IEnumerable<ContentClass> contentClasses)
    {
        yield return new CodeSource("PipelineArgs.g.cs", CreatePipelineArgsSource());
        yield return new CodeSource("ContentPipelineContext.g.cs", CreateContentPipelineContext());
        yield return new CodeSource("BaseContentPipelineService.g.cs", CreateBasePipelineService());
        yield return new CodeSource("ContentPipelineService.g.cs", CreateContentPipelineService());
        yield return new CodeSource("DefaultContentPipeline.g.cs", CreateContentPipeline());
        yield return new CodeSource("XhtmlRenderService.g.cs", CreateXhtmlRenderService());

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
                protected abstract IContentPipelineModel RunPipelineForContent(IContentData contentData, IContentPipelineContext pipelineContext);

                public virtual IContentPipelineModel ExecutePipeline(IContentData content, IContentPipelineContext pipelineContext) => RunPipelineForContent(content, pipelineContext);
                
                public virtual IContentPipelineModel ExecutePipeline(PipelineArgs pipelineArgs) => RunPipelineForContent(pipelineArgs.Content, new ContentPipelineContext(pipelineArgs.HttpContext, pipelineArgs.Language, this));
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
                .Line($"public ContentPipelineService({CodeConsts.NewLine}\t\t\t{string.Join($", {CodeConsts.NewLine}\t\t\t", contentClasses.Select(c => $"IContentPipeline<{c.FullyQualifiedName}, {GetPipelineModelFullName(c)}> {GetContentPipelineName(c)}"))})")
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

        string GetContentPipelineName(ContentClass contentClass) => $"{contentClass.Group}{contentClass.Guid.Substring(0, 8)}{contentClass.Name}";

        string CreateContentPipeline() =>
            $$"""
            #nullable enable
            namespace {{SharedNamespace}}.Services;

            using {{SharedNamespace}}.Interfaces;
            using {{SharedNamespace}}.Models;
            using EPiServer.Core;

            internal class DefaultContentPipeline<TContent, TPipelineModel> : IContentPipeline<TContent, TPipelineModel> where TContent : IContentData where TPipelineModel : IContentPipelineModel, new()
            {
                public DefaultContentPipeline(IEnumerable<IContentPipelineStep<TContent, TPipelineModel>> contentPipelineSteps, IEnumerable<IContentPipelineStep<IContentData, ContentPipelineModel>> sharedPipelineSteps)
                {
                    ContentPipelineSteps = contentPipelineSteps.OrderBy(ps => ps.Order);
                    SharedPipelineSteps = sharedPipelineSteps.OrderBy(ps => ps.Order);
                }

                private IEnumerable<IContentPipelineStep<TContent, TPipelineModel>> ContentPipelineSteps { get; }

                private IEnumerable<IContentPipelineStep<IContentData, ContentPipelineModel>> SharedPipelineSteps { get; }

                public TPipelineModel Run(TContent content, IContentPipelineContext pipelineContext)
                {
                    TPipelineModel pipelineModel = new();
                    if (pipelineModel is ContentPipelineModel sharedPipelineModel)
                    {
                        foreach (var sharedPipelineStep in SharedPipelineSteps)
                        {
                            sharedPipelineStep.Execute(content, sharedPipelineModel, pipelineContext);
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

        string CreateXhtmlRenderService() =>
            $$"""
            #nullable enable
            namespace {{SharedNamespace}}.Services;

            using {{SharedNamespace}}.Interfaces;
            using EPiServer.Core;
            using EPiServer.Web;
            using EPiServer.Web.Mvc.Html;
            using EPiServer.Web.Routing;
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Mvc.Abstractions;
            using Microsoft.AspNetCore.Mvc.Rendering;
            using Microsoft.AspNetCore.Mvc.ViewFeatures;
            using Microsoft.AspNetCore.Routing;
            using Microsoft.Extensions.DependencyInjection;
            using System;
            using System.IO;

            public class XhtmlRenderService : IXhtmlRenderService
            {
                private readonly ITempDataProvider _tempDataProvider;
                private readonly IServiceProvider _serviceProvider;

                public XhtmlRenderService(ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
                {
                    _tempDataProvider = tempDataProvider;
                    _serviceProvider = serviceProvider;
                }

                public virtual string RenderXhtmlString(HttpContext? context, XhtmlString? xhtmlString)
                {
                    //we use the service provider here to ensure we always gets a fresh htmlhelper, eles we start getting wierd error when the sites comes under load
                    var htmlHelper = _serviceProvider.GetService<IHtmlHelper>();
                    if (context is null || xhtmlString is null)
                    {
                        return string.Empty;
                    }

                    using var stringWriter = new StringWriter();

                    var viewContext = new ViewContext
                    {
                        HttpContext = context,
                        Writer = stringWriter,
                        RouteData = new RouteData(),
                        TempData = new TempDataDictionary(context, _tempDataProvider),
                        ActionDescriptor = new ActionDescriptor()
                    };

                    if (htmlHelper is IViewContextAware viewContextAware)
                    {
                        viewContextAware.Contextualize(viewContext);
                    }

                    var virtualPathArguments = new VirtualPathArguments
                    {
                        ContextMode = ContextMode.Default,
                        ForceCanonical = true,
                        ForceAbsolute = false,
                        ValidateTemplate = false
                    };

                    htmlHelper.RenderXhtmlString(xhtmlString, virtualPathArguments);
                    stringWriter.Flush();

                    return stringWriter.ToString();
                }
            }
            """;

        string CreateContentPipelineContext() =>
            $$"""
            #nullable enable
            namespace {{SharedNamespace}}.Entities;

            using System.Globalization;
            using Microsoft.AspNetCore.Http;
            using EPiServer.Core;
            using {{SharedNamespace}}.Interfaces;

            internal record ContentPipelineContext(HttpContext HttpContext, CultureInfo? Language, IContentPipelineService ContentPipelineService) : IContentPipelineContext;
            """;
    }
}