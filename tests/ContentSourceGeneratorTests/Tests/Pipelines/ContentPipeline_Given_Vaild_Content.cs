using ContentPipeline.Entities;
using ContentPipeline.Interfaces;
using ContentPipeline.Models.Common;
using ContentPipeline.Models.MediaContent;
using ContentPipeline.Properties;
using ContentPipeline.ServiceCollectionExtensions;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.ContentPropertyConverters;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities;
using ContentPipelineSourceGeneratorTests.Utils;
using EPiServer;
using EPiServer.Core;
using EPiServer.Web.Routing;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Globalization;
using System.Reflection;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Attributes;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.PipelineSteps;

namespace ContentPipelineSourceGeneratorTests.Tests.Pipelines;

[Trait("Pipelines", "")]
public class ContentPipeline_Given_Vaild_Content
{
    [AutoMock, Theory]
    protected async Task Should_Map_Properties_For_ContentPage(TestData testData, ContentPageTestData contentPageTestData)
    {
        var contentPage = new ContentPage
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
        var datasourceAttribute = contentPage.GetType()
            .GetProperty(nameof(ContentPage.CustomMappingWithCustomAttribute))
            ?.GetCustomAttribute<DatasourceAttribute>()!;
        
        IServiceCollection services = new ServiceCollection();
        (var contentLoader, var urlResolver, var tempDataProvider, var htmlHelper) = testData;
        services
            .AddContentPipelineServices()
            .AddTransient<CustomConverter>()
            .AddTransient<DatasourceConverter>()
            .AddTransient<IContentPipelineStep<ContentPage, ContentPipeline.Models.Awesome.ContentPagePipelineModel>, DefaultAsyncPipelineStep>()
            .AddTransient(sl => contentLoader)
            .AddTransient(sl => urlResolver)
            .AddTransient(sl => tempDataProvider)
            .AddTransient(sl => htmlHelper)
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
        var contentPipelineService = serviceProvider.GetRequiredService<IContentPipelineService>();

        var httpContext = new DefaultHttpContext();

        var pipelineArgs = new PipelineArgs
        {
            HttpContext = httpContext,
            Content = contentPage
        };

        var contentPipelineModel = await contentPipelineService.ExecutePipelineAsync(pipelineArgs);
        var contentModel = (ContentPipeline.Models.Awesome.ContentPagePipelineModel)contentPipelineModel;

        Assert.NotNull(contentModel);
        Assert.Equal(contentPageTestData.Title, contentModel.Title);
        Assert.Equal(contentModel.ListOfStrings, contentModel.ListOfStrings);
        Assert.IsType<Link>(contentModel.Url);
        Assert.Equal($"/{contentPageTestData.Url}", ((Link)contentModel.Url!).Url);
        Assert.IsType<Link>(contentModel.Link);
        Assert.Equal($"/link/{contentPageTestData.LinkId}", ((Link)contentModel.Link!).Url);
        Assert.IsType<Link>(contentModel.LinkToPage);
        Assert.Equal($"/link/{contentPageTestData.PageLinkId}", ((Link)contentModel.LinkToPage!).Url);
        Assert.True(contentModel.CustomMapping);
        Assert.Equal(nameof(Jpg), contentModel.MediaLink?.Type);
        Assert.Equal($"/link/{contentPageTestData.MediaLinkId}", contentModel.MediaLink?.Url);
        Assert.IsType<JpgPipelineModel>(contentModel.MediaLink?.Properties);
        Assert.Equal(imageContent.Title, ((JpgPipelineModel)contentModel.MediaLink?.Properties!).Title);
        Assert.Equal(imageContent.AltText, ((JpgPipelineModel)contentModel.MediaLink?.Properties!).AltText);
        Assert.Equal(imageContent.Copyright, ((JpgPipelineModel)contentModel.MediaLink?.Properties!).Copyright);
        Assert.IsType<ContentBlockPipelineModel>(contentModel.BlockLink);
        Assert.Equal(datasourceAttribute.DatasourceConfig, contentModel.CustomMappingWithCustomAttribute?.Url);
        Assert.Equal(datasourceAttribute.DatasourceName, contentModel.CustomMappingWithCustomAttribute?.Id);
        var contentBlock = contentModel.BlockLink as ContentBlockPipelineModel;
        Assert.Equal(blockContent.Color.ToString(), contentBlock?.Color);
        Assert.Equal(blockContent.Header, contentBlock?.Header);
        Assert.True(string.IsNullOrEmpty(contentBlock?.Text));

    }

    protected record TestData(IContentLoader ContentLoader, IUrlResolver UrlResolver, ITempDataProvider TempDataProvider, IHtmlHelper HtmlHelper);
    protected record ContentPageTestData(string Title, string Url, int IgnoreLinkId, int LinkId, int PageLinkId, int MediaLinkId, int BlockLinkId, List<string> List);
}
