# Testing

This document provides comprehensive testing strategies and examples for ContentPipeline implementations.

## Testing Strategy Overview

Testing ContentPipeline involves several layers:

1. **Unit Tests**: Test individual property converters and pipeline steps
2. **Integration Tests**: Test the complete pipeline with generated models
3. **Source Generator Tests**: Verify code generation behavior
4. **Performance Tests**: Ensure acceptable performance characteristics

## Test Project Setup

### Basic Test Project Structure

```
Tests/
├── Unit/
│   ├── Converters/
│   ├── PipelineSteps/
│   └── Services/
├── Integration/
│   ├── PipelineTests/
│   └── ApiTests/
├── SourceGenerator/
│   └── GenerationTests/
├── Performance/
│   └── BenchmarkTests/
└── TestData/
    ├── ContentModels/
    └── MockData/
```

### Dependencies

Add these NuGet packages to your test project:

```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="xunit" Version="2.4.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.4.5" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="NSubstitute" Version="5.1.0" />
<PackageReference Include="AutoFixture" Version="4.18.0" />
<PackageReference Include="AutoFixture.Xunit2" Version="4.18.0" />
<PackageReference Include="AutoFixture.AutoNSubstitute" Version="4.18.0" />
<PackageReference Include="Microsoft.AspNetCore.TestHost" Version="7.0.0" />
```

## Unit Testing

### Testing Property Converters

```csharp
using ContentPipeline.Interfaces;
using EPiServer.Core;
using FluentAssertions;
using NSubstitute;
using Xunit;

public class XhtmlToSummaryConverterTests
{
    [Fact]
    public void GetValue_WithNullInput_ReturnsEmptyString()
    {
        // Arrange
        var converter = new XhtmlToSummaryConverter();
        var content = Substitute.For<IContentData>();
        var context = Substitute.For<IContentPipelineContext>();

        // Act
        var result = converter.GetValue(null, content, "TestProperty", context);

        // Assert
        result.Should().Be(string.Empty);
    }

    [Fact]
    public void GetValue_WithValidXhtml_ReturnsPlainText()
    {
        // Arrange
        var converter = new XhtmlToSummaryConverter();
        var xhtml = new XhtmlString("<p>Hello <strong>World</strong></p>");
        var content = Substitute.For<IContentData>();
        var context = Substitute.For<IContentPipelineContext>();

        // Act
        var result = converter.GetValue(xhtml, content, "TestProperty", context);

        // Assert
        result.Should().Be("Hello World");
    }

    [Theory]
    [InlineData("<p>Short text</p>", "Short text")]
    [InlineData("<div>A very long text that exceeds the maximum length limit</div>", "A very long text that exceed...")]
    public void GetValue_WithMaxLengthConfig_TruncatesCorrectly(string input, string expected)
    {
        // Arrange
        var converter = new XhtmlToSummaryConverter();
        var xhtml = new XhtmlString(input);
        var content = Substitute.For<IContentData>();
        var context = Substitute.For<IContentPipelineContext>();
        var config = new Dictionary<string, string> { ["maxLength"] = "30" };

        // Act
        var result = converter.GetValue(xhtml, content, "TestProperty", context, config);

        // Assert
        result.Should().Be(expected);
    }
}
```

### Testing Pipeline Steps

```csharp
public class SeoEnrichmentStepTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidContent_EnrichesModel()
    {
        // Arrange
        var step = new SeoEnrichmentStep();
        var content = new ArticlePage
        {
            Title = "Test Article",
            Introduction = "This is a test article introduction"
        };
        var model = new ArticlePagePipelineModel();
        var context = Substitute.For<IContentPipelineContext>();

        // Act
        await step.ExecuteAsync(content, model, context);

        // Assert
        model.SeoTitle.Should().Be("Test Article | My Site");
        model.MetaDescription.Should().StartWith("This is a test article");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullTitle_HandlesGracefully()
    {
        // Arrange
        var step = new SeoEnrichmentStep();
        var content = new ArticlePage { Title = null };
        var model = new ArticlePagePipelineModel();
        var context = Substitute.For<IContentPipelineContext>();

        // Act & Assert
        await step.Invoking(s => s.ExecuteAsync(content, model, context))
                 .Should().NotThrowAsync();
        
        model.SeoTitle.Should().Be("| My Site");
    }
}
```

### Testing with AutoFixture

```csharp
using AutoFixture;
using AutoFixture.Xunit2;

public class ContentServiceTests
{
    [Theory, AutoData]
    public void ConvertContent_WithValidContent_ReturnsModel(
        [Frozen] IContentPipelineService pipelineService,
        [Frozen] IContentLoader contentLoader,
        ContentService sut,
        ArticlePage content)
    {
        // Arrange
        var expectedModel = new ArticlePagePipelineModel
        {
            Title = content.Title,
            Introduction = content.Introduction
        };
        
        pipelineService.ExecutePipeline(Arg.Any<IContentData>(), Arg.Any<IContentPipelineContext>())
                      .Returns(expectedModel);

        // Act
        var result = sut.ConvertContent(content, Mock.Of<HttpContext>());

        // Assert
        result.Should().BeEquivalentTo(expectedModel);
    }
}
```

## Integration Testing

### Pipeline Integration Tests

```csharp
public class ArticlePipelineIntegrationTests : IClassFixture<TestServerFixture>
{
    private readonly TestServerFixture _fixture;

    public ArticlePipelineIntegrationTests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Pipeline_WithCompleteArticle_GeneratesCorrectModel()
    {
        // Arrange
        var content = CreateTestArticle();
        var pipelineService = _fixture.Services.GetRequiredService<IContentPipelineService>();
        var context = new ContentPipelineContext
        {
            HttpContext = _fixture.CreateHttpContext()
        };

        // Act
        var result = pipelineService.ExecutePipeline(content, context);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ArticlePagePipelineModel>();
        
        var model = result as ArticlePagePipelineModel;
        model.Title.Should().Be(content.Title);
        model.Introduction.Should().Be(content.Introduction);
        model.MainContent.Should().NotBeEmpty();
        model.FeaturedImage?.Url.Should().NotBeEmpty();
    }

    private ArticlePage CreateTestArticle()
    {
        return new ArticlePage
        {
            Title = "Test Article",
            Introduction = "Test introduction",
            MainContent = new XhtmlString("<p>Test content</p>"),
            FeaturedImage = new ContentReference(123)
        };
    }
}

public class TestServerFixture : IDisposable
{
    public IServiceProvider Services { get; }
    public TestServer Server { get; }

    public TestServerFixture()
    {
        var hostBuilder = new WebHostBuilder()
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.AddContentPipelineServices();
                services.AddTransient<IContentLoader, MockContentLoader>();
                services.AddTransient<IUrlResolver, MockUrlResolver>();
            });

        Server = new TestServer(hostBuilder);
        Services = Server.Services;
    }

    public HttpContext CreateHttpContext()
    {
        return new DefaultHttpContext
        {
            RequestServices = Services
        };
    }

    public void Dispose()
    {
        Server?.Dispose();
    }
}
```

### API Integration Tests

```csharp
public class ArticleApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ArticleApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetArticle_WithValidId_ReturnsCorrectData()
    {
        // Arrange
        var articleId = 123;

        // Act
        var response = await _client.GetAsync($"/api/articles/{articleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var article = JsonSerializer.Deserialize<ArticleData>(content);
        
        article.Should().NotBeNull();
        article.Id.Should().Be(articleId);
        article.Title.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetArticle_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = 999;

        // Act
        var response = await _client.GetAsync($"/api/articles/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

## Source Generator Testing

### Testing Generated Code

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

public class SourceGeneratorTests
{
    [Fact]
    public async Task Generator_WithValidContentModel_GeneratesCorrectPipelineModel()
    {
        // Arrange
        var source = @"
using EPiServer.Core;
using EPiServer.DataAnnotations;
using ContentPipeline.Attributes;

[ContentType(GUID = ""12345678-1234-1234-1234-123456789012"")]
[ContentPipelineModel(""Test"")]
public class TestPage : PageData
{
    public virtual string? Title { get; set; }
    public virtual string? Description { get; set; }
}";

        var expectedGenerated = @"
namespace ContentPipeline.Models.Test
{
    public partial class TestPagePipelineModel : ContentPipeline.Interfaces.IContentPipelineModel
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}";

        // Act & Assert
        await new SourceGeneratorTest
        {
            TestState =
            {
                Sources = { source },
                GeneratedSources =
                {
                    ("TestPagePipelineModel.g.cs", expectedGenerated)
                }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task Generator_WithIgnoredProperty_ExcludesFromModel()
    {
        // Arrange
        var source = @"
using EPiServer.Core;
using EPiServer.DataAnnotations;
using ContentPipeline.Attributes;

[ContentType(GUID = ""12345678-1234-1234-1234-123456789012"")]
[ContentPipelineModel(""Test"")]
public class TestPage : PageData
{
    public virtual string? Title { get; set; }
    
    [ContentPipelineIgnore]
    public virtual string? AdminNotes { get; set; }
}";

        // Act
        var result = await CompileWithGenerator(source);

        // Assert
        var generatedModel = result.GeneratedSources
            .First(s => s.FileName.Contains("TestPagePipelineModel"));
            
        generatedModel.Text.Should().Contain("Title");
        generatedModel.Text.Should().NotContain("AdminNotes");
    }

    private async Task<GeneratorResult> CompileWithGenerator(string source)
    {
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { CSharpSyntaxTree.ParseText(source) },
            MetadataReferences.GetMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ContentPipelineSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
        
        return new GeneratorResult
        {
            Compilation = outputCompilation,
            Diagnostics = diagnostics,
            GeneratedSources = outputCompilation.SyntaxTrees
                .Where(t => t.FilePath.Contains("Generated"))
                .Select(t => new GeneratedSource(t.FilePath, t.ToString()))
                .ToList()
        };
    }
}

public class GeneratorResult
{
    public Compilation Compilation { get; set; }
    public ImmutableArray<Diagnostic> Diagnostics { get; set; }
    public List<GeneratedSource> GeneratedSources { get; set; }
}

public class GeneratedSource
{
    public string FileName { get; set; }
    public string Text { get; set; }
    
    public GeneratedSource(string fileName, string text)
    {
        FileName = fileName;
        Text = text;
    }
}
```

## Performance Testing

### Benchmark Tests

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net60)]
public class PipelineBenchmarks
{
    private IContentPipelineService _pipelineService;
    private ArticlePage _testContent;
    private IContentPipelineContext _context;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection()
            .AddContentPipelineServices()
            .AddSingleton<IContentLoader, MockContentLoader>()
            .BuildServiceProvider();

        _pipelineService = services.GetRequiredService<IContentPipelineService>();
        _testContent = CreateTestContent();
        _context = new ContentPipelineContext();
    }

    [Benchmark]
    public IContentPipelineModel ExecutePipeline()
    {
        return _pipelineService.ExecutePipeline(_testContent, _context);
    }

    [Benchmark]
    public async Task<IContentPipelineModel> ExecutePipelineAsync()
    {
        return await _pipelineService.ExecutePipelineAsync(_testContent, _context);
    }

    [Params(1, 10, 100)]
    public int ContentCount { get; set; }

    [Benchmark]
    public List<IContentPipelineModel> ExecuteMultiplePipelines()
    {
        var results = new List<IContentPipelineModel>();
        
        for (int i = 0; i < ContentCount; i++)
        {
            results.Add(_pipelineService.ExecutePipeline(_testContent, _context));
        }
        
        return results;
    }

    private ArticlePage CreateTestContent()
    {
        return new ArticlePage
        {
            Title = "Performance Test Article",
            Introduction = "This is a performance test",
            MainContent = new XhtmlString("<p>Content for performance testing</p>")
        };
    }
}

// Run benchmarks
public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<PipelineBenchmarks>();
    }
}
```

### Load Testing

```csharp
[Collection("LoadTest")]
public class PipelineLoadTests
{
    private readonly IContentPipelineService _pipelineService;
    private readonly List<ArticlePage> _testContent;

    public PipelineLoadTests()
    {
        // Setup test data and services
        _pipelineService = CreatePipelineService();
        _testContent = CreateTestContent(1000);
    }

    [Fact]
    public async Task Pipeline_UnderHighLoad_MaintainsPerformance()
    {
        // Arrange
        var concurrency = Environment.ProcessorCount * 2;
        var semaphore = new SemaphoreSlim(concurrency);
        var tasks = new List<Task<TimeSpan>>();

        // Act
        foreach (var content in _testContent)
        {
            tasks.Add(ProcessWithTiming(content, semaphore));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        var averageTime = results.Average(r => r.TotalMilliseconds);
        var maxTime = results.Max(r => r.TotalMilliseconds);
        
        averageTime.Should().BeLessThan(100); // 100ms average
        maxTime.Should().BeLessThan(500);     // 500ms max
        
        var successRate = results.Count(r => r.TotalMilliseconds < 1000) / (double)results.Length;
        successRate.Should().BeGreaterThan(0.95); // 95% success rate
    }

    private async Task<TimeSpan> ProcessWithTiming(ArticlePage content, SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            var result = _pipelineService.ExecutePipeline(content, new ContentPipelineContext());
            
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
        finally
        {
            semaphore.Release();
        }
    }
}
```

## Mock Objects and Test Helpers

### Mock Content Loader

```csharp
public class MockContentLoader : IContentLoader
{
    private readonly Dictionary<ContentReference, IContentData> _content = new();

    public void AddContent(ContentReference reference, IContentData content)
    {
        _content[reference] = content;
    }

    public T Get<T>(ContentReference contentLink) where T : class, IContentData
    {
        return _content.TryGetValue(contentLink, out var content) ? content as T : null;
    }

    public bool TryGet<T>(ContentReference contentLink, out T content) where T : class, IContentData
    {
        content = Get<T>(contentLink);
        return content != null;
    }

    // Implement other IContentLoader methods as needed
}
```

### Test Data Builders

```csharp
public class ArticlePageBuilder
{
    private readonly ArticlePage _article = new();

    public ArticlePageBuilder WithTitle(string title)
    {
        _article.Title = title;
        return this;
    }

    public ArticlePageBuilder WithIntroduction(string introduction)
    {
        _article.Introduction = introduction;
        return this;
    }

    public ArticlePageBuilder WithContent(string content)
    {
        _article.MainContent = new XhtmlString(content);
        return this;
    }

    public ArticlePageBuilder WithFeaturedImage(ContentReference image)
    {
        _article.FeaturedImage = image;
        return this;
    }

    public ArticlePage Build() => _article;

    public static implicit operator ArticlePage(ArticlePageBuilder builder) => builder.Build();
}

// Usage in tests
var article = new ArticlePageBuilder()
    .WithTitle("Test Article")
    .WithIntroduction("Test intro")
    .WithContent("<p>Test content</p>");
```

## Test Configuration

### Test Settings

```json
{
  "ContentPipeline": {
    "EnableDebugLogging": true,
    "ValidationEnabled": true,
    "CacheEnabled": false
  },
  "ConnectionStrings": {
    "EPiServerDB": "Data Source=(localdb)\\MSSQLLocalDB;Database=ContentPipelineTests;Integrated Security=true"
  }
}
```

### Test Startup Configuration

```csharp
public class TestStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPipelineServices();
        
        // Replace with test implementations
        services.Replace<IContentLoader, MockContentLoader>();
        services.Replace<IUrlResolver, MockUrlResolver>();
        
        // Add test-specific services
        services.AddSingleton<TestDataSeeder>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Test-specific pipeline configuration
    }
}
```

## Continuous Integration

### Test Execution in CI

```yaml
# .github/workflows/test.yml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 6.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Run unit tests
      run: dotnet test --configuration Release --logger trx --collect:"XPlat Code Coverage"
      
    - name: Run integration tests
      run: dotnet test Tests.Integration --configuration Release
      
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
```

This comprehensive testing approach ensures your ContentPipeline implementation is reliable, performant, and maintainable across different scenarios and environments.