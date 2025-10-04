# API Reference

This document provides a comprehensive reference for all public APIs in ContentPipeline.

## Core Interfaces

### IContentPipelineService

Main service interface for executing content pipelines.

```csharp
public interface IContentPipelineService
{
    IContentPipelineModel? ExecutePipeline(IContent content, IContentPipelineContext context);
    Task<IContentPipelineModel?> ExecutePipelineAsync(IContent content, IContentPipelineContext context);
}
```

#### Methods

**ExecutePipeline**
- **Parameters**:
  - `content` (`IContent`): The content to process
  - `context` (`IContentPipelineContext`): Processing context
- **Returns**: `IContentPipelineModel?` - The generated pipeline model or null
- **Description**: Synchronously executes the pipeline for the given content

**ExecutePipelineAsync**
- **Parameters**:
  - `content` (`IContent`): The content to process
  - `context` (`IContentPipelineContext`): Processing context
- **Returns**: `Task<IContentPipelineModel?>` - The generated pipeline model or null
- **Description**: Asynchronously executes the pipeline for the given content

### IContentPipelineContext

Provides context information for pipeline execution.

```csharp
public interface IContentPipelineContext
{
    HttpContext HttpContext { get; set; }
    Dictionary<string, object> CustomData { get; set; }
    IServiceProvider? ServiceProvider { get; set; }
}
```

#### Properties

**HttpContext**
- **Type**: `HttpContext`
- **Description**: The current HTTP context for the request

**CustomData**
- **Type**: `Dictionary<string, object>`
- **Description**: Custom data that can be passed between pipeline steps

**ServiceProvider**
- **Type**: `IServiceProvider?`
- **Description**: Service provider for dependency injection

### IContentPipelineModel

Marker interface for all generated pipeline models.

```csharp
public interface IContentPipelineModel
{
    // Marker interface - no members
}
```

## Property Converters

### IContentPropertyConverter<TProperty, TValue>

Interface for custom property converters.

```csharp
public interface IContentPropertyConverter<in TProperty, out TValue>
{
    TValue GetValue(
        TProperty property,
        IContentData content,
        string propertyName,
        IContentPipelineContext pipelineContext,
        Dictionary<string, string>? config = null);
}
```

#### Type Parameters

- **TProperty**: The source property type from the content model
- **TValue**: The target type in the pipeline model

#### Methods

**GetValue**
- **Parameters**:
  - `property` (`TProperty`): The property value to convert
  - `content` (`IContentData`): The containing content object
  - `propertyName` (`string`): The name of the property being converted
  - `pipelineContext` (`IContentPipelineContext`): The pipeline context
  - `config` (`Dictionary<string, string>?`): Optional configuration parameters
- **Returns**: `TValue` - The converted value
- **Description**: Converts a property value from the source type to the target type

### Built-in Converters

#### XhtmlStringConverter

Converts `XhtmlString` to HTML string.

```csharp
public class XhtmlStringConverter : IContentPropertyConverter<XhtmlString?, string>
```

#### ContentReferenceConverter

Converts `ContentReference` to `Link` object.

```csharp
public class ContentReferenceConverter : IContentPropertyConverter<ContentReference?, Link?>
```

#### MediaConverter

Converts media `ContentReference` to `Media` object.

```csharp
public class MediaConverter : IContentPropertyConverter<ContentReference?, Media?>
```

#### ContentAreaConverter

Converts `ContentArea` to `ContentAreaPipelineModel`.

```csharp
public class ContentAreaConverter : IContentPropertyConverter<ContentArea?, ContentAreaPipelineModel?>
```

#### EnumConverter<T>

Converts enum values to strings.

```csharp
public class EnumConverter<T> : IContentPropertyConverter<T?, string>
    where T : struct, Enum
```

## Pipeline Steps

### IContentPipelineStep<TContent, TContentPipelineModel>

Interface for synchronous pipeline steps.

```csharp
public interface IContentPipelineStep<in TContent, in TContentPipelineModel>
    where TContent : IContentData
    where TContentPipelineModel : IContentPipelineModel
{
    int Order { get; }
    bool IsAsync { get; }
    
    void Execute(
        TContent content,
        TContentPipelineModel contentPipelineModel,
        IContentPipelineContext pipelineContext);
        
    Task ExecuteAsync(
        TContent content,
        TContentPipelineModel contentPipelineModel,
        IContentPipelineContext pipelineContext);
}
```

#### Properties

**Order**
- **Type**: `int`
- **Description**: Execution order (lower values execute first)

**IsAsync**
- **Type**: `bool`
- **Description**: Indicates if the step is asynchronous

#### Methods

**Execute**
- **Parameters**:
  - `content` (`TContent`): The source content
  - `contentPipelineModel` (`TContentPipelineModel`): The pipeline model being built
  - `pipelineContext` (`IContentPipelineContext`): The pipeline context
- **Description**: Synchronously executes the pipeline step

**ExecuteAsync**
- **Parameters**:
  - `content` (`TContent`): The source content
  - `contentPipelineModel` (`TContentPipelineModel`): The pipeline model being built
  - `pipelineContext` (`IContentPipelineContext`): The pipeline context
- **Returns**: `Task` - Completion task
- **Description**: Asynchronously executes the pipeline step

### AsyncContentPipelineStep<TContent, TContentPipelineModel>

Abstract base class for asynchronous pipeline steps.

```csharp
public abstract class AsyncContentPipelineStep<TContent, TContentPipelineModel> 
    : IContentPipelineStep<TContent, TContentPipelineModel>
    where TContent : IContentData
    where TContentPipelineModel : IContentPipelineModel
{
    protected AsyncContentPipelineStep(int order);
    
    public int Order { get; }
    public bool IsAsync => true;
    
    public void Execute(TContent content, TContentPipelineModel contentPipelineModel, IContentPipelineContext pipelineContext);
    public abstract Task ExecuteAsync(TContent content, TContentPipelineModel contentPipelineModel, IContentPipelineContext pipelineContext);
}
```

### IPostContentPipelineStep<TContent, TContentPipelineModel>

Interface for post-processing pipeline steps.

```csharp
public interface IPostContentPipelineStep<in TContent, in TContentPipelineModel>
    where TContent : IContentData
    where TContentPipelineModel : IContentPipelineModel
{
    int Order { get; }
    
    void Execute(
        TContent content,
        TContentPipelineModel contentPipelineModel,
        IContentPipelineContext pipelineContext);
}
```

## Generated Models

### Generated Pipeline Models

For each content type decorated with `[ContentPipelineModel]`, a corresponding pipeline model is generated:

```csharp
// Source content type
[ContentType(GUID = "...")]
[ContentPipelineModel("Pages")]
public class ArticlePage : PageData
{
    public virtual string? Title { get; set; }
    public virtual XhtmlString? Body { get; set; }
}

// Generated pipeline model
namespace ContentPipeline.Models.Pages
{
    public partial class ArticlePagePipelineModel : IContentPipelineModel
    {
        public string? Title { get; set; }
        public string? Body { get; set; }
    }
}
```

### Generated Properties

#### Link

Generated for `ContentReference` properties.

```csharp
public sealed partial class Link : ILinkPipelineModel
{
    public string? Url { get; set; }
    public string? Title { get; set; }
    public string? Target { get; set; }
}
```

#### Media

Generated for media `ContentReference` properties.

```csharp
public sealed partial class Media
{
    public string? Url { get; set; }
    public string? Type { get; set; }
    public string? AltText { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}
```

#### ContentAreaPipelineModel

Generated for `ContentArea` properties.

```csharp
public sealed partial class ContentAreaPipelineModel
{
    public IEnumerable<ContentAreaItemPipelineModel>? Items { get; set; }
}

public sealed partial class ContentAreaItemPipelineModel
{
    public string? DisplayOption { get; set; }
    public IContentPipelineModel? Content { get; set; }
}
```

## Attributes

### ContentPipelineModelAttribute

Marks a content type for pipeline model generation.

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class ContentPipelineModelAttribute : Attribute
{
    public ContentPipelineModelAttribute(string group);
    public ContentPipelineModelAttribute(string group, int order);
    
    public string Group { get; }
    public int Order { get; set; }
}
```

#### Constructor Parameters

- **group** (`string`): The group name for organizing generated models
- **order** (`int`, optional): Processing order within the group

#### Properties

**Group**
- **Type**: `string`
- **Description**: The group name for the generated model

**Order**
- **Type**: `int`
- **Description**: Processing order (default: 0)

### ContentPipelineIgnoreAttribute

Excludes a property from pipeline model generation.

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class ContentPipelineIgnoreAttribute : Attribute
{
}
```

### ContentPipelinePropertyConverterAttribute<T>

Specifies a custom property converter for a property.

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class ContentPipelinePropertyConverterAttribute<T> : Attribute
    where T : class
{
    public string? Config { get; set; }
}
```

#### Properties

**Config**
- **Type**: `string?`
- **Description**: Configuration string passed to the converter (format: "key1=value1;key2=value2")

## Service Collection Extensions

### ContentPipelineServiceCollectionExtensions

Extension methods for registering ContentPipeline services.

```csharp
public static class ContentPipelineServiceCollectionExtensions
{
    public static IServiceCollection AddContentPipelineServices(this IServiceCollection services);
}
```

#### Methods

**AddContentPipelineServices()**
- **Parameters**: `services` (`IServiceCollection`): The service collection
- **Returns**: `IServiceCollection` - The service collection for chaining
- **Description**: Registers all ContentPipeline services with default configuration







This API reference provides the complete interface for working with ContentPipeline. For implementation examples and advanced usage patterns, see the other documentation files.