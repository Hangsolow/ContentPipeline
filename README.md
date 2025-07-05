# Content Pipeline

A source generator for Optimizely CMS that converts content models to JSON-friendly pipeline models, providing more customization options than the standard [Optimizely Content Delivery API](https://docs.developers.optimizely.com/content-management-system/v1.5.0-content-delivery-api/docs/content-delivery-api).

**Note:** You should always evaluate whether the Content Delivery API meets your needs before using this library, as it may provide sufficient functionality for many use cases.

## Key Features

- 🚀 **Source Generator**: Automatically generates pipeline models at compile time
- 🎯 **Type-Safe**: Strongly typed models with full IntelliSense support
- 🔧 **Customizable**: Create custom property converters for complex transformations
- ⚡ **Performance**: Minimal runtime overhead with compile-time code generation
- 🏗️ **Pipeline Architecture**: Extensible pipeline steps for content processing

## Installation

Install the NuGet package:

```bash
dotnet add package Hangsolow.ContentPipeline
```

## Quick Start

1. **Register Services**: Add ContentPipeline services to your DI container in `Program.cs` or `Startup.cs`:

```csharp
services.AddContentPipelineServices();
```

2. **Use the Service**: Resolve `IContentPipelineService` from the DI container:

```csharp
using ContentPipeline.Interfaces;
using EPiServer.Core;
using Microsoft.AspNetCore.Http;

public class ContentService(IContentPipelineService contentPipelineService)
{
    public IContentPipelineModel? ConvertToPipelineModel(IContent content, HttpContext httpContext)
    {
        var pipelineContext = new ContentPipelineContext 
        { 
            HttpContext = httpContext 
        };
        
        return contentPipelineService.ExecutePipeline(content, pipelineContext);
    }
}
```

## Basic Usage

### 1. Enable Content Model in the Content Pipeline

To enable a content model for conversion in the ContentPipeline, mark it with both `ContentPipelineModelAttribute` and `ContentTypeAttribute`. The source generator will automatically detect and process these models.

The `ContentPipelineModelAttribute` has two optional properties:
- **Group**: Organizes models into logical groups (defaults to `"Common"` if not set)
- **Order**: Determines processing order within the group

```csharp
using EPiServer.Core;
using EPiServer.DataAnnotations;
using ContentPipeline.Attributes;

[ContentType(GUID = "308068d7-e9b1-4958-b13b-bc612707cb85")]
[ContentPipelineModel("Awesome")]
public class ContentPage : PageData
{
    public virtual string? Title { get; set; }
    
    public virtual string? Description { get; set; }
}
```

This generates a corresponding pipeline model:

```csharp
namespace ContentPipeline.Models.Awesome
{
    public partial class ContentPagePipelineModel : IContentPipelineModel
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
```

### 2. Create a Custom Field Mapping

Custom property converters enable deep customization of how content properties are transformed in the pipeline model. This is one of the main advantages of using ContentPipeline over the standard Content Delivery API.

#### Create a Custom PropertyConverter

A PropertyConverter implements the `IContentPropertyConverter<TProperty, TValue>` interface:
- **TProperty**: The source property type (e.g., `string`, `XhtmlString`, `ContentReference`)
- **TValue**: The target value type in the pipeline model

```csharp
using ContentPipeline.Interfaces;
using EPiServer.Core;
using System.Collections.Generic;

public class XhtmlToContentSummaryConverter : IContentPropertyConverter<XhtmlString?, ContentSummary>
{
    public ContentSummary GetValue(
        XhtmlString? property, 
        IContentData content, 
        string propertyName,
        IContentPipelineContext pipelineContext, 
        Dictionary<string, string>? config = null)
    {
        return new ContentSummary
        {
            HasContent = property?.IsEmpty == false,
            PlainText = property?.ToHtmlString()?.StripHtml() ?? string.Empty,
            WordCount = property?.ToHtmlString()?.CountWords() ?? 0
        };
    }
}

public class ContentSummary
{
    public bool HasContent { get; set; }
    public string PlainText { get; set; } = string.Empty;
    public int WordCount { get; set; }
}
```

**Register the converter** in your DI container:

```csharp
services.AddSingleton<XhtmlToContentSummaryConverter>();
```

#### Using a Custom PropertyConverter

Apply the `ContentPipelinePropertyConverterAttribute` to use your custom converter:

```csharp
using ContentPipeline.Attributes;
using EPiServer.Core;
using EPiServer.DataAnnotations;

[ContentType(GUID = "308068d7-e9b1-4958-b13b-bc612707cb85")]
[ContentPipelineModel("Awesome")]
public class ContentPage : PageData
{
    public virtual string? Title { get; set; }

    [ContentPipelinePropertyConverter<XhtmlToContentSummaryConverter>]
    public virtual XhtmlString? MainContent { get; set; }
}
```

This generates:

```csharp
namespace ContentPipeline.Models.Awesome
{
    public partial class ContentPagePipelineModel : IContentPipelineModel
    {
        public string? Title { get; set; }
        public ContentSummary MainContent { get; set; }
    }
}
```

### 3. Ignore a Content Property

Use the `ContentPipelineIgnoreAttribute` to exclude properties from the generated pipeline model:

```csharp
using ContentPipeline.Attributes;
using EPiServer.Core;
using EPiServer.DataAnnotations;

[ContentType(GUID = "308068d7-e9b1-4958-b13b-bc612707cb85")]
[ContentPipelineModel("Awesome")]
public class ContentPage : PageData
{
    public virtual string? Title { get; set; }

    [ContentPipelineIgnore]
    public virtual ContentReference? InternalLink { get; set; }
    
    [ContentPipelineIgnore]
    public virtual string? AdminNotes { get; set; }
}
```

> **Note**: The `EPiServer.DataAnnotations.IgnoreAttribute` has no effect in ContentPipeline context. Always use `ContentPipelineIgnoreAttribute`.

## Documentation

For comprehensive guides and advanced topics, see our [complete documentation](./docs/):

**Quick Links:**
- **[📚 Getting Started Guide](./docs/getting-started.md)** - Complete setup walkthrough
- **[📖 Documentation Index](./docs/README.md)** - All documentation topics

**Advanced Topics:**

- **[Architecture](./docs/architecture.md)** - Source generator architecture and core concepts
- **[Configuration](./docs/configuration.md)** - Advanced configuration options
- **[Testing](./docs/testing.md)** - Testing strategies and examples
- **[Advanced Usage](./docs/advanced-usage.md)** - Complex scenarios and patterns
- **[API Reference](./docs/api-reference.md)** - Complete API documentation
- **[Troubleshooting](./docs/troubleshooting.md)** - Common issues and solutions

## Contributing

Contributions are welcome! Please read our contributing guidelines and submit pull requests to the main repository.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
