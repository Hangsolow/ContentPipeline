using ContentPipeline.Interfaces;
using EPiServer.Core;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.ContentPropertyConverters;

public class CustomConverter : IContentPropertyConverter<XhtmlString?, bool>
{
    public bool GetValue(XhtmlString? property, IContentData content, string propertyName,
        IContentPipelineContext pipelineContext, Dictionary<string, string>? config = null)
    {
        return property?.IsEmpty is false;
    }
}