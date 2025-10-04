# Getting Started

This comprehensive guide will walk you through setting up and using ContentPipeline in your Optimizely CMS project.

## Prerequisites

- .NET 6.0 or later
- Optimizely CMS 12.x or later
- Visual Studio 2022 or VS Code with C# extension

## Installation

### 1. Install the NuGet Package

```bash
dotnet add package Hangsolow.ContentPipeline
```

Or via Package Manager Console in Visual Studio:

```powershell
Install-Package Hangsolow.ContentPipeline
```

### 2. Register Services

Add ContentPipeline services to your dependency injection container in `Program.cs`:

```csharp
using ContentPipeline.ServiceCollectionExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add Optimizely CMS services
builder.Services.AddCmsServices();

// Add ContentPipeline services
builder.Services.AddContentPipelineServices();

var app = builder.Build();

// Configure the request pipeline
app.UseRouting();
app.MapContent();

app.Run();
```

For older projects using `Startup.cs`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddCmsServices();
    services.AddContentPipelineServices();
}
```

## Basic Setup

### 1. Create Your First Content Model

Start by creating a simple page type with the required attributes:

```csharp
using EPiServer.Core;
using EPiServer.DataAnnotations;
using ContentPipeline.Attributes;

[ContentType(
    GUID = "A4C58B24-C6F4-4C85-B6A7-23D5E5A8C9B1",
    DisplayName = "Article Page",
    Description = "A page for articles and blog posts")]
[ContentPipelineModel("Pages")]  // Group name
public class ArticlePage : PageData
{
    [Display(Name = "Title", Order = 10)]
    public virtual string? Title { get; set; }

    [Display(Name = "Introduction", Order = 20)]
    public virtual string? Introduction { get; set; }

    [Display(Name = "Main Content", Order = 30)]
    public virtual XhtmlString? MainContent { get; set; }

    [Display(Name = "Featured Image", Order = 40)]
    [UIHint("MediaFile")]
    public virtual ContentReference? FeaturedImage { get; set; }
}
```

### 2. Build Your Project

After adding the attributes, build your project. The source generator will automatically create:

- Pipeline model: `ArticlePagePipelineModel`
- Service registrations
- Property converters

You can find the generated files in your project's `obj` folder under:
```
obj/Debug/net6.0/generated/ContentPipeline/ContentPipeline.ContentPipelineSourceGenerator/
```

### 3. Use the Pipeline Service

Create a service to convert your content:

```csharp
using ContentPipeline.Interfaces;
using EPiServer.Core;
using Microsoft.AspNetCore.Http;

public class ArticleService
{
    private readonly IContentPipelineService _pipelineService;

    public ArticleService(IContentPipelineService pipelineService)
    {
        _pipelineService = pipelineService;
    }

    public async Task<ArticleData?> GetArticleAsync(int contentId, HttpContext httpContext)
    {
        var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
        
        if (!contentLoader.TryGet<ArticlePage>(new ContentReference(contentId), out var article))
            return null;

        var context = new ContentPipelineContext { HttpContext = httpContext };
        var pipelineModel = _pipelineService.ExecutePipeline(article, context);

        return new ArticleData
        {
            Id = article.ContentLink.ID,
            Title = pipelineModel.Title,
            Introduction = pipelineModel.Introduction,
            Content = pipelineModel.MainContent,
            FeaturedImageUrl = pipelineModel.FeaturedImage?.Url
        };
    }
}

public class ArticleData
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Introduction { get; set; }
    public string? Content { get; set; }
    public string? FeaturedImageUrl { get; set; }
}
```

### 4. Create a Controller

Expose your content through a Web API controller:

```csharp
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly ArticleService _articleService;

    public ArticlesController(ArticleService articleService)
    {
        _articleService = articleService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ArticleData>> GetArticle(int id)
    {
        var article = await _articleService.GetArticleAsync(id, HttpContext);
        
        if (article == null)
            return NotFound();

        return Ok(article);
    }
}
```

Don't forget to register your service:

```csharp
builder.Services.AddScoped<ArticleService>();
```

## Working with Different Content Types

### Block Types

Create reusable block types:

```csharp
[ContentType(
    GUID = "B5D9AC35-D7F5-4D96-C7B8-34E6F6B9C0C2",
    DisplayName = "Quote Block")]
[ContentPipelineModel("Blocks")]
public class QuoteBlock : BlockData
{
    public virtual string? Quote { get; set; }
    public virtual string? Author { get; set; }
    public virtual string? Source { get; set; }
}
```

### Media Types

Handle media content:

```csharp
[ContentType(
    GUID = "C6E0BD46-E8G6-5E07-D8C9-45F7G7C0D1D3",
    DisplayName = "Image Media")]
[ContentPipelineModel("Media")]
public class ImageMedia : ImageData
{
    public virtual string? AltText { get; set; }
    public virtual string? Caption { get; set; }
    public virtual string? Copyright { get; set; }
}
```

## Property Transformations

### Default Conversions

ContentPipeline automatically handles common Optimizely types:

| Source Type | Target Type | Description |
|-------------|-------------|-------------|
| `string` | `string` | Direct mapping |
| `XhtmlString` | `string` | Rendered HTML |
| `ContentReference` | `Link` | URL and metadata |
| `ContentReference` (Media) | `Media` | Media with URL and type |
| `ContentArea` | `ContentAreaPipelineModel` | Structured content area |
| Enums | `string` | Enum value as string |

### Ignoring Properties

Exclude properties from the pipeline model:

```csharp
[ContentType(GUID = "...")]
[ContentPipelineModel("Pages")]
public class ArticlePage : PageData
{
    public virtual string? Title { get; set; }

    [ContentPipelineIgnore]
    public virtual string? InternalNotes { get; set; }  // Excluded

    [ContentPipelineIgnore]
    public virtual DateTime? InternalModified { get; set; }  // Excluded
}
```

## Testing Your Setup

### 1. Create a Test Content Item

In Optimizely CMS admin:

1. Go to **Edit** mode
2. Create a new **Article Page**
3. Fill in the content
4. Publish the page

### 2. Test the API

Use a tool like Postman or curl to test your endpoint:

```bash
curl https://localhost:5001/api/articles/123
```

Expected response:
```json
{
  "id": 123,
  "title": "My First Article",
  "introduction": "This is an introduction",
  "content": "<p>Main content here</p>",
  "featuredImageUrl": "https://example.com/images/featured.jpg"
}
```

## Advanced Configuration

### Custom Property Converters

For complex transformations, create custom converters:

```csharp
public class CustomDateConverter : IContentPropertyConverter<DateTime?, string>
{
    public string GetValue(
        DateTime? property, 
        IContentData content, 
        string propertyName,
        IContentPipelineContext pipelineContext, 
        Dictionary<string, string>? config = null)
    {
        return property?.ToString("yyyy-MM-dd") ?? string.Empty;
    }
}

// Register the converter
builder.Services.AddSingleton<CustomDateConverter>();

// Use in your content model
[ContentPipelinePropertyConverter<CustomDateConverter>]
public virtual DateTime? PublishDate { get; set; }
```

### Pipeline Steps

Add custom processing logic:

```csharp
public class SeoEnrichmentStep : AsyncContentPipelineStep<ArticlePage, ArticlePagePipelineModel>
{
    public SeoEnrichmentStep() : base(order: 100) { }

    public override async Task ExecuteAsync(
        ArticlePage content, 
        ArticlePagePipelineModel contentPipelineModel, 
        IContentPipelineContext pipelineContext)
    {
        // Add SEO metadata, social media tags, etc.
        contentPipelineModel.SeoTitle = GenerateSeoTitle(content.Title);
        contentPipelineModel.MetaDescription = GenerateMetaDescription(content.Introduction);
    }

    private string GenerateSeoTitle(string? title) => $"{title} | My Site";
    private string GenerateMetaDescription(string? intro) => intro?.Substring(0, Math.Min(160, intro.Length ?? 0)) ?? "";
}

// Register the step
builder.Services.AddSingleton<IContentPipelineStep<ArticlePage, ArticlePagePipelineModel>, SeoEnrichmentStep>();
```

## Troubleshooting

### Common Issues

1. **Generated code not appearing**: Ensure you've built the project after adding attributes
2. **Missing pipeline model**: Check that both `[ContentType]` and `[ContentPipelineModel]` attributes are present
3. **Build errors**: Verify the source generator package is properly installed

### Viewing Generated Code

To see what code is being generated:

1. Add to your `.csproj` file:
```xml
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

2. Rebuild the project
3. Check the `Generated` folder in your project root

## Next Steps

Now that you have the basics working:

1. **[Architecture](architecture.md)** - Understand how the system works
2. **[Configuration](configuration.md)** - Advanced configuration options
3. **[Advanced Usage](advanced-usage.md)** - Complex scenarios and patterns
4. **[Testing](testing.md)** - Testing strategies for your pipeline models
5. **[API Reference](api-reference.md)** - Complete API documentation

## Sample Project

For a complete working example, check out the sample project in the repository that demonstrates all the concepts covered in this guide.