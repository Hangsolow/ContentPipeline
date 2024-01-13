# Content Pipeline

The Content Pipeline is intended to be used with [Optimizely CMS](https://www.optimizely.com/products/orchestrate/content-management/) to convert content to more json friendly models, when needing more customization options then [Optimizely content delivery api](https://docs.developers.optimizely.com/content-management-system/v1.5.0-content-delivery-api/docs/content-delivery-api) provides.

You should always check if the content delivery api can be used for your usecase before starting using this library.

# Getting Started

Install #NUGET_PACKACGE
add `services.AddContentPipelineServices()` in program.cs/startup.cs
and then you can resolve IContentPipelineService from the DI container and are good to go with the default configuration

```csharp
using ContentPipeline.Entities;
using ContentPipeline.Interfaces;

public class ContentService(IContentPipelineService contentPipelineService)
{
    public IContentPipelineModel ConvertToPipelineModel(IContent content, HttpContext httpContext)
    {
        var pipelineArgs = new PipelineArgs
        {
             HttpContext = httpContext,
             Content = content
        };
        var pipelineModel = ContentPipelineService.ExecutePipeline(pipelineArgs);
    }
}
```

# Enable Content Model in the Content Pipeline

in order for a Content Model to enabled for convertion in the ContentPipeline it must be marked by the `ContentPipelineModelAttribute` and the `ContentTypeAttribute` in order for the source generator to pick it up
`ContentPipelineModelAttribute` have two properties: Group and Order, both are optional but we recommend setting Group as this will default to `"Common"` if not set.

```csharp
[ContentType(GUID = "308068d7-e9b1-4958-b13b-bc612707cb85")]
[ContentPipelineModel("Awesome")]
public class ContentPage : PageData
{
    public virtual string? Title { get; set; }
}
```

# Create a custom field mapping

One of the main reasons for using this library is the option for creating a custom field mapping using a custom property converter that enables deep and easy customization of the pipeline model
A couple of steps a needed for creating and using a PropertyConverter

## Create a custom PropertyConverter

A PropertyConverter needs to implement the `IContentPropertyConverter<TProperty, out TValue>` interface where TProperty is the property type on the content (e.g string, XhtmlString, ContentReference and so on) and TValue is what the property is converted to (and what will be on the pipeline model)

```csharp
using ContentPipeline.Interfaces;
using EPiServer.Core;

public class CustomConverter : IContentPropertyConverter<XhtmlString?, bool>
{
    public bool GetValue(XhtmlString? property, IContentData content, string propertyName,
        IContentPipelineContext pipelineContext, Dictionary<string, string>? config = null)
    {
        return property?.IsEmpty is false;
    }
}
```

it will need to be registed in the service container
`services.AddSingleton<CustomConverter>()`

## using a custom PropertyConverter

just use ContentPipelinePropertyConverterAttribute with your custom PropertyConverter and thats it.

```csharp
using ContentPipeline.Attributes;

[ContentType(GUID = "308068d7-e9b1-4958-b13b-bc612707cb85")]
[ContentPipelineModel("Awesome")]
public class ContentPage : PageData
{
    public virtual string? Title { get; set; }

    [ContentPipelinePropertyConverter<CustomConverter>]
    public virtual XhtmlString? CustomMapping { get; set; }
}
```

this will result in the `ContentPagePipelineModel` looking like this:

```csharp
namespace ContentPipeline.Models.Awesome
{
    public partial class ContentPagePipelineModel : ContentPipeline.Models.ContentPipelineModel
    {
        public string? Title { get; set; }
        public bool CustomMapping { get; set; }
    }
}
```

# Ignore a content property

In order to remove a property from the pipeline model use the `ContentPipelineIgnoreAttribute`

```csharp
using ContentPipeline.Attributes;

[ContentType(GUID = "308068d7-e9b1-4958-b13b-bc612707cb85")]
[ContentPipelineModel("Awesome")]
public class ContentPage : PageData
{
   public virtual string? Title { get; set; }

   [ContentPipelineIgnore]
   public virtual ContentReference? IgnoreLink { get; set; }
}
```

this will remove the property from the pipeline model, note that using `EPiServer.DataAnnotations.IgnoreAttribute` does not do anything in the context of ContentPipeline
