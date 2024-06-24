using ContentPipeline.Interfaces;
using ContentPipeline.Models.Awesome;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.PipelineSteps;
internal class DefaultAsyncPipelineStep() : ContentPipeline.Pipelines.AsyncContentPipelineStep<ContentPage, ContentPagePipelineModel>(order: 100)
{
    public async override Task ExecuteAsync(ContentPage content, ContentPagePipelineModel contentPipelineModel, IContentPipelineContext pipelineContext)
    {
        await Task.CompletedTask;
    }
}
