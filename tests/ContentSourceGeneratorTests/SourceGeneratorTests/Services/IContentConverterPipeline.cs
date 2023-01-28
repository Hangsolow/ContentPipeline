using ContentPipeline.Models;
using EPiServer.Core;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Services
{
    internal interface IContentConverterPipeline<TContent, TContentPipelineModel>
        where TContent : IContentData
        where TContentPipelineModel : ContentPipelineModel, new()
    {
    }
}