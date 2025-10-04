# Architecture

This document provides an in-depth look at the ContentPipeline architecture, explaining how the source generator works and the core concepts that power the system.

## Overview

ContentPipeline is built around a source generator that analyzes your Optimizely CMS content models at compile time and generates corresponding pipeline models with customizable property transformations. The architecture consists of several key components:

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│  Content Model  │───▶│ Source Generator │───▶│ Pipeline Model  │
│  (PageData)     │    │                  │    │ (JSON-friendly) │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                              │
                              ▼
                    ┌──────────────────┐
                    │ Property         │
                    │ Converters       │
                    └──────────────────┘
```

## Core Components

### 1. Source Generator (`ContentPipelineSourceGenerator`)

The source generator is the heart of the system. It:

- **Discovers Content Models**: Scans for classes decorated with `[ContentType]` and `[ContentPipelineModel]` attributes
- **Analyzes Properties**: Examines each property and determines the appropriate transformation
- **Generates Code**: Creates pipeline models, interfaces, and service registrations at compile time
- **Handles Dependencies**: Manages relationships between different content types and their converters

#### Generator Phases

1. **Post-Initialization**: Creates base interfaces and shared types
2. **Analysis**: Discovers and parses content models 
3. **Code Generation**: Emits pipeline models, converters, and services
4. **Registration**: Generates service collection extensions

### 2. Content Models

Content models are your existing Optimizely CMS content types that you want to transform:

```csharp
[ContentType(GUID = "...")]
[ContentPipelineModel("Pages")]  // Group: "Pages"
public class ArticlePage : PageData
{
    public virtual string? Title { get; set; }
    public virtual XhtmlString? Body { get; set; }
    public virtual ContentReference? FeaturedImage { get; set; }
}
```

### 3. Pipeline Models

Generated models that are JSON-serialization friendly:

```csharp
namespace ContentPipeline.Models.Pages
{
    public partial class ArticlePagePipelineModel : IContentPipelineModel
    {
        public string? Title { get; set; }
        public string? Body { get; set; }  // XhtmlString → string
        public Media? FeaturedImage { get; set; }  // ContentReference → Media
    }
}
```

### 4. Property Converters

Transform properties from Optimizely types to pipeline-friendly types:

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

#### Built-in Converters

- **`XhtmlStringConverter`**: Converts `XhtmlString` to HTML string
- **`ContentReferenceConverter`**: Resolves `ContentReference` to `Link` objects
- **`MediaConverter`**: Transforms media references to `Media` objects with URLs
- **`ContentAreaConverter`**: Converts `ContentArea` to structured data
- **`EnumConverter<T>`**: Handles enum transformations

### 5. Pipeline Architecture

The system uses a pipeline pattern for processing content:

```csharp
┌─────────────┐    ┌──────────────┐    ┌─────────────┐    ┌──────────────┐
│ Raw Content │───▶│ Pipeline     │───▶│ Property    │───▶│ Final Model  │
│             │    │ Steps        │    │ Conversion  │    │              │
└─────────────┘    └──────────────┘    └─────────────┘    └──────────────┘
```

#### Pipeline Steps

Pipeline steps allow you to hook into the conversion process:

- **`IContentPipelineStep<TContent, TModel>`**: Synchronous processing
- **`AsyncContentPipelineStep<TContent, TModel>`**: Asynchronous processing
- **`IPostContentPipelineStep<TContent, TModel>`**: Post-processing hooks

### 6. Service Registration

The source generator automatically creates service registrations:

```csharp
public static class ContentPipelineServiceCollectionExtensions
{
    public static IServiceCollection AddContentPipelineServices(this IServiceCollection services)
    {
        return services
            .AddContentPipelineGeneratedSteps()
            .AddSingleton<IContentPipelineService, ContentPipelineService>()
            .AddSingleton<IXhtmlStringConverter, XhtmlStringConverter>()
            // ... other services
    }
}
```

## Code Generation Details

### File Structure

The source generator produces several types of files:

```
Generated/
├── Interfaces/
│   ├── IContentPipelineModel.g.cs
│   ├── IContentPipelineService.g.cs
│   └── ...
├── Models/
│   ├── {Group}/
│   │   └── {ContentType}PipelineModel.g.cs
│   └── ...
├── Properties/
│   ├── Link.g.cs
│   ├── Media.g.cs
│   └── ContentAreaPipelineModel.g.cs
├── Services/
│   └── ContentPipelineServiceRegistrations.g.cs
└── Pipelines/
    └── {Group}/
        └── Steps/
            └── {ContentType}PipelineStep.g.cs
```

### Compilation Context

The source generator operates within these constraints:

- **Target Framework**: .NET Standard 2.0 (required for source generators)
- **Language Version**: C# 11
- **Nullable Context**: Enabled (`#nullable enable`)
- **Dependencies**: Minimal external dependencies to avoid version conflicts

## Extension Points

### Custom Property Converters

Create converters for complex transformations:

```csharp
public class ComplexDataConverter : IContentPropertyConverter<ComplexType, SimpleModel>
{
    public SimpleModel GetValue(/* parameters */)
    {
        // Custom transformation logic
    }
}
```

### Pipeline Steps

Add custom processing logic:

```csharp
public class CustomPipelineStep : AsyncContentPipelineStep<MyContent, MyModel>
{
    public CustomPipelineStep() : base(order: 100) { }
    
    public override async Task ExecuteAsync(/* parameters */)
    {
        // Custom processing
    }
}
```

### Configuration Options

Configure the generator through MSBuild properties:

```xml
<PropertyGroup>
    <ContentPipeline_EnableForms>true</ContentPipeline_EnableForms>
</PropertyGroup>
```

## Performance Considerations

### Compile-Time Generation

- **Zero Runtime Overhead**: All code generation happens at compile time
- **Type Safety**: Full compile-time type checking
- **IntelliSense**: Complete IDE support for generated code

### Runtime Performance

- **Direct Property Access**: No reflection or dynamic dispatch
- **Minimal Allocations**: Efficient object creation patterns
- **Lazy Loading**: Content references resolved on-demand

## Debugging and Troubleshooting

### Generated Code Inspection

View generated code in:
```
obj/Debug/{framework}/generated/ContentPipeline/ContentPipeline.ContentPipelineSourceGenerator/
```

### Common Issues

1. **Missing Attributes**: Ensure both `[ContentType]` and `[ContentPipelineModel]` are present
2. **Namespace Conflicts**: Check for naming collisions in generated code
3. **Build Errors**: Verify source generator package is properly referenced

### Diagnostics

Enable source generator diagnostics:

```xml
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

## Best Practices

### Content Model Design

1. **Use Meaningful Groups**: Organize models with descriptive group names
2. **Property Naming**: Use clear, consistent property names
3. **Virtual Properties**: Ensure properties are virtual for Optimizely compatibility

### Converter Design

1. **Stateless Converters**: Keep converters stateless and thread-safe
2. **Error Handling**: Handle null values and edge cases gracefully
3. **Performance**: Optimize for the most common scenarios

### Pipeline Steps

1. **Ordering**: Use appropriate order values for step execution
2. **Async Operations**: Use async steps for I/O operations
3. **Error Recovery**: Implement appropriate error handling and logging

This architecture enables ContentPipeline to provide a flexible, performant, and type-safe way to transform Optimizely CMS content into JSON-friendly models while maintaining compile-time safety and runtime performance.