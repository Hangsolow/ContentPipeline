using ContentPipeline.Interfaces;
using ContentPipeline.Models.Awesome;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.PipelineSteps;

namespace ContentPipelineSourceGeneratorTests.Tests.Pipelines;

[Trait("Pipelines", "")]
public class PipelineStepPropertiesTests
{
    [Fact]
    public void AsyncPipelineStep_Should_Have_IsAsync_True()
    {
        var step = new DefaultAsyncPipelineStep();
        
        Assert.True(step.IsAsync);
    }

    [Fact]
    public void AsyncPipelineStep_Should_Have_IsSync_False()
    {
        var step = new DefaultAsyncPipelineStep();
        
        Assert.False(step.IsSync);
    }

    [Fact]
    public void SyncPipelineStep_Should_Have_IsAsync_False()
    {
        IContentPipelineStep<ContentPage, ContentPagePipelineModel> step = new TestSyncPipelineStep();
        
        Assert.False(step.IsAsync);
    }

    [Fact]
    public void SyncPipelineStep_Should_Have_IsSync_True()
    {
        IContentPipelineStep<ContentPage, ContentPagePipelineModel> step = new TestSyncPipelineStep();
        
        Assert.True(step.IsSync);
    }

    [Fact]
    public void OverriddenAsyncPipelineStep_Should_Respect_Override_IsAsync()
    {
        var step = new OverriddenAsyncPipelineStep();
        
        Assert.False(step.IsAsync);
    }

    [Fact]
    public void OverriddenAsyncPipelineStep_Should_Respect_Override_IsSync()
    {
        var step = new OverriddenAsyncPipelineStep();
        
        Assert.True(step.IsSync);
    }

    [Fact]
    public void IsSync_Should_Be_Opposite_Of_IsAsync()
    {
        var asyncStep = new DefaultAsyncPipelineStep();
        IContentPipelineStep<ContentPage, ContentPagePipelineModel> syncStep = new TestSyncPipelineStep();
        
        Assert.Equal(!asyncStep.IsAsync, asyncStep.IsSync);
        Assert.Equal(!syncStep.IsAsync, syncStep.IsSync);
    }

    private class TestSyncPipelineStep : IContentPipelineStep<ContentPage, ContentPagePipelineModel>
    {
        public int Order => 100;

        public void Execute(ContentPage content, ContentPagePipelineModel contentPipelineModel, IContentPipelineContext pipelineContext)
        {
            // Sync execution
        }
    }

    private class OverriddenAsyncPipelineStep : ContentPipeline.Pipelines.AsyncContentPipelineStep<ContentPage, ContentPagePipelineModel>
    {
        public OverriddenAsyncPipelineStep() : base(order: 100)
        {
        }

        public override bool IsAsync => false;

        public override Task ExecuteAsync(ContentPage content, ContentPagePipelineModel contentPipelineModel, IContentPipelineContext pipelineContext)
        {
            return Task.CompletedTask;
        }
    }
}
