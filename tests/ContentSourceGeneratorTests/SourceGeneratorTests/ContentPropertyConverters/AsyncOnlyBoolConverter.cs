using ContentPipeline.Interfaces;
using EPiServer.Core;
using System.Threading.Tasks;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.ContentPropertyConverters;

public class AsyncOnlyBoolConverter : IContentPropertyConverter<XhtmlString?, bool>, IAsyncContentPropertyConverter<XhtmlString?, bool>
{
    public bool GetValue(XhtmlString? property, IContentData content, string propertyName, IContentPipelineContext pipelineContext, Dictionary<string, string>? config = null)
    {
        throw new InvalidOperationException("Sync converter path should not be used for async execution.");
    }

    public Task<bool> GetValueAsync(XhtmlString? property, IContentData content, string propertyName, IContentPipelineContext pipelineContext, Dictionary<string, string>? config = null)
    {
        return Task.FromResult(property?.IsEmpty is false);
    }
}
