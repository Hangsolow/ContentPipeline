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

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Tests.ServiceCollectionTests
{
    public class ContentPipelineServiceCollectionExtensionsTests
    {
        [Fact]
        public void Should_Resolve_ContentPipelineService()
        {
            IServiceCollection services = new ServiceCollection();
            var contentLoader = Substitute.For<IContentLoader>();
            var urlResolver = Substitute.For<IUrlResolver>();
            var tempDataProvider = Substitute.For<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>();

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
    }
}
