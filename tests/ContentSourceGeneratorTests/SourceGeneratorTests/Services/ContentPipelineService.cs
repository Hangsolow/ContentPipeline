using ContentPipeline.Entities;
using ContentPipeline.Interfaces;
using EPiServer.Core;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities;
using ContentPipeline.Models.Awesome;

namespace ContentPipeline.Services;

public partial class ContentPipelineService
{
    protected override TContentPipelineModel RunPipeline<TContent, TContentPipelineModel>(TContent content, IContentPipelineContext context)
    {
        
        return new TContentPipelineModel();
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