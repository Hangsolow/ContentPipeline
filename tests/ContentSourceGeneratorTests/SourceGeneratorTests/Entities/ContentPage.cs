using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Attributes;
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities;

[ContentType(GUID = "308068d7-e9b1-4958-b13b-bc612707cb85")]
public class ContentPage : PageData
{
    public virtual string? Title { get; set; }
    
    public virtual IList<string>? ListOfStrings { get; set; }
}