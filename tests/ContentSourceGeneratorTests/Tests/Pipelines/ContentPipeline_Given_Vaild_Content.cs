using ContentPipelineSourceGeneratorTests.Utils;
using EPiServer.Web.Routing;
using EPiServer;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.ContentPropertyConverters;
using ContentPipeline.Interfaces;
using ContentPipeline.ServiceCollectionExtensions;
using Microsoft.AspNetCore.Http;
using ContentPipeline.Entities;
using NSubstitute;
using FluentAssertions;
using ContentPipeline.Models;
using EPiServer.Core;
using ContentPipeline.Models.Common;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using EPiServer.Web.Mvc.Html;

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
            Link = new EPiServer.Core.ContentReference(contentPageTestData.LinkId),
            MediaLink = new EPiServer.Core.ContentReference(contentPageTestData.MediaLinkId),
            BlockLink = new EPiServer.Core.ContentReference(contentPageTestData.BlockLinkId),
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
        contentModel.Url?.Url.Should().Be($"/{contentPageTestData.Url}");
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
    protected record ContentPageTestData(string Title, string Url, int LinkId, int MediaLinkId, int BlockLinkId, List<string> List);
}
