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

    [Fact]
    public void ContentPipelineStep_Should_Have_IsAsync_False()
    {
        var step = new DefaultSyncPipelineStep();
        
        Assert.False(step.IsAsync);
    }

    [Fact]
    public void ContentPipelineStep_Should_Have_IsSync_True()
    {
        var step = new DefaultSyncPipelineStep();
        
        Assert.True(step.IsSync);
    }

    [Fact]
    public void ContentPipelineStep_Execute_Should_Be_Called()
    {
        var step = new TestableContentPipelineStep();
        var content = new ContentPage();
        var model = new ContentPagePipelineModel();
        var context = new TestPipelineContext();

        step.Execute(content, model, context);

        Assert.True(step.ExecuteCalled);
    }

    [Fact]
    public async Task ContentPipelineStep_ExecuteAsync_Should_Call_Execute()
    {
        var step = new TestableContentPipelineStep();
        var content = new ContentPage();
        var model = new ContentPagePipelineModel();
        var context = new TestPipelineContext();

        await step.ExecuteAsync(content, model, context);

        Assert.True(step.ExecuteCalled);
    }

    [Fact]
    public void OverriddenContentPipelineStep_Should_Respect_Override_IsAsync()
    {
        var step = new OverriddenContentPipelineStep();
        
        Assert.True(step.IsAsync);
    }

    [Fact]
    public void OverriddenContentPipelineStep_Should_Respect_Override_IsSync()
    {
        var step = new OverriddenContentPipelineStep();
        
        Assert.False(step.IsSync);
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

    private class TestableContentPipelineStep : ContentPipeline.Pipelines.ContentPipelineStep<ContentPage, ContentPagePipelineModel>
    {
        public TestableContentPipelineStep() : base(order: 100)
        {
        }

        public bool ExecuteCalled { get; private set; }

        public override void Execute(ContentPage content, ContentPagePipelineModel contentPipelineModel, IContentPipelineContext pipelineContext)
        {
            ExecuteCalled = true;
        }
    }

    private class OverriddenContentPipelineStep : ContentPipeline.Pipelines.ContentPipelineStep<ContentPage, ContentPagePipelineModel>
    {
        public OverriddenContentPipelineStep() : base(order: 100)
        {
        }

        public override bool IsAsync => true;

        public override void Execute(ContentPage content, ContentPagePipelineModel contentPipelineModel, IContentPipelineContext pipelineContext)
        {
            // Override execute
        }
    }

    private class TestPipelineContext : IContentPipelineContext
    {
        public Microsoft.AspNetCore.Http.HttpContext HttpContext => null!;
        public ContentPipeline.Interfaces.IContentPipelineService ContentPipelineService => null!;
        public System.Globalization.CultureInfo? Language => null;
    }
}
