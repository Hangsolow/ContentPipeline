using System.Globalization;
using ContentPipeline.Entities;
using ContentPipeline.Interfaces;
using EPiServer.Core;
using Microsoft.AspNetCore.Http;

namespace ContentPipeline.Services;

public partial class ContentPipelineService
{
    public ContentPipelineService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    private IServiceProvider ServiceProvider { get; }
    protected override TContentPipelineModel RunPipeline<TContent, TContentPipelineModel>(TContent content, IContentPipelineContext context)
    {
        
        throw new NotImplementedException();
    }

    public override IContentPipelineModel ExecutePipeline(IContentData content, IContentPipelineContext pipelineContext)
    {
        return RunPipelineForContent(content, pipelineContext);
    }

    public override IContentPipelineModel ExecutePipeline(PipelineArgs pipelineArgs)
    {
        var context = new PipelineContext(pipelineArgs.HttpContext, pipelineArgs.Language, this);

        return RunPipelineForContent(pipelineArgs.Content, context);
    }
}

internal record PipelineContext(HttpContext HttpContext, CultureInfo? Language, IContentPipelineService ContentPipelineService) : IContentPipelineContext;