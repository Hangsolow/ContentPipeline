# Configuration

This document covers advanced configuration options and customization strategies for ContentPipeline.

## MSBuild Configuration

### Source Generator Options

Configure the source generator through MSBuild properties in your `.csproj` file:

```xml
<PropertyGroup>
    <!-- Enable Optimizely Forms support -->
    <ContentPipeline_EnableForms>true</ContentPipeline_EnableForms>
    
    <!-- Enable generated file output for debugging -->
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
    
    <!-- Enable source generator diagnostics -->
    <ReportAnalyzer>true</ReportAnalyzer>
</PropertyGroup>
```

### Available Properties

| Property | Default | Description |
|----------|---------|-------------|
| `ContentPipeline_EnableForms` | `false` | Enables support for Optimizely Forms content types |
| `EmitCompilerGeneratedFiles` | `false` | Outputs generated files to disk for inspection |
| `CompilerGeneratedFilesOutputPath` | `obj/Generated` | Directory for generated files |

## Service Configuration

### Basic Registration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Minimal setup
    services.AddContentPipelineServices();
}
```

### Advanced Registration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add ContentPipeline with custom configuration
    services.AddContentPipelineServices(options =>
    {
        options.DefaultGroup = "Common";
        options.EnableCaching = true;
        options.MaxCacheSize = 1000;
    });

    // Register custom converters
    services.AddSingleton<IContentPropertyConverter<DateTime?, string>, CustomDateConverter>();
    services.AddSingleton<IContentPropertyConverter<XhtmlString?, MarkdownContent>, XhtmlToMarkdownConverter>();

    // Register custom pipeline steps
    services.AddSingleton<IContentPipelineStep<ArticlePage, ArticlePagePipelineModel>, SeoEnrichmentStep>();
    services.AddSingleton<IPostContentPipelineStep<ProductPage, ProductPagePipelineModel>, PriceCalculationStep>();
}
```

## Content Model Configuration

### Attribute Configuration

#### ContentPipelineModel Attribute

```csharp
// Basic usage
[ContentPipelineModel("Pages")]

// With explicit order
[ContentPipelineModel("Pages", Order = 10)]

// Different group names for organization
[ContentPipelineModel("Marketing")]    // Marketing content
[ContentPipelineModel("Commerce")]     // Product pages
[ContentPipelineModel("Navigation")]   // Menu and navigation
```

#### Property-Level Configuration

```csharp
[ContentType(GUID = "...")]
[ContentPipelineModel("Pages")]
public class ArticlePage : PageData
{
    // Default mapping (string → string)
    public virtual string? Title { get; set; }

    // Custom converter with configuration
    [ContentPipelinePropertyConverter<RichTextConverter>]
    public virtual XhtmlString? Body { get; set; }

    // Ignore property completely
    [ContentPipelineIgnore]
    public virtual string? AdminNotes { get; set; }

    // Media file mapping
    [UIHint("MediaFile")]
    public virtual ContentReference? FeaturedImage { get; set; }

    // Content area mapping
    public virtual ContentArea? ContentBlocks { get; set; }
}
```

## Property Converter Configuration

### Built-in Converter Options

#### XhtmlString Converter

```csharp
public class CustomXhtmlConverter : IContentPropertyConverter<XhtmlString?, string>
{
    public string GetValue(
        XhtmlString? property, 
        IContentData content, 
        string propertyName,
        IContentPipelineContext pipelineContext, 
        Dictionary<string, string>? config = null)
    {
        // Check configuration
        var stripTags = config?.GetValueOrDefault("stripTags", "false") == "true";
        var maxLength = int.Parse(config?.GetValueOrDefault("maxLength", "0") ?? "0");

        var html = property?.ToHtmlString() ?? string.Empty;

        if (stripTags)
            html = StripHtmlTags(html);

        if (maxLength > 0 && html.Length > maxLength)
            html = html.Substring(0, maxLength) + "...";

        return html;
    }

    private string StripHtmlTags(string html) => 
        System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
}
```

Usage with configuration:

```csharp
[ContentPipelinePropertyConverter<CustomXhtmlConverter>(Config = "stripTags=true;maxLength=200")]
public virtual XhtmlString? Summary { get; set; }
```

### Complex Converter Examples

#### Multi-Language Content Converter

```csharp
public class MultiLanguageStringConverter : IContentPropertyConverter<string?, MultiLanguageString>
{
    private readonly IContentLoader _contentLoader;

    public MultiLanguageStringConverter(IContentLoader contentLoader)
    {
        _contentLoader = contentLoader;
    }

    public MultiLanguageString GetValue(
        string? property, 
        IContentData content, 
        string propertyName,
        IContentPipelineContext pipelineContext, 
        Dictionary<string, string>? config = null)
    {
        var result = new MultiLanguageString();

        if (content is ILocalizable localizable)
        {
            foreach (var language in localizable.ExistingLanguages)
            {
                var localized = _contentLoader.Get<IContentData>(
                    localizable.ContentLink, 
                    language);
                
                var propertyInfo = localized.GetType().GetProperty(propertyName);
                var value = propertyInfo?.GetValue(localized) as string;
                
                result.Translations[language.TwoLetterISOLanguageName] = value ?? string.Empty;
            }
        }

        return result;
    }
}

public class MultiLanguageString
{
    public Dictionary<string, string> Translations { get; set; } = new();
}
```

#### SEO Data Converter

```csharp
public class SeoDataConverter : IContentPropertyConverter<string?, SeoData>
{
    public SeoData GetValue(
        string? property, 
        IContentData content, 
        string propertyName,
        IContentPipelineContext pipelineContext, 
        Dictionary<string, string>? config = null)
    {
        var seoData = new SeoData
        {
            Title = property ?? string.Empty,
            MetaDescription = GenerateMetaDescription(content),
            OpenGraphTitle = property ?? string.Empty,
            TwitterTitle = property ?? string.Empty,
            CanonicalUrl = GenerateCanonicalUrl(content, pipelineContext),
            Keywords = ExtractKeywords(content)
        };

        return seoData;
    }

    private string GenerateMetaDescription(IContentData content)
    {
        // Logic to generate meta description from content
        return string.Empty;
    }

    private string GenerateCanonicalUrl(IContentData content, IContentPipelineContext context)
    {
        // Logic to generate canonical URL
        return string.Empty;
    }

    private string[] ExtractKeywords(IContentData content)
    {
        // Logic to extract keywords
        return Array.Empty<string>();
    }
}

public class SeoData
{
    public string Title { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string OpenGraphTitle { get; set; } = string.Empty;
    public string TwitterTitle { get; set; } = string.Empty;
    public string CanonicalUrl { get; set; } = string.Empty;
    public string[] Keywords { get; set; } = Array.Empty<string>();
}
```

## Pipeline Step Configuration

### Step Ordering

Pipeline steps execute in order. Use meaningful order values:

```csharp
// Early steps (0-99): Data validation, basic transformations
public class ValidationStep : ContentPipelineStep<MyContent, MyModel>
{
    public ValidationStep() : base(order: 10) { }
}

// Middle steps (100-199): Content enrichment
public class ContentEnrichmentStep : AsyncContentPipelineStep<MyContent, MyModel>
{
    public ContentEnrichmentStep() : base(order: 100) { }
}

// Late steps (200+): Finalization, caching
public class CacheWarmupStep : AsyncContentPipelineStep<MyContent, MyModel>
{
    public CacheWarmupStep() : base(order: 200) { }
}
```

### Conditional Pipeline Steps

```csharp
public class ConditionalStep : AsyncContentPipelineStep<ArticlePage, ArticlePagePipelineModel>
{
    private readonly IFeatureToggle _featureToggle;

    public ConditionalStep(IFeatureToggle featureToggle) : base(order: 150)
    {
        _featureToggle = featureToggle;
    }

    public override async Task ExecuteAsync(
        ArticlePage content, 
        ArticlePagePipelineModel model, 
        IContentPipelineContext context)
    {
        if (!_featureToggle.IsEnabled("advanced-seo"))
            return;

        // Advanced SEO processing
        await ProcessAdvancedSeo(content, model, context);
    }
}
```

### Error Handling in Pipeline Steps

```csharp
public class ResilientStep : AsyncContentPipelineStep<MyContent, MyModel>
{
    private readonly ILogger<ResilientStep> _logger;
    private readonly IConfiguration _config;

    public ResilientStep(ILogger<ResilientStep> logger, IConfiguration config) : base(order: 100)
    {
        _logger = logger;
        _config = config;
    }

    public override async Task ExecuteAsync(
        MyContent content, 
        MyModel model, 
        IContentPipelineContext context)
    {
        try
        {
            await ProcessContent(content, model, context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing content {ContentId}", content.ContentLink);
            
            // Decide whether to fail or continue based on configuration
            var failOnError = _config.GetValue<bool>("ContentPipeline:FailOnStepError");
            if (failOnError)
                throw;
                
            // Continue with default values
            SetDefaultValues(model);
        }
    }
}
```

## Context Configuration

### Custom Pipeline Context

```csharp
public class ExtendedPipelineContext : IContentPipelineContext
{
    public required HttpContext HttpContext { get; init; }
    public required IContentPipelineService ContentPipelineService { get; init; }
    public CultureInfo? Language { get; init; }
    
    // Additional custom properties
    public string ApiVersion { get; set; } = "v1";
    public bool IncludeDebugInfo { get; set; }
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public Dictionary<string, object> CustomData { get; set; } = new();
}

// Usage
public class ContentService
{
    public IContentPipelineModel ConvertContent(IContent content, HttpContext httpContext)
    {
        var context = new ExtendedPipelineContext
        {
            HttpContext = httpContext,
            ApiVersion = httpContext.Request.Headers["X-API-Version"].FirstOrDefault() ?? "v1",
            IncludeDebugInfo = httpContext.Request.Query.ContainsKey("debug"),
            CustomData = 
            {
                ["UserAgent"] = httpContext.Request.Headers["User-Agent"].ToString(),
                ["RequestTime"] = DateTime.UtcNow
            }
        };

        return _pipelineService.ExecutePipeline(content, context);
    }
}
```

## Performance Configuration

### Caching Configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configure memory cache for pipeline results
    services.AddMemoryCache(options =>
    {
        options.SizeLimit = 1000;
        options.CompactionPercentage = 0.25;
    });

    // Register caching pipeline step
    services.AddSingleton<ICachingStep, MemoryCachingStep>();
}

public class MemoryCachingStep : IPostContentPipelineStep<IContentData, IContentPipelineModel>
{
    private readonly IMemoryCache _cache;

    public MemoryCachingStep(IMemoryCache cache)
    {
        _cache = cache;
    }

    public int Order => 1000; // Run last

    public void Execute(
        IContentData content, 
        IContentPipelineModel contentPipelineModel, 
        IContentPipelineContext pipelineContext)
    {
        var cacheKey = $"pipeline_{content.ContentLink}_{content.Changed:yyyyMMddHHmmss}";
        
        _cache.Set(cacheKey, contentPipelineModel, new MemoryCacheEntryOptions
        {
            Size = 1,
            SlidingExpiration = TimeSpan.FromMinutes(15),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        });
    }
}
```

### Async Configuration

```csharp
// Configure async processing
public void ConfigureServices(IServiceCollection services)
{
    // Configure task scheduler for async operations
    services.Configure<TaskSchedulerOptions>(options =>
    {
        options.MaxConcurrency = Environment.ProcessorCount;
        options.QueueLimit = 1000;
    });

    // Register async-heavy steps
    services.AddSingleton<IAsyncImageProcessor, ImageProcessingStep>();
}
```

## Environment-Specific Configuration

### Development Configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    if (_environment.IsDevelopment())
    {
        // Add development-only pipeline steps
        services.AddSingleton<IContentPipelineStep<IContentData, IContentPipelineModel>, 
                              DebugValidationStep>();
    }
}
```

### Production Configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    if (_environment.IsProduction())
    {
        // Add production monitoring
        services.AddSingleton<IContentPipelineStep<IContentData, IContentPipelineModel>, 
                              MetricsCollectionStep>();
    }
}
```

## Integration Configuration

### API Integration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configure for API usage
    services.Configure<JsonOptions>(options =>
    {
        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.SerializerOptions.WriteIndented = false;
        options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

    // Add API-specific converters
    services.AddSingleton<IContentPropertyConverter<DateTime?, string>, ApiDateConverter>();
}

public class ApiDateConverter : IContentPropertyConverter<DateTime?, string>
{
    public string GetValue(
        DateTime? property, 
        IContentData content, 
        string propertyName,
        IContentPipelineContext pipelineContext, 
        Dictionary<string, string>? config = null)
    {
        return property?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? string.Empty;
    }
}
```

### Headless CMS Configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configure for headless scenarios
    services.AddContentPipelineServices(options =>
    {
        options.ResolveContentReferences = true;
        options.IncludeMetadata = true;
        options.ExpandContentAreas = true;
    });

    // Add headless-specific converters
    services.AddSingleton<IContentPropertyConverter<ContentReference?, HeadlessLink>, 
                          HeadlessLinkConverter>();
}
```

This configuration system allows you to customize ContentPipeline behavior to match your specific requirements and environment needs.