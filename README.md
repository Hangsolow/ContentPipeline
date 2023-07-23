# Content Pipeline

ContentPipeline is a project to helps to convert Content(IContentData) from [Optimizely CMS 12](https://docs.developers.optimizely.com/content-management-system/docs) to a more serialization friendly data objects while enabling customisation via pipelines and property converters


## How to enable Content

the ContentPipeline source generator selects content that have both the ContentType and ContentPipelineModel attribute
```csharp
using ContentPipeline.Attributes;
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace ContentPipelineDemo.Models;

[ContentType(GUID = "a446798f-e7f5-4f83-a9a2-b72047c7eaa1")]
[ContentPipelineModel("Group")]
public class ContentBlock : BlockData
{
    public virtual string? Header { get; set; }

    public virtual XhtmlString? Text { get; set; }

    public virtual ContentReference? Link { get; set; }
}
```
note that ContentPipelineModelAttribute takes a optional group name that is used to group the generated code, if none is given then it defaults to 'Common'

## ContentPropertyConverter
You can overwrite the default behavior for converting content properties by creating a ContentPropertyConverter and mark a property to use it.

Model:
```csharp
using ContentPipeline.Attributes;
using ContentPipelineDemo.ContentPropertyConverters;
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace ContentPipelineDemo.Models;

[ContentType(GUID = "a446798f-e7f5-4f83-a9a2-b72047c7eaa1")]
[ContentPipelineModel("Group")]
public class ContentBlock : BlockData
{
    public virtual string? Header { get; set; }
    
    [ContentPipelinePropertyConverter<CustomConverter>]
    public virtual XhtmlString? Text { get; set; }
    
    public virtual ContentReference? Link { get; set; }
}
```
Converter:
```csharp
using ContentPipeline.Interfaces;
using EPiServer.Core;

namespace ContentPipelineDemo.ContentPropertyConverters;

public class CustomConverter : IContentPropertyConverter<XhtmlString?, bool>
{
    public bool GetValue(XhtmlString? property, IContentData content, string propertyName,
        IContentPipelineContext pipelineContext, Dictionary<string, string>? config = null)
    {
        return property?.IsEmpty is false;
    }
}
```

This will replace the default mapping in the generated pipelinestep for this content type with a call to CustomConverter and the generated model will be using 'bool' for the Text field instead of the default string.

This a simple but powerful method of customisation.

### Ignore fields
Adding ´[ContentPipelineIgnore]´ to a field will remove it from the generated class
```csharp
using ContentPipeline.Attributes;
using ContentPipelineDemo.ContentPropertyConverters;
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace ContentPipelineDemo.Models;

[ContentType(GUID = "a446798f-e7f5-4f83-a9a2-b72047c7eaa1")]
[ContentPipelineModel("Group")]
public class ContentBlock : BlockData
{
    [ContentPipelineIgnore]
    public virtual string? Header { get; set; }
    
    [ContentPipelinePropertyConverter<CustomConverter>]
    public virtual XhtmlString? Text { get; set; }
    
    public virtual ContentReference? Link { get; set; }
}
```