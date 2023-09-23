using ContentPipeline.Entities;
using ContentPipeline.Interfaces;
using ContentPipeline.Models.Common;
using ContentPipeline.Models.Media;
using ContentPipeline.Properties;
using ContentPipeline.ServiceCollectionExtensions;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.ContentPropertyConverters;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities;
using ContentPipelineSourceGeneratorTests.Utils;
using EPiServer;
using EPiServer.Core;
using EPiServer.Web.Routing;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Globalization;

namespace ContentPipelineSourceGeneratorTests.Tests.Pipelines;

[Trait("Pipelines", "")]
public class ContentPipeline_Given_Vaild_Content
{
    [AutoMock, Theory]
    protected void Should_Map_Properties_For_ContentPage(TestData testData, ContentPageTestData contentPageTestData)
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

        IServiceCollection services = new ServiceCollection();
        (var contentLoader, var urlResolver, var tempDataProvider, var htmlHelper) = testData;
        services
            .AddContentPipelineServices()
            .AddTransient<CustomConverter>()
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

        var contentModel = (ContentPipeline.Models.Awesome.ContentPagePipelineModel)contentPipelineService.ExecutePipeline(pipelineArgs);

        contentModel.Should().NotBeNull();
        contentModel.Title.Should().Be(contentPageTestData.Title);
        contentModel.ListOfStrings.Should().BeEquivalentTo(contentModel.ListOfStrings);
        contentModel.Url?.Should().BeOfType<Link>().Subject.Url.Should().Be($"/{contentPageTestData.Url}");
        contentModel.Link?.Should().BeOfType<Link>().Subject.Url.Should().Be($"/link/{contentPageTestData.LinkId}");
        contentModel.LinkToPage?.Should().BeOfType<Link>().Subject.Url.Should().Be($"/link/{contentPageTestData.PageLinkId}");
        contentModel.CustomMapping.Should().BeTrue();
        contentModel.MediaLink?.Type.Should().Be(nameof(Jpg));
        contentModel.MediaLink?.Url.Should().Be($"/link/{contentPageTestData.MediaLinkId}");
        contentModel.MediaLink?.Properties.Should().BeOfType<JpgPipelineModel>();
        contentModel.MediaLink?.Properties.Should().BeOfType<JpgPipelineModel>().Subject.Title.Should().Be(imageContent.Title);
        contentModel.MediaLink?.Properties.Should().BeOfType<JpgPipelineModel>().Subject.AltText.Should().Be(imageContent.AltText);
        contentModel.MediaLink?.Properties.Should().BeOfType<JpgPipelineModel>().Subject.Copyright.Should().Be(imageContent.Copyright);
        contentModel.BlockLink?.Should().BeOfType<ContentBlockPipelineModel>();
        var contentBlock = contentModel.BlockLink as ContentBlockPipelineModel;
        contentBlock?.Color.Should().Be(blockContent.Color.ToString());
        contentBlock?.Header.Should().Be(blockContent.Header);
        contentBlock?.Text.Should().BeEmpty();

    }

    protected record TestData(IContentLoader ContentLoader, IUrlResolver UrlResolver, ITempDataProvider TempDataProvider, IHtmlHelper HtmlHelper);
    protected record ContentPageTestData(string Title, string Url, int IgnoreLinkId, int LinkId, int PageLinkId, int MediaLinkId, int BlockLinkId, List<string> List);
}
