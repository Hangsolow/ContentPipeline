using ContentPipeline.Interfaces;
using ContentPipeline.Entities;
using ContentPipeline.Models;
using ContentPipeline.Properties;
using ContentPipeline.ServiceCollectionExtensions;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ContentPipelineSourceGeneratorTests.Tests.Pipelines;

[Trait("Pipelines", "")]
public class ContentAreaConverter_Given_ContentArea
{
    [Fact]
    public void Should_Filter_Items_Using_ContentAreaItemsRenderingFilters()
    {
        var services = new ServiceCollection();
        var contentLoader = Substitute.For<IContentLoader>();

        services
            .AddContentPipelineServices()
            .AddSingleton(contentLoader)
            .AddSingleton<IContentAreaItemsRenderingFilter>(new ExcludeItemFilter(2));

        using var serviceProvider = services.BuildServiceProvider();
        var converter = serviceProvider.GetRequiredService<IContentAreaConverter>();

        var contentArea = new ContentArea();
        contentArea.Items.Add(new ContentAreaItem { ContentLink = new ContentReference(1) });
        contentArea.Items.Add(new ContentAreaItem { ContentLink = new ContentReference(2) });
        contentArea.Items[0].RenderSettings[ContentFragment.ContentDisplayOptionAttributeName] = "wide";

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
        var pipelineContext = Substitute.For<IContentPipelineContext>();
        pipelineContext.HttpContext.Returns(httpContext);

        var result = converter.GetValue(contentArea, Substitute.For<IContentData>(), nameof(contentArea), pipelineContext);

        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        var items = result.Items!.ToList();
        Assert.Single(items);
        Assert.Equal("wide", items[0].DisplayOption);
    }

    [Fact]
    public async Task Should_Use_Async_Pipeline_Execution_In_GetValueAsync()
    {
        var services = new ServiceCollection();
        var contentLoader = Substitute.For<IContentLoader>();
        var pipelineService = new TestContentPipelineService();

        services
            .AddContentPipelineServices()
            .AddSingleton(contentLoader);

        using var serviceProvider = services.BuildServiceProvider();
        var converter = serviceProvider.GetRequiredService<IContentAreaConverter>();
        var asyncConverter = (IAsyncContentPropertyConverter<ContentArea?, ContentAreaPipelineModel?>)converter;

        var contentArea = new ContentArea();
        contentArea.Items.Add(new ContentAreaItem { ContentLink = new ContentReference(3) });
        var modelContent = Substitute.For<IContent>();
        contentLoader.TryGet(Arg.Any<ContentReference>(), out Arg.Any<IContent>()).Returns(callInfo =>
        {
            callInfo[1] = modelContent;
            return true;
        });

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
        var pipelineContext = Substitute.For<IContentPipelineContext>();
        pipelineContext.HttpContext.Returns(httpContext);
        pipelineContext.ContentPipelineService.Returns(pipelineService);

        var result = await asyncConverter.GetValueAsync(contentArea, Substitute.For<IContentData>(), nameof(contentArea), pipelineContext);

        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Single(result.Items!);
        Assert.Equal(1, pipelineService.AsyncExecuteCalls);
        Assert.Equal(0, pipelineService.SyncExecuteCalls);
    }

    private sealed class ExcludeItemFilter : IContentAreaItemsRenderingFilter
    {
        private readonly int _excludedId;

        public ExcludeItemFilter(int excludedId)
        {
            _excludedId = excludedId;
        }

        public ValueTask FilterAsync(IList<ContentAreaItem> items, IPrincipal principal, ContextMode mode)
        {
            for (var i = items.Count - 1; i >= 0; i--)
            {
                if (items[i].ContentLink.ID == _excludedId)
                {
                    items.RemoveAt(i);
                }
            }

            return ValueTask.CompletedTask;
        }
    }

    private sealed class TestContentPipelineService : IContentPipelineService
    {
        public int SyncExecuteCalls { get; private set; }

        public int AsyncExecuteCalls { get; private set; }

        public IContentPipelineModel ExecutePipeline(IContentData content, IContentPipelineContext pipelineContext)
        {
            SyncExecuteCalls++;
            throw new InvalidOperationException("Sync pipeline should not be used by GetValueAsync.");
        }

        public IContentPipelineModel ExecutePipeline(PipelineArgs pipelineArgs)
        {
            SyncExecuteCalls++;
            throw new InvalidOperationException("Sync pipeline should not be used by GetValueAsync.");
        }

        public Task<IContentPipelineModel> ExecutePipelineAsync(IContentData content, IContentPipelineContext pipelineContext)
        {
            AsyncExecuteCalls++;
            return Task.FromResult<IContentPipelineModel>(new ContentPipelineModel());
        }

        public Task<IContentPipelineModel> ExecutePipelineAsync(PipelineArgs pipelineArgs)
        {
            AsyncExecuteCalls++;
            return Task.FromResult<IContentPipelineModel>(new ContentPipelineModel());
        }
    }
}
