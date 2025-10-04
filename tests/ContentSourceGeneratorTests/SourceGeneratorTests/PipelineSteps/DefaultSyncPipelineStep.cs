using ContentPipeline.Interfaces;
using ContentPipeline.Models.Awesome;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.PipelineSteps;

internal class DefaultSyncPipelineStep() : ContentPipeline.Pipelines.ContentPipelineStep<ContentPage, ContentPagePipelineModel>(order: 100)
{
    public override void Execute(ContentPage content, ContentPagePipelineModel contentPipelineModel, IContentPipelineContext pipelineContext)
    {
        // Sync execution
    }
}
