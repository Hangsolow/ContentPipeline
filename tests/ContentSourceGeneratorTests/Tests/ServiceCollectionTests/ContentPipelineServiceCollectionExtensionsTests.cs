using ContentPipeline.Entities;
using ContentPipeline.Interfaces;
using ContentPipeline.Models.Awesome;
using ContentPipeline.ServiceCollectionExtensions;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.ContentPropertyConverters;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities;
using EPiServer;
using EPiServer.Web.Routing;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using System.Globalization;
using ContentPipeline.Converters;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.Enums;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ContentPipelineSourceGeneratorTests.Utils;

namespace ContentPipelineSourceGeneratorTests.Tests.ServiceCollectionTests;

public class ContentPipelineServiceCollectionExtensionsTests
{
    [AutoMock, Theory]
    protected void Should_Resolve_ContentPipelineService(TestData testData)
    {
        IServiceCollection services = new ServiceCollection();
        (var contentLoader, var urlResolver, var tempDataProvider) = testData;

        services
            .AddContentPipelineServices()
            .AddTransient<CustomConverter>()
            .AddTransient(sl => contentLoader)
            .AddTransient(sl => urlResolver)
            .AddTransient(sl => tempDataProvider)
            ;
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var contentPipelineService = serviceProvider.GetRequiredService<IContentPipelineService>();

        contentPipelineService.Should().NotBeNull();
    }

    protected record TestData(IContentLoader ContentLoader, IUrlResolver UrlResolver, ITempDataProvider TempDataProvider);
}
