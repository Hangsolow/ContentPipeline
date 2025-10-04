using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Org.BouncyCastle.Ocsp;
using EPiServer.Web.Routing;
using EPiServer;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.ContentPropertyConverters;
using Microsoft.Extensions.DependencyInjection;
using ContentPipeline.Interfaces;
using ContentPipeline.ServiceCollectionExtensions;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities;
using EPiServer.Core;
using System.Globalization;
using ContentPipeline.Entities;
using Microsoft.AspNetCore.Http;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.PipelineSteps;

namespace ContentPipelineSourceGeneratorTests.Benchmarks;
[MemoryDiagnoser]
[SimpleJob(warmupCount: 2, iterationCount: 5)]
public class ContentPipelineServiceBenchmarks
{
    private IContentPipelineService contentPipelineService = null!;

    private readonly TestData testData = new(Substitute.For<IContentLoader>(), Substitute.For<IUrlResolver>(), Substitute.For<ITempDataProvider>(), Substitute.For<IHtmlHelper>());
    private static readonly ContentPageTestData contentPageTestData = new("Title of content", "url/here", 10, 20, 50, 30, 40, new List<string> { "List", "of", "strings" });
    private readonly ContentPage contentPage = new ContentPage
    {
        Title = contentPageTestData.Title,
        Url = new Url($"/{contentPageTestData.Url}"),
        IgnoreLink = new EPiServer.Core.ContentReference(contentPageTestData.IgnoreLinkId),
        Link = new EPiServer.Core.ContentReference(contentPageTestData.LinkId),
        MediaLink = new EPiServer.Core.ContentReference(contentPageTestData.MediaLinkId),
        BlockLink = new EPiServer.Core.ContentReference(contentPageTestData.BlockLinkId),
        LinkToPage = new PageReference(contentPageTestData.PageLinkId),
        ListOfStrings = contentPageTestData.List,
        CustomMapping = new EPiServer.Core.XhtmlString("Text String")
    };

    private readonly HttpContext httpContext = new DefaultHttpContext();

    [GlobalSetup]
    public void GlobalSetup()
    {
        IServiceCollection services = new ServiceCollection();
        var contentLoader = testData.ContentLoader;
        var urlResolver = testData.UrlResolver;
        var tempDataProvider = testData.TempDataProvider;

        services
            .AddContentPipelineServices()
            .AddTransient<CustomConverter>()
            .AddTransient<DatasourceConverter>()
            .AddTransient<IContentPipelineStep<ContentPage, ContentPipeline.Models.Awesome.ContentPagePipelineModel>, DefaultAsyncPipelineStep>()
            .AddTransient(sl => contentLoader)
            .AddTransient(sl => urlResolver)
            .AddTransient(sl => tempDataProvider)
            ;

        urlResolver.GetUrl(contentPage.Link, null, null).Returns($"/link/{contentPageTestData.LinkId}");
        urlResolver.GetUrl(contentPage.LinkToPage, null, null).Returns($"/link/{contentPageTestData.PageLinkId}");
        urlResolver.GetUrl(contentPage.MediaLink, null, null).Returns($"/link/{contentPageTestData.MediaLinkId}");

        var imageContent = new Jpg
        {
            Title = "Image Title",
            AltText = "Alt Text",
            Copyright = "Copyright Text",
            ContentLink = contentPage.MediaLink,
        };

        contentLoader.TryGet(contentPage.MediaLink, out Arg.Any<IContent>()).Returns(x =>
        {
            x[1] = imageContent;
            return true;
        });

        var blockContent = new ContentBlock
        {
            Color = SourceGeneratorTests.Entities.Enums.ColorEnum.Red,
            Header = "Header",
            Link = contentPage.BlockLink,
        };


        contentLoader.TryGet(contentPage.BlockLink, Arg.Any<CultureInfo>(), out Arg.Any<IContentData>()).Returns(x =>
        {
            x[2] = blockContent;
            return true;
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        contentPipelineService = serviceProvider.GetRequiredService<IContentPipelineService>();
    }

    [Benchmark(Baseline = true)]
    public IContentPipelineModel BenchmarkPipelineService()
    {
        ContentPipelineContext context = new(httpContext, null, contentPipelineService);

        return contentPipelineService.ExecutePipeline(contentPage, context);
    }

    [Benchmark]
    public async Task<IContentPipelineModel> BenchmarkPipelineServiceAsync()
    {
        ContentPipelineContext context = new(httpContext, null, contentPipelineService);

        return await contentPipelineService.ExecutePipelineAsync(contentPage, context);
    }
    protected record TestData(IContentLoader ContentLoader, IUrlResolver UrlResolver, ITempDataProvider TempDataProvider, IHtmlHelper HtmlHelper);
    protected record ContentPageTestData(string Title, string Url, int IgnoreLinkId, int LinkId, int PageLinkId, int MediaLinkId, int BlockLinkId, List<string> List);
}


