using ContentPipeline.Attributes;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.ContentPropertyConverters;
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities;

[ContentType(GUID = "9ab58407-5159-4a72-a9b8-64f53d81871e")]
[ContentPipelineModel("Awesome", -95)]
public class AsyncConverterPage : PageData
{
    [ContentPipelinePropertyConverter<AsyncOnlyBoolConverter>]
    public virtual XhtmlString? AsyncValue { get; set; }
}
