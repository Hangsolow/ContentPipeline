---
description: 'Development guidelines for ContentPipeline source generator'
applyTo: '**/*.cs'
---

# ContentPipeline Development Guide

ContentPipeline is a Roslyn source generator for Optimizely CMS that converts content models to JSON-friendly pipeline models at compile time. These guidelines help maintain consistency and quality in the codebase.

## Project Overview

- **Purpose**: Source generator for Optimizely CMS content transformation
- **Target Framework**: .NET Standard 2.0 (source generator), .NET 8+ (runtime)
- **Language Version**: C# 11 (source generator), C# 13 (consumer code)
- **Architecture**: Pipeline pattern with extensible steps and custom property converters
- **Documentation**: See [docs/](../docs/) for comprehensive guides

## C# Coding Standards

### Language Features

- Use C# 11 features in source generator code (netstandard2.0 constraint)
- Use latest C# 13 features in test and runtime code
- Prefer file-scoped namespace declarations (enforced in `.editorconfig`)
- Use implicit usings where enabled
- Use pattern matching and switch expressions for cleaner code
- Use `nameof` instead of string literals for member names

### Nullable Reference Types

- Nullable is enabled globally in `Directory.Build.props`
- Nullable warnings are treated as errors (`<WarningsAsErrors>nullable</WarningsAsErrors>`)
- Always use `is null` or `is not null` instead of `== null` or `!= null`
- Declare variables non-nullable by default, check for null at entry points
- Trust the null annotations - don't add unnecessary null checks

### Naming Conventions

- **PascalCase**: Classes, methods, properties, public members
- **camelCase**: Private fields, local variables, parameters
- **MACRO_CASE**: Constants
- **Prefix with "I"**: Interface names (e.g., `IContentPipelineService`)
- Use descriptive names that reflect purpose (e.g., `ContentPipelineModelAttribute`)

### Code Formatting

- Follow `.editorconfig` settings strictly
- File-scoped namespaces are required (warning level)
- Expression-bodied properties/accessors/lambdas preferred
- Regular methods should use block bodies (not expression-bodied)
- Use spaces, not tabs (defined in `.editorconfig`)
- Place opening braces on new lines for consistency

### Documentation

- Add XML doc comments for all public APIs
- Include `<summary>`, `<param>`, `<returns>`, and `<exception>` tags
- Add `<example>` and `<code>` sections for complex functionality
- Reference related documentation in [docs/](../docs/) folder
- Keep comments clear and concise, explaining "why" not "what"

## Source Generator Development

### Roslyn Source Generator Patterns

- Source generators must target .NET Standard 2.0
- Use `IIncrementalGenerator` interface for incremental generation
- Implement proper caching and incremental updates for performance
- Generate files in appropriate namespaces (e.g., `ContentPipeline.Models.{GroupName}`)
- Add `#nullable enable` to all generated files
- Include generated code attributes: `[GeneratedCode("ContentPipeline", "version")]`

### Code Generation Structure

Generated code is organized as follows:
```
Generated/
├── Interfaces/           # Core interfaces (IContentPipelineModel, etc.)
├── Models/{Group}/      # Pipeline models per content type
├── Properties/          # Shared property models (Link, Media, etc.)
├── Services/            # Service registration extensions
└── Pipelines/{Group}/Steps/  # Pipeline step implementations
```

### Code Builder Patterns

- Use `CodeBuilders/` classes for generating consistent code
- Separate concerns: interfaces, models, steps, services
- Generate partial classes to allow user extensions
- Include proper using directives in generated code
- Handle edge cases in property type conversions

### Diagnostics and Debugging

- Enable source generator diagnostics in test projects:
  ```xml
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
  ```
- Generated files are in `obj/Debug/{framework}/generated/ContentPipeline/`
- Use `ReportDiagnostic()` for compilation errors/warnings in source generator
- Test generated code with actual Optimizely CMS types

## Optimizely CMS Integration

### Content Model Attributes

- Content types must have both `[ContentType]` and `[ContentPipelineModel]` attributes
- `ContentPipelineModelAttribute` properties:
  - `Group`: Organizes models into logical groups (default: "Common")
  - `Order`: Determines processing order within group
- Detect content types from Roslyn syntax trees

### Property Converters

- Implement `IContentPropertyConverter<TProperty, TValue>` for custom conversions
- Common conversions: `XhtmlString → string`, `ContentReference → Link`, `ContentArea → ContentAreaPipelineModel`
- Register converters in DI container
- Handle nullable types appropriately
- Optimize for performance (converters run frequently)

### Pipeline Architecture

- Pipeline steps extend `AsyncContentPipelineStep<TContent, TModel>` or `ContentPipelineStep<TContent, TModel>`
- Specify `order` parameter to control execution sequence
- Use `IContentPipelineContext` for passing request-scoped data
- Support both sync and async execution patterns
- Handle errors gracefully within pipeline steps

## Testing Guidelines

### Test Framework

- Use **xUnit** for all tests
- Use **AutoFixture with NSubstitute** for mocking (`AutoMockAttribute`)
- Use **BenchmarkDotNet** for performance tests (separate project)

### Test Structure and Conventions

- Test class naming: `{ComponentName}Tests` (e.g., `PipelineStepPropertiesTests`)
- Test method naming: `{MethodName}_Should_{ExpectedBehavior}` or `{Scenario}_Should_{ExpectedBehavior}`
- Do NOT use "Arrange", "Act", "Assert" comments
- Use `[Trait("Category", "Value")]` to categorize tests
- Use `[Fact]` for simple tests, `[Theory]` with `[InlineData]` for parameterized tests

### Test Organization

```
tests/
├── ContentSourceGeneratorTests/     # Main test project
│   ├── SourceGeneratorTests/        # Source generator validation tests
│   ├── Tests/                       # Unit tests
│   └── Benchmarks/                  # Performance benchmarks
└── ContentPipelineBenchmarks/       # Standalone benchmark project
```

### What to Test

- Source generator output validation (syntax tree analysis)
- Pipeline step execution and ordering
- Property converter transformations
- Service registration and DI integration
- Edge cases: null handling, empty collections, missing properties
- Performance characteristics for critical paths

### Example Test Pattern

```csharp
[Trait("Pipelines", "")]
public class PipelineStepPropertiesTests
{
    [Fact]
    public void AsyncPipelineStep_Should_Have_IsAsync_True()
    {
        var step = new DefaultAsyncPipelineStep();
        
        Assert.True(step.IsAsync);
    }
}
```

## Build and Package Management

### Project Configuration

- Use `Directory.Build.props` for global settings
- Enable `RestorePackagesWithLockFile` for reproducible builds
- Lock mode enforced in CI with `RestoreLockedMode`
- Source generator project has `<IncludeBuildOutput>false</IncludeBuildOutput>`
- Package as analyzer: `Pack="true" PackagePath="analyzers/dotnet/cs"`

### Dependencies

- Minimize dependencies in source generator (avoid version conflicts)
- Use appropriate versions for Roslyn APIs (`Microsoft.CodeAnalysis.CSharp`)
- Test project uses Optimizely CMS packages (EPiServer.*)
- Keep package references up to date but test compatibility

### NuGet Package

- Package ID: `Hangsolow.ContentPipeline`
- Development dependency: `<DevelopmentDependency>true</DevelopmentDependency>`
- Include `readme.md` in package
- Suppress dependencies when packing (source generator pattern)

## Documentation

### User Documentation

All documentation is in the [docs/](../docs/) folder:
- **[Getting Started](../docs/getting-started.md)**: Setup and basic usage
- **[Architecture](../docs/architecture.md)**: How the system works
- **[Configuration](../docs/configuration.md)**: Advanced configuration options
- **[Advanced Usage](../docs/advanced-usage.md)**: Complex scenarios and patterns
- **[Testing](../docs/testing.md)**: Testing strategies
- **[API Reference](../docs/api-reference.md)**: Complete API documentation
- **[Troubleshooting](../docs/troubleshooting.md)**: Common issues and solutions

### Updating Documentation

- Update relevant docs when adding features or changing behavior
- Keep code examples in documentation working and up-to-date
- Cross-reference related documentation sections
- Include XML doc comments for IntelliSense support

## Common Patterns

### Adding a New Property Converter

1. Create class implementing `IContentPropertyConverter<TProperty, TValue>`
2. Add to generated service registrations in source generator
3. Document in [docs/advanced-usage.md](../docs/advanced-usage.md)
4. Add tests for conversion logic

### Adding a New Pipeline Step

1. Extend `AsyncContentPipelineStep` or `ContentPipelineStep`
2. Set appropriate order value
3. Register in DI container
4. Add tests for step execution

### Extending Generated Models

- Generated classes are `partial` - users can extend them
- Add additional properties or methods in separate partial class files
- Don't modify generated code directly (it will be regenerated)

## Performance Considerations

- Source generators run during compilation - optimize for speed
- Use incremental generation patterns (`IIncrementalGenerator`)
- Cache expensive computations in pipeline steps
- Avoid reflection in hot paths
- Profile with BenchmarkDotNet for critical code paths

## Troubleshooting

For common issues, see [docs/troubleshooting.md](../docs/troubleshooting.md)

Key debugging steps:
1. Enable `EmitCompilerGeneratedFiles` to inspect generated code
2. Check build output for source generator diagnostics
3. Verify both `[ContentType]` and `[ContentPipelineModel]` attributes present
4. Clean and rebuild if generator not updating
5. Check namespace conflicts in generated code

## Contributing

- Follow existing code patterns and conventions
- Add tests for new features
- Update documentation for user-facing changes
- Keep commits focused and descriptive
- Reference issue numbers in commit messages