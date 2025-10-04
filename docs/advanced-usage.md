# Advanced Usage

This document covers advanced scenarios, patterns, and best practices for using ContentPipeline in complex applications.

## Advanced Property Converters

### Multi-Stage Converters

Create converters that perform complex, multi-step transformations:

```csharp
public class RichContentConverter : IContentPropertyConverter<XhtmlString?, RichContent>
{
    private readonly IContentLoader _contentLoader;
    private readonly IImageProcessor _imageProcessor;
    private readonly ILinkProcessor _linkProcessor;

    public RichContentConverter(
        IContentLoader contentLoader,
        IImageProcessor imageProcessor,
        ILinkProcessor linkProcessor)
    {
        _contentLoader = contentLoader;
        _imageProcessor = imageProcessor;
        _linkProcessor = linkProcessor;
    }

    public RichContent GetValue(
        XhtmlString? property,
        IContentData content,
        string propertyName,
        IContentPipelineContext pipelineContext,
        Dictionary<string, string>? config = null)
    {
        if (property?.IsEmpty != false)
            return new RichContent();

        var html = property.ToHtmlString();
        
        return new RichContent
        {
            Html = html,
            PlainText = ExtractPlainText(html),
            Images = ExtractAndProcessImages(html, pipelineContext),
            Links = ExtractAndProcessLinks(html, pipelineContext),
            Metadata = ExtractMetadata(html),
            WordCount = CountWords(html),
            ReadingTime = CalculateReadingTime(html),
            Headings = ExtractHeadings(html),
            TableOfContents = GenerateTableOfContents(html)
        };
    }

    private List<ProcessedImage> ExtractAndProcessImages(string html, IContentPipelineContext context)
    {
        var images = new List<ProcessedImage>();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        foreach (var img in doc.DocumentNode.Descendants("img"))
        {
            var src = img.GetAttributeValue("src", "");
            var alt = img.GetAttributeValue("alt", "");
            
            if (ContentReference.TryParse(src, out var contentRef))
            {
                if (_contentLoader.TryGet<ImageData>(contentRef, out var imageData))
                {
                    images.Add(new ProcessedImage
                    {
                        Url = _imageProcessor.GetOptimizedUrl(imageData, context.HttpContext),
                        AltText = alt,
                        Width = imageData.BinaryData?.Width ?? 0,
                        Height = imageData.BinaryData?.Height ?? 0,
                        ResponsiveUrls = _imageProcessor.GenerateResponsiveUrls(imageData, context.HttpContext)
                    });
                }
            }
        }

        return images;
    }

    private List<ProcessedLink> ExtractAndProcessLinks(string html, IContentPipelineContext context)
    {
        // Similar implementation for processing links
        return new List<ProcessedLink>();
    }

    // Additional helper methods...
}

public class RichContent
{
    public string Html { get; set; } = string.Empty;
    public string PlainText { get; set; } = string.Empty;
    public List<ProcessedImage> Images { get; set; } = new();
    public List<ProcessedLink> Links { get; set; } = new();
    public ContentMetadata Metadata { get; set; } = new();
    public int WordCount { get; set; }
    public TimeSpan ReadingTime { get; set; }
    public List<Heading> Headings { get; set; } = new();
    public TableOfContents TableOfContents { get; set; } = new();
}
```

### Conditional Converters

Create converters that behave differently based on context or configuration:

```csharp
public class ContextualImageConverter : IContentPropertyConverter<ContentReference?, object>
{
    private readonly IContentLoader _contentLoader;
    private readonly IUrlResolver _urlResolver;

    public ContextualImageConverter(IContentLoader contentLoader, IUrlResolver urlResolver)
    {
        _contentLoader = contentLoader;
        _urlResolver = urlResolver;
    }

    public object GetValue(
        ContentReference? property,
        IContentData content,
        string propertyName,
        IContentPipelineContext pipelineContext,
        Dictionary<string, string>? config = null)
    {
        if (property?.IsNullOrEmpty() != false)
            return GetNullValue(config);

        if (!_contentLoader.TryGet<ImageData>(property, out var imageData))
            return GetNullValue(config);

        var apiVersion = config?.GetValueOrDefault("apiVersion", "v1");
        var includeMetadata = bool.Parse(config?.GetValueOrDefault("includeMetadata", "false"));
        var format = config?.GetValueOrDefault("format", "standard");

        return apiVersion switch
        {
            "v1" => CreateV1Response(imageData, includeMetadata, pipelineContext),
            "v2" => CreateV2Response(imageData, includeMetadata, format, pipelineContext),
            "graphql" => CreateGraphQLResponse(imageData, pipelineContext),
            _ => CreateStandardResponse(imageData, pipelineContext)
        };
    }

    private object CreateV1Response(ImageData image, bool includeMetadata, IContentPipelineContext context)
    {
        var response = new { Url = _urlResolver.GetUrl(image.ContentLink) };
        
        if (includeMetadata)
        {
            return new
            {
                response.Url,
                AltText = image.AltText,
                Width = image.BinaryData?.Width,
                Height = image.BinaryData?.Height
            };
        }

        return response;
    }

    private object CreateV2Response(ImageData image, bool includeMetadata, string format, IContentPipelineContext context)
    {
        // Enhanced v2 format with more features
        return new ImageV2
        {
            Id = image.ContentLink.ID,
            Url = _urlResolver.GetUrl(image.ContentLink),
            Metadata = includeMetadata ? CreateMetadata(image) : null,
            Transformations = CreateTransformations(image, format, context),
            ResponsiveImages = CreateResponsiveImages(image, context)
        };
    }

    private object GetNullValue(Dictionary<string, string>? config)
    {
        var returnType = config?.GetValueOrDefault("nullBehavior", "null");
        return returnType switch
        {
            "empty" => new { },
            "placeholder" => new { Url = "/images/placeholder.jpg" },
            _ => null
        };
    }
}
```

### Async Property Converters

For converters that need to perform async operations:

```csharp
public class AsyncExternalApiConverter : IContentPropertyConverter<string?, ExternalApiData>
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AsyncExternalApiConverter> _logger;

    public AsyncExternalApiConverter(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<AsyncExternalApiConverter> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public ExternalApiData GetValue(
        string? property,
        IContentData content,
        string propertyName,
        IContentPipelineContext pipelineContext,
        Dictionary<string, string>? config = null)
    {
        if (string.IsNullOrEmpty(property))
            return new ExternalApiData();

        // Note: Converters must be synchronous per the IContentPropertyConverter interface
        // For async operations, consider using pipeline steps instead
        return GetValueSync(property, config);
    }

    private async Task<ExternalApiData> GetValueAsync(string identifier, Dictionary<string, string>? config)
    {
        var cacheKey = $"external_api_{identifier}";
        
        if (_cache.TryGetValue(cacheKey, out ExternalApiData cachedData))
            return cachedData;

        try
        {
            var apiUrl = config?.GetValueOrDefault("apiUrl", "https://api.example.com");
            var response = await _httpClient.GetAsync($"{apiUrl}/data/{identifier}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<ExternalApiData>(json);
                
                _cache.Set(cacheKey, data, TimeSpan.FromMinutes(15));
                return data;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch external data for {Identifier}", identifier);
        }

        return new ExternalApiData { Error = "Failed to fetch data" };
    }

    private ExternalApiData GetValueSync(string identifier, Dictionary<string, string>? config)
    {
        // Simplified synchronous version
        return new ExternalApiData { Id = identifier, Source = "cache" };
    }
}
```

## Advanced Pipeline Steps

### Conditional Pipeline Steps

Create pipeline steps that execute based on specific conditions:

```csharp
public class ConditionalAnalyticsStep : AsyncContentPipelineStep<IContentData, IContentPipelineModel>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IFeatureToggle _featureToggle;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConditionalAnalyticsStep> _logger;

    public ConditionalAnalyticsStep(
        IAnalyticsService analyticsService,
        IFeatureToggle featureToggle,
        IConfiguration configuration,
        ILogger<ConditionalAnalyticsStep> logger) : base(order: 200)
    {
        _analyticsService = analyticsService;
        _featureToggle = featureToggle;
        _configuration = configuration;
        _logger = logger;
    }

    public override async Task ExecuteAsync(
        IContentData content,
        IContentPipelineModel contentPipelineModel,
        IContentPipelineContext pipelineContext)
    {
        // Check if analytics is enabled
        if (!_featureToggle.IsEnabled("analytics-enrichment"))
            return;

        // Check content type eligibility
        if (!IsEligibleContent(content))
            return;

        // Check user consent (GDPR compliance)
        if (!HasAnalyticsConsent(pipelineContext))
            return;

        try
        {
            var analyticsData = await _analyticsService.GetContentMetricsAsync(content.ContentLink);
            
            if (contentPipelineModel is IAnalyticsEnhanced enhanced)
            {
                enhanced.Analytics = new AnalyticsData
                {
                    PageViews = analyticsData.PageViews,
                    UniqueVisitors = analyticsData.UniqueVisitors,
                    AverageTimeOnPage = analyticsData.AverageTimeOnPage,
                    BounceRate = analyticsData.BounceRate,
                    LastUpdated = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail the pipeline
            // Get logger from DI through constructor injection instead
            _logger?.LogWarning(ex, "Failed to enrich content {ContentId} with analytics data", content.ContentLink);
        }
    }

    private bool IsEligibleContent(IContentData content)
    {
        var eligibleTypes = _configuration.GetSection("Analytics:EligibleContentTypes").Get<string[]>();
        return eligibleTypes?.Contains(content.GetType().Name) ?? false;
    }

    private bool HasAnalyticsConsent(IContentPipelineContext context)
    {
        return context.HttpContext?.Request.Cookies.ContainsKey("analytics-consent") ?? false;
    }
}
```

### Bulk Processing Pipeline Steps

For handling multiple content items efficiently:

```csharp
public class BulkImageOptimizationStep : AsyncContentPipelineStep<IContentData, IContentPipelineModel>
{
    private readonly IBulkImageProcessor _imageProcessor;
    private readonly SemaphoreSlim _semaphore;
    private static readonly ConcurrentQueue<ImageOptimizationRequest> _optimizationQueue = new();
    private static readonly Timer _batchTimer;

    static BulkImageOptimizationStep()
    {
        _batchTimer = new Timer(ProcessBatch, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public BulkImageOptimizationStep(IBulkImageProcessor imageProcessor) : base(order: 300)
    {
        _imageProcessor = imageProcessor;
        _semaphore = new SemaphoreSlim(1, 1);
    }

    public override async Task ExecuteAsync(
        IContentData content,
        IContentPipelineModel contentPipelineModel,
        IContentPipelineContext pipelineContext)
    {
        if (contentPipelineModel is IImageContainer imageContainer && imageContainer.Images?.Any() == true)
        {
            foreach (var image in imageContainer.Images)
            {
                var request = new ImageOptimizationRequest
                {
                    ContentId = content.ContentLink.ID,
                    ImageUrl = image.Url,
                    RequiredSizes = GetRequiredSizes(pipelineContext),
                    CompletionSource = new TaskCompletionSource<OptimizedImage>(),
                    RequestedAt = DateTime.UtcNow
                };

                _optimizationQueue.Enqueue(request);
                
                // Await the result (will be processed in batch)
                var optimizedImage = await request.CompletionSource.Task;
                image.OptimizedUrls = optimizedImage.Urls;
                image.WebPUrl = optimizedImage.WebPUrl;
                image.AvifUrl = optimizedImage.AvifUrl;
            }
        }
    }

    private static async void ProcessBatch(object? state)
    {
        var requests = new List<ImageOptimizationRequest>();
        
        // Collect all pending requests
        while (_optimizationQueue.TryDequeue(out var request))
        {
            requests.Add(request);
        }

        if (!requests.Any())
            return;

        try
        {
            var processor = ServiceLocator.Current.GetInstance<IBulkImageProcessor>();
            var results = await processor.OptimizeBatchAsync(requests.Select(r => r.ImageUrl));

            // Complete all requests
            for (int i = 0; i < requests.Count && i < results.Count; i++)
            {
                requests[i].CompletionSource.SetResult(results[i]);
            }
        }
        catch (Exception ex)
        {
            // Complete all requests with error
            foreach (var request in requests)
            {
                request.CompletionSource.SetException(ex);
            }
        }
    }

    private string[] GetRequiredSizes(IContentPipelineContext context)
    {
        var userAgent = context.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "";
        var isMobile = userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase);
        
        return isMobile 
            ? new[] { "320w", "480w", "640w" }
            : new[] { "480w", "768w", "1024w", "1440w", "1920w" };
    }
}

public class ImageOptimizationRequest
{
    public int ContentId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string[] RequiredSizes { get; set; } = Array.Empty<string>();
    public TaskCompletionSource<OptimizedImage> CompletionSource { get; set; } = new();
    public DateTime RequestedAt { get; set; }
}
```

## Complex Content Relationships

### Hierarchical Content Processing

Handle complex parent-child relationships by extending the context:

```csharp
// Custom context to track hierarchy depth
public class HierarchicalPipelineContext : IContentPipelineContext
{
    private readonly IContentPipelineContext _baseContext;
    
    public HierarchicalPipelineContext(IContentPipelineContext baseContext, int depth = 0, string path = "")
    {
        _baseContext = baseContext;
        HierarchyDepth = depth;
        ContentPath = path;
    }
    
    public HttpContext HttpContext => _baseContext.HttpContext;
    public IContentPipelineService ContentPipelineService => _baseContext.ContentPipelineService;
    public CultureInfo? Language => _baseContext.Language;
    
    public int HierarchyDepth { get; }
    public string ContentPath { get; }
}

public class HierarchicalContentConverter : IContentPropertyConverter<ContentArea?, HierarchicalContent>
{
    private readonly IContentLoader _contentLoader;
    private readonly IContentPipelineService _pipelineService;
    private readonly int _maxDepth;

    public HierarchicalContentConverter(
        IContentLoader contentLoader,
        IContentPipelineService pipelineService,
        IConfiguration configuration)
    {
        _contentLoader = contentLoader;
        _pipelineService = pipelineService;
        _maxDepth = configuration.GetValue<int>("ContentPipeline:MaxHierarchyDepth", 3);
    }

    public HierarchicalContent GetValue(
        ContentArea? property,
        IContentData content,
        string propertyName,
        IContentPipelineContext pipelineContext,
        Dictionary<string, string>? config = null)
    {
        if (property?.IsEmpty != false)
            return new HierarchicalContent();

        var currentDepth = GetCurrentDepth(pipelineContext);
        if (currentDepth >= _maxDepth)
        {
            return new HierarchicalContent { TruncatedDueToDepth = true };
        }

        var items = new List<HierarchicalContentItem>();
        
        foreach (var item in property.Items)
        {
            if (_contentLoader.TryGet<IContentData>(item.ContentLink, out var childContent))
            {
                var childContext = CreateChildContext(pipelineContext, currentDepth + 1, childContent);
                var childModel = _pipelineService.ExecutePipeline(childContent, childContext);
                
                items.Add(new HierarchicalContentItem
                {
                    Content = childModel,
                    DisplayOption = item.RenderSettings?.DisplayOption,
                    Depth = currentDepth + 1,
                    Path = BuildPath(pipelineContext, childContent),
                    Metadata = ExtractMetadata(childContent, item)
                });
            }
        }

        return new HierarchicalContent
        {
            Items = items,
            TotalCount = items.Count,
            MaxDepthReached = currentDepth == _maxDepth - 1
        };
    }

    private int GetCurrentDepth(IContentPipelineContext context)
    {
        return context is HierarchicalPipelineContext hierarchical ? hierarchical.HierarchyDepth : 0;
    }

    private IContentPipelineContext CreateChildContext(IContentPipelineContext parent, int newDepth, IContentData childContent)
    {
        var parentPath = parent is HierarchicalPipelineContext hierarchical ? hierarchical.ContentPath : "";
        var contentName = childContent.Name ?? childContent.ContentLink.ID.ToString();
        var newPath = string.IsNullOrEmpty(parentPath) ? contentName : $"{parentPath}/{contentName}";
        
        return new HierarchicalPipelineContext(parent, newDepth, newPath);
    }

    private string BuildPath(IContentPipelineContext context, IContentData content)
    {
        if (context is HierarchicalPipelineContext hierarchical)
            return hierarchical.ContentPath;
            
        return content.Name ?? content.ContentLink.ID.ToString();
    }
}
```

### Cross-Reference Resolution

Resolve complex content relationships:

```csharp
public class CrossReferenceResolver : IPostContentPipelineStep<IContentData, IContentPipelineModel>
{
    private readonly IContentLoader _contentLoader;
    private readonly IContentRepository _contentRepository;
    private readonly IContentPipelineService _pipelineService;

    public int Order => 500; // Run after other steps

    public CrossReferenceResolver(
        IContentLoader contentLoader,
        IContentRepository contentRepository,
        IContentPipelineService pipelineService)
    {
        _contentLoader = contentLoader;
        _contentRepository = contentRepository;
        _pipelineService = pipelineService;
    }

    public void Execute(
        IContentData content,
        IContentPipelineModel contentPipelineModel,
        IContentPipelineContext pipelineContext)
    {
        if (contentPipelineModel is ICrossReferenceable crossReferenceable)
        {
            ResolveCrossReferences(content, crossReferenceable, pipelineContext);
        }
    }

    private void ResolveCrossReferences(
        IContentData content,
        ICrossReferenceable model,
        IContentPipelineContext context)
    {
        // Resolve incoming references (what content links to this)
        model.IncomingReferences = GetIncomingReferences(content, context);
        
        // Resolve related content (same tags, categories, etc.)
        model.RelatedContent = GetRelatedContent(content, context);
        
        // Resolve content hierarchy
        model.Breadcrumb = BuildBreadcrumb(content, context);
        
        // Resolve next/previous in sequence
        if (content is ISequentialContent)
        {
            var (previous, next) = GetSequentialReferences(content, context);
            model.PreviousContent = previous;
            model.NextContent = next;
        }
    }

    private List<ContentReference> GetIncomingReferences(IContentData content, IContentPipelineContext context)
    {
        var references = _contentRepository.GetReferencesToContent(content.ContentLink, false);
        return references.Take(10).ToList(); // Limit to prevent performance issues
    }

    private List<IContentPipelineModel> GetRelatedContent(IContentData content, IContentPipelineContext context)
    {
        if (content is ICategorizable categorizable && categorizable.Category?.Any() == true)
        {
            var relatedContent = FindContentByCategories(categorizable.Category, content.ContentLink);
            return ConvertToModels(relatedContent, context);
        }

        return new List<IContentPipelineModel>();
    }

    private List<IContentData> FindContentByCategories(CategoryList categories, ContentReference exclude)
    {
        // Implementation to find content with similar categories
        return new List<IContentData>();
    }

    private List<IContentPipelineModel> ConvertToModels(
        IEnumerable<IContentData> content,
        IContentPipelineContext context)
    {
        return content
            .Take(5) // Limit related content
            .Select(c => _pipelineService.ExecutePipeline(c, context))
            .Where(m => m != null)
            .ToList();
    }
}

public interface ICrossReferenceable
{
    List<ContentReference> IncomingReferences { get; set; }
    List<IContentPipelineModel> RelatedContent { get; set; }
    List<BreadcrumbItem> Breadcrumb { get; set; }
    IContentPipelineModel? PreviousContent { get; set; }
    IContentPipelineModel? NextContent { get; set; }
}
```

## Performance Optimization Patterns

### Caching Strategies

Implement intelligent caching for pipeline results:

```csharp
public class IntelligentCachingStep : IPostContentPipelineStep<IContentData, IContentPipelineModel>
{
    private readonly IDistributedCache _cache;
    private readonly IContentEvents _contentEvents;
    private readonly ILogger<IntelligentCachingStep> _logger;
    private readonly ConcurrentDictionary<ContentReference, DateTime> _lastModified = new();

    public int Order => 1000; // Run last

    public IntelligentCachingStep(
        IDistributedCache cache,
        IContentEvents contentEvents,
        ILogger<IntelligentCachingStep> logger)
    {
        _cache = cache;
        _contentEvents = contentEvents;
        _logger = logger;
        
        // Subscribe to content events for cache invalidation
        _contentEvents.PublishedContent += OnContentPublished;
        _contentEvents.DeletedContent += OnContentDeleted;
    }

    public void Execute(
        IContentData content,
        IContentPipelineModel contentPipelineModel,
        IContentPipelineContext pipelineContext)
    {
        var cacheStrategy = DetermineCacheStrategy(content, pipelineContext);
        
        if (cacheStrategy.ShouldCache)
        {
            var cacheKey = GenerateCacheKey(content, pipelineContext, cacheStrategy);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = cacheStrategy.SlidingExpiration,
                AbsoluteExpirationRelativeToNow = cacheStrategy.AbsoluteExpiration
            };

            var serializedModel = JsonSerializer.Serialize(contentPipelineModel);
            var cacheData = new CachedPipelineModel
            {
                Model = serializedModel,
                ContentType = contentPipelineModel.GetType().AssemblyQualifiedName,
                CachedAt = DateTime.UtcNow,
                ContentVersion = content.ContentGuid,
                Dependencies = ExtractDependencies(contentPipelineModel)
            };

            _cache.SetAsync(cacheKey, JsonSerializer.SerializeToUtf8Bytes(cacheData), cacheOptions);
            _lastModified[content.ContentLink] = DateTime.UtcNow;
        }
    }

    private CacheStrategy DetermineCacheStrategy(IContentData content, IContentPipelineContext context)
    {
        // Dynamic caching strategy based on content type and usage patterns
        var contentType = content.GetType();
        var isHighTraffic = IsHighTrafficContent(content);
        var hasPersonalization = HasPersonalization(context);
        var isRealTimeData = ContainsRealTimeData(content);

        return new CacheStrategy
        {
            ShouldCache = !hasPersonalization && !isRealTimeData,
            SlidingExpiration = isHighTraffic ? TimeSpan.FromMinutes(30) : TimeSpan.FromHours(2),
            AbsoluteExpiration = isHighTraffic ? TimeSpan.FromHours(6) : TimeSpan.FromDays(1),
            InvalidationTags = GenerateInvalidationTags(content)
        };
    }

    private void OnContentPublished(object? sender, ContentEventArgs e)
    {
        // Invalidate cache for the published content and its dependencies
        InvalidateContentCache(e.Content.ContentLink);
        InvalidateDependentCache(e.Content.ContentLink);
    }

    private async void InvalidateContentCache(ContentReference contentLink)
    {
        var cachePattern = $"pipeline_{contentLink.ID}_*";
        await InvalidateCachePattern(cachePattern);
    }
}

public class CacheStrategy
{
    public bool ShouldCache { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }
    public TimeSpan? AbsoluteExpiration { get; set; }
    public List<string> InvalidationTags { get; set; } = new();
}
```

### Lazy Loading Implementation

Implement lazy loading for expensive operations:

```csharp
public class LazyContentConverter : IContentPropertyConverter<ContentArea?, LazyContentCollection>
{
    private readonly IContentLoader _contentLoader;
    private readonly IContentPipelineService _pipelineService;

    public LazyContentConverter(IContentLoader contentLoader, IContentPipelineService pipelineService)
    {
        _contentLoader = contentLoader;
        _pipelineService = pipelineService;
    }

    public LazyContentCollection GetValue(
        ContentArea? property,
        IContentData content,
        string propertyName,
        IContentPipelineContext pipelineContext,
        Dictionary<string, string>? config = null)
    {
        if (property?.IsEmpty != false)
            return new LazyContentCollection();

        return new LazyContentCollection
        {
            ContentArea = property,
            ContentLoader = _contentLoader,
            PipelineService = _pipelineService,
            Context = pipelineContext,
            LoadBehavior = DetermineLazyLoadBehavior(config)
        };
    }

    private LazyLoadBehavior DetermineLazyLoadBehavior(Dictionary<string, string>? config)
    {
        var behavior = config?.GetValueOrDefault("lazyLoad", "auto");
        return behavior switch
        {
            "immediate" => LazyLoadBehavior.Immediate,
            "lazy" => LazyLoadBehavior.Lazy,
            "viewport" => LazyLoadBehavior.OnViewport,
            _ => LazyLoadBehavior.Auto
        };
    }
}

public class LazyContentCollection : IEnumerable<IContentPipelineModel>
{
    internal ContentArea? ContentArea { get; set; }
    internal IContentLoader? ContentLoader { get; set; }
    internal IContentPipelineService? PipelineService { get; set; }
    internal IContentPipelineContext? Context { get; set; }
    internal LazyLoadBehavior LoadBehavior { get; set; }

    private readonly Lazy<List<IContentPipelineModel>> _lazyItems;

    public LazyContentCollection()
    {
        _lazyItems = new Lazy<List<IContentPipelineModel>>(LoadItems);
    }

    public int Count => ContentArea?.Items.Count() ?? 0;
    
    public bool IsLoaded => _lazyItems.IsValueCreated;

    public IEnumerable<IContentPipelineModel> Items => _lazyItems.Value;

    public async Task<IEnumerable<IContentPipelineModel>> LoadAsync()
    {
        return await Task.Run(() => _lazyItems.Value);
    }

    public IContentPipelineModel? GetItem(int index)
    {
        if (ContentArea?.Items.Count() <= index)
            return null;

        // Load only the specific item
        var item = ContentArea.Items.ElementAt(index);
        if (ContentLoader?.TryGet<IContentData>(item.ContentLink, out var content) == true)
        {
            return PipelineService?.ExecutePipeline(content, Context);
        }

        return null;
    }

    private List<IContentPipelineModel> LoadItems()
    {
        if (ContentArea?.Items == null || PipelineService == null || ContentLoader == null || Context == null)
            return new List<IContentPipelineModel>();

        var items = new List<IContentPipelineModel>();
        
        foreach (var item in ContentArea.Items)
        {
            if (ContentLoader.TryGet<IContentData>(item.ContentLink, out var content))
            {
                var model = PipelineService.ExecutePipeline(content, Context);
                if (model != null)
                    items.Add(model);
            }
        }

        return items;
    }

    public IEnumerator<IContentPipelineModel> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public enum LazyLoadBehavior
{
    Auto,
    Immediate,
    Lazy,
    OnViewport
}
```

## Error Handling and Resilience

### Graceful Degradation

Implement robust error handling that maintains functionality:

```csharp
public class ResilientContentStep : AsyncContentPipelineStep<IContentData, IContentPipelineModel>
{
    private readonly ILogger<ResilientContentStep> _logger;
    private readonly ICircuitBreaker _circuitBreaker;
    private readonly IRetryPolicy _retryPolicy;
    private readonly IConfiguration _configuration;

    public ResilientContentStep(
        ILogger<ResilientContentStep> logger,
        ICircuitBreaker circuitBreaker,
        IRetryPolicy retryPolicy,
        IConfiguration configuration) : base(order: 100)
    {
        _logger = logger;
        _circuitBreaker = circuitBreaker;
        _retryPolicy = retryPolicy;
        _configuration = configuration;
    }

    public override async Task ExecuteAsync(
        IContentData content,
        IContentPipelineModel contentPipelineModel,
        IContentPipelineContext pipelineContext)
    {
        try
        {
            await _circuitBreaker.ExecuteAsync(async () =>
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await ProcessContentWithExternalService(content, contentPipelineModel, pipelineContext);
                });
            });
        }
        catch (CircuitBreakerOpenException)
        {
            _logger.LogWarning("Circuit breaker is open, using fallback for content {ContentId}", content.ContentLink);
            ApplyFallbackValues(contentPipelineModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing content {ContentId}, applying fallback", content.ContentLink);
            ApplyFallbackValues(contentPipelineModel);
            
            // Optionally re-throw based on configuration
            if (ShouldFailOnError())
                throw;
        }
    }

    private async Task ProcessContentWithExternalService(
        IContentData content,
        IContentPipelineModel model,
        IContentPipelineContext context)
    {
        // External service call that might fail
        var externalData = await CallExternalService(content);
        EnrichModelWithExternalData(model, externalData);
    }

    private void ApplyFallbackValues(IContentPipelineModel model)
    {
        if (model is IEnrichable enrichable)
        {
            enrichable.ExternalData = new ExternalData
            {
                IsFromFallback = true,
                Message = "External service unavailable",
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    private bool ShouldFailOnError()
    {
        return _configuration.GetValue<bool>("ContentPipeline:FailOnError", false);
    }
}
```

These advanced patterns enable you to build robust, scalable, and maintainable ContentPipeline implementations that can handle complex scenarios while maintaining good performance and reliability.