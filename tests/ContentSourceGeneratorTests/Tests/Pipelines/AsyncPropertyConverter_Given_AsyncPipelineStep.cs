using ContentPipeline.Interfaces;
using ContentPipeline.Models.Awesome;
using ContentPipeline.Pipelines.Awesome.Steps;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.ContentPropertyConverters;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities;
using EPiServer.Core;
using NSubstitute;

namespace ContentPipelineSourceGeneratorTests.Tests.Pipelines;

[Trait("Pipelines", "")]
public class AsyncPropertyConverter_Given_AsyncPipelineStep
{
    [Fact]
    public async Task Should_Use_GetValueAsync_When_Available()
    {
        var step = new AsyncConverterPagePipelineStep(new AsyncOnlyBoolConverter());
        var content = new AsyncConverterPage
        {
            AsyncValue = new XhtmlString("<p>test</p>")
        };
        var model = new AsyncConverterPagePipelineModel();
        var context = Substitute.For<IContentPipelineContext>();

        await step.ExecuteAsync(content, model, context);

        Assert.True(model.AsyncValue);
    }
}
