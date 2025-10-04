# Troubleshooting

This guide helps you diagnose and resolve common issues when working with ContentPipeline.

## Build and Compilation Issues

### Generated Code Not Appearing

**Problem**: Pipeline models are not being generated after adding attributes.

**Symptoms**:
- Missing generated models in IntelliSense
- Compilation errors about missing types
- Empty generated files folder

**Solutions**:

1. **Verify Attributes**: Ensure both attributes are present:
   ```csharp
   [ContentType(GUID = "...")] // Required
   [ContentPipelineModel("GroupName")] // Required
   public class MyPage : PageData
   ```

2. **Clean and Rebuild**:
   ```bash
   dotnet clean
   dotnet build
   ```

3. **Check Source Generator Package**:
   ```xml
   <PackageReference Include="Hangsolow.ContentPipeline" Version="latest">
     <PrivateAssets>all</PrivateAssets>
     <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
   </PackageReference>
   ```

4. **Enable Generated File Output**:
   ```xml
   <PropertyGroup>
     <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
     <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
   </PropertyGroup>
   ```

5. **Check Target Framework**: Ensure you're targeting a supported framework:
   ```xml
   <TargetFramework>net6.0</TargetFramework> <!-- or later -->
   ```

### Build Errors with Generated Code

**Problem**: Compilation fails with errors in generated code.

**Common Errors**:
```
CS0246: The type or namespace name 'ContentPipelineModel' could not be found
CS0234: The type or namespace name 'Models' does not exist
```

**Solutions**:

1. **Check Namespace Conflicts**:
   ```csharp
   // Avoid naming conflicts
   namespace MyProject.Models // Don't use "Models" directly
   {
       [ContentPipelineModel("Pages")]
       public class MyPage : PageData { }
   }
   ```

2. **Verify Content Type GUID**: Ensure GUIDs are unique and valid:
   ```csharp
   [ContentType(GUID = "12345678-1234-1234-1234-123456789012")] // Valid GUID format
   ```

3. **Check Property Accessibility**:
   ```csharp
   public virtual string? Title { get; set; } // Must be virtual and public
   ```

### Source Generator Diagnostics

**Enable Diagnostics**:

```xml
<PropertyGroup>
  <ReportAnalyzer>true</ReportAnalyzer>
  <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
  <WarningsAsErrors />
</PropertyGroup>
```

**View Diagnostics**: Check the build output for source generator messages:
```
1>ContentPipeline.SourceGenerator: Info: Generated model for ArticlePage in group 'Pages'
1>ContentPipeline.SourceGenerator: Warning: Property 'ComplexProperty' skipped - no converter found
```

## Runtime Issues

### Null Reference Exceptions

**Problem**: `NullReferenceException` when executing pipeline.

**Common Causes**:

1. **Service Not Registered**:
   ```csharp
   // In Program.cs - ensure this is called
   services.AddContentPipelineServices();
   ```

2. **Custom Converter Not Registered**:
   ```csharp
   services.AddSingleton<IContentPropertyConverter<CustomType, TargetType>, CustomConverter>();
   ```

3. **Missing HTTP Context**:
   ```csharp
   var context = new ContentPipelineContext 
   { 
       HttpContext = httpContext // Don't forget this
   };
   ```

### Property Conversion Failures

**Problem**: Properties are not being converted correctly.

**Debugging Steps**:

1. **Enable Debug Logging**: Use standard .NET logging configuration:
   ```csharp
   services.AddLogging(builder =>
   {
       builder.AddConsole();
       builder.SetMinimumLevel(LogLevel.Debug);
   });
   ```

2. **Check Converter Registration**: Verify custom converters are registered:
   ```csharp
   // In ConfigureServices
   services.AddSingleton<MyCustomConverter>();
   ```

3. **Test Converter Isolation**:
   ```csharp
   [Test]
   public void TestConverter()
   {
       var converter = new MyCustomConverter();
       var result = converter.GetValue(inputValue, content, propertyName, context);
       Assert.NotNull(result);
   }
   ```

### Performance Issues

**Problem**: Pipeline execution is slow.

**Performance Optimization**:

1. **Implement Caching**: Use memory caching for expensive operations:
   ```csharp
   services.AddMemoryCache();
   
   // In your pipeline step or converter
   public class CachedConverter : IContentPropertyConverter<XhtmlString?, string>
   {
       private readonly IMemoryCache _cache;
       
       public CachedConverter(IMemoryCache cache)
       {
           _cache = cache;
       }
   }
   ```

2. **Optimize Converters**: Avoid expensive operations in converters:
   ```csharp
   public class OptimizedConverter : IContentPropertyConverter<XhtmlString?, string>
   {
       private readonly IMemoryCache _cache;
   
       public string GetValue(XhtmlString? property, ...)
       {
           var cacheKey = $"xhtml_{property?.GetHashCode()}";
           return _cache.GetOrCreate(cacheKey, () => ProcessXhtml(property));
       }
   }
   ```

3. **Async Pipeline Steps**: Use async for I/O operations:
   ```csharp
   public class AsyncStep : AsyncContentPipelineStep<MyContent, MyModel>
   {
       public override async Task ExecuteAsync(...)
       {
           await SomeAsyncOperation();
       }
   }
   ```

## Configuration Issues

### Service Registration Problems

**Problem**: Services not resolving correctly.

**Common Issues**:

1. **Wrong Service Lifetime**:
   ```csharp
   // Correct: Register as singleton for stateless converters
   services.AddSingleton<IContentPropertyConverter<string, string>, MyConverter>();
   
   // Avoid: Transient for heavy objects
   services.AddTransient<ExpensiveConverter>(); // Bad
   ```

2. **Missing Dependencies**:
   ```csharp
   public class MyConverter : IContentPropertyConverter<string, ProcessedString>
   {
       private readonly IContentLoader _contentLoader; // Ensure this is registered
       
       public MyConverter(IContentLoader contentLoader)
       {
           _contentLoader = contentLoader;
       }
   }
   ```

### Configuration Validation

**Add Configuration Validation** for custom services:

```csharp
public class CustomPipelineValidator
{
    private readonly IServiceProvider _serviceProvider;
    
    public CustomPipelineValidator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public bool ValidateConfiguration()
    {
        // Check if required services are registered
        var pipeline = _serviceProvider.GetService<IContentPipeline<IContentData, IContentPipelineModel>>();
        return pipeline != null;
    }
}

// Register validator
services.AddSingleton<CustomPipelineValidator>();
```

## Content Model Issues

### Property Not Appearing in Pipeline Model

**Problem**: Content property is missing from generated pipeline model.

**Checklist**:

1. **Property is Virtual**:
   ```csharp
   public virtual string? Title { get; set; } // ✓ Correct
   public string? Title { get; set; }         // ✗ Wrong - not virtual
   ```

2. **Property is Public**:
   ```csharp
   public virtual string? Title { get; set; }    // ✓ Correct
   internal virtual string? Title { get; set; }  // ✗ Wrong - not public
   ```

3. **Property Has Getter and Setter**:
   ```csharp
   public virtual string? Title { get; set; }    // ✓ Correct
   public virtual string? Title { get; }         // ✗ Wrong - no setter
   ```

4. **Property Not Ignored**:
   ```csharp
   public virtual string? Title { get; set; }    // ✓ Included
   
   [ContentPipelineIgnore]
   public virtual string? Admin { get; set; }    // ✗ Excluded
   ```

### Custom Property Converter Not Working

**Problem**: Custom converter is not being used.

**Debugging Steps**:

1. **Check Attribute Syntax**:
   ```csharp
   [ContentPipelinePropertyConverter<MyConverter>] // ✓ Correct
   public virtual XhtmlString? Content { get; set; }
   ```

2. **Verify Converter Interface**:
   ```csharp
   public class MyConverter : IContentPropertyConverter<XhtmlString?, string> // ✓ Correct interface
   {
       public string GetValue(...) { ... }
   }
   ```

3. **Service Registration**:
   ```csharp
   services.AddSingleton<MyConverter>(); // Must be registered
   ```

4. **Check Generated Code**: Look at the generated pipeline step to see which converter is being used.

## Memory and Performance Issues

### Memory Leaks

**Problem**: Memory usage grows over time.

**Common Causes and Solutions**:

1. **Static Event Handlers**:
   ```csharp
   public class MyConverter : IContentPropertyConverter<string, string>, IDisposable
   {
       public MyConverter()
       {
           SomeStaticEvent += HandleEvent; // Can cause leaks
       }
       
       public void Dispose()
       {
           SomeStaticEvent -= HandleEvent; // Always unsubscribe
       }
   }
   ```

2. **Caching Issues**:
   ```csharp
   // Use proper cache eviction
   _cache.Set(key, value, new MemoryCacheEntryOptions
   {
       SlidingExpiration = TimeSpan.FromMinutes(30),
       Size = 1 // Enable size-based eviction
   });
   ```

3. **Large Object Accumulation**:
   ```csharp
   public class OptimizedConverter : IContentPropertyConverter<XhtmlString?, string>
   {
       public string GetValue(XhtmlString? property, ...)
       {
           if (property?.IsEmpty != false)
               return string.Empty; // Early return for empty content
           
           // Process only what's needed
           return ProcessMinimal(property);
       }
   }
   ```

### High CPU Usage

**Problem**: High CPU usage during pipeline execution.

**Optimization Strategies**:

1. **Reduce Reflection**:
   ```csharp
   // Cache reflection results
   private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();
   
   public PropertyInfo[] GetProperties(Type type)
   {
       return PropertyCache.GetOrAdd(type, t => t.GetProperties());
   }
   ```

2. **Optimize String Operations**:
   ```csharp
   // Use StringBuilder for multiple concatenations
   var sb = new StringBuilder();
   foreach (var item in items)
   {
       sb.Append(item);
   }
   return sb.ToString();
   ```

3. **Parallel Processing**:
   ```csharp
   public class ParallelProcessingStep : AsyncContentPipelineStep<MyContent, MyModel>
   {
       public override async Task ExecuteAsync(...)
       {
           var tasks = items.Select(async item => await ProcessItemAsync(item));
           await Task.WhenAll(tasks);
       }
   }
   ```

## Debugging Techniques

### Enable Detailed Logging

```csharp
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
    builder.SetMinimumLevel(LogLevel.Debug);
});
```

### Custom Diagnostic Pipeline Step

```csharp
public class DiagnosticStep : IContentPipelineStep<IContentData, IContentPipelineModel>
{
    private readonly ILogger<DiagnosticStep> _logger;
    
    public int Order => int.MaxValue; // Run last
    public bool IsAsync => false;
    
    public void Execute(IContentData content, IContentPipelineModel model, IContentPipelineContext context)
    {
        _logger.LogInformation("Pipeline executed for {ContentType} {ContentId}", 
            content.GetType().Name, content.ContentLink);
            
        // Log model properties
        var properties = model.GetType().GetProperties();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(model);
            _logger.LogDebug("Property {PropertyName}: {Value}", prop.Name, value);
        }
    }
    
    public Task ExecuteAsync(...) => Task.CompletedTask;
}
```

### Performance Profiling

```csharp
public class ProfilingStep : AsyncContentPipelineStep<IContentData, IContentPipelineModel>
{
    private readonly ILogger<ProfilingStep> _logger;
    
    public ProfilingStep(ILogger<ProfilingStep> logger) : base(order: 0)
    {
        _logger = logger;
    }
    
    public override async Task ExecuteAsync(...)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Your processing logic here
            await ProcessContent(content, contentPipelineModel, pipelineContext);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Processing took {ElapsedMs}ms for {ContentType}", 
                stopwatch.ElapsedMilliseconds, content.GetType().Name);
        }
    }
}
```

## Testing Issues

### Unit Test Setup Problems

**Problem**: Unit tests failing due to missing dependencies.

**Solution - Mock Setup**:

```csharp
[Test]
public void TestPipelineExecution()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddContentPipelineServices();
    
    // Mock external dependencies
    var mockContentLoader = new Mock<IContentLoader>();
    var mockUrlResolver = new Mock<IUrlResolver>();
    
    services.AddSingleton(mockContentLoader.Object);
    services.AddSingleton(mockUrlResolver.Object);
    
    var serviceProvider = services.BuildServiceProvider();
    var pipelineService = serviceProvider.GetRequiredService<IContentPipelineService>();
    
    // Act & Assert
    var result = pipelineService.ExecutePipeline(testContent, testContext);
    Assert.NotNull(result);
}
```

### Integration Test Issues

**Problem**: Integration tests failing in CI/CD environment.

**Solutions**:

1. **Environment-Specific Configuration**:
   ```csharp
   public class TestStartup
   {
       public void ConfigureServices(IServiceCollection services)
       {
           if (Environment.GetEnvironmentVariable("CI") == "true")
           {
               // CI-specific setup
               services.AddSingleton<IContentLoader, MockContentLoader>();
           }
           else
           {
               // Local development setup
               services.AddEpiServer();
           }
       }
   }
   ```

2. **Test Data Management**:
   ```csharp
   [SetUp]
   public void SetUp()
   {
       // Create test data
       _testContent = new ArticlePage
       {
           ContentLink = new ContentReference(123),
           Title = "Test Article"
       };
       
       // Setup mocks
       _mockContentLoader
           .Setup(x => x.TryGet<IContentData>(It.IsAny<ContentReference>(), out _testContent))
           .Returns(true);
   }
   ```

## Getting Help

### Enable Verbose Output

Add to your project file:
```xml
<PropertyGroup>
  <MSBuildVerbosity>detailed</MSBuildVerbosity>
</PropertyGroup>
```

### Community Resources

1. **GitHub Issues**: Report bugs at [repository issues](https://github.com/Hangsolow/ContentPipeline/issues)
2. **Discussions**: Ask questions in [GitHub Discussions](https://github.com/Hangsolow/ContentPipeline/discussions)
3. **Documentation**: Check the [docs folder](../docs/) for comprehensive guides

### Reporting Issues

When reporting issues, include:

1. **ContentPipeline Version**
2. **Optimizely CMS Version**
3. **Target Framework**
4. **Generated Code** (if relevant)
5. **Full Error Message and Stack Trace**
6. **Minimal Reproduction Case**

**Issue Template**:
```
## Environment
- ContentPipeline Version: x.x.x
- Optimizely CMS Version: x.x.x
- .NET Version: x.x
- OS: Windows/Linux/macOS

## Expected Behavior
[Description of expected behavior]

## Actual Behavior
[Description of actual behavior]

## Steps to Reproduce
1. [Step 1]
2. [Step 2]
3. [Step 3]

## Code Sample
```csharp
// Minimal code that reproduces the issue
```

## Error Message
```
[Full error message and stack trace]
```
```

This troubleshooting guide should help you resolve most common issues. If you encounter problems not covered here, please create an issue in the repository.