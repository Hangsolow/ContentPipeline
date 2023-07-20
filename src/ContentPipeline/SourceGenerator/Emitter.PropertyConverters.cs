namespace ContentPipeline.SourceGenerator;

internal partial class Emitter
{
    internal IEnumerable<CodeSource> GetContentPropertyConverters()
    {
        yield return new("EnumConverter.g.cs", CreateEnumConverter());
        yield return new("BlockPropertyConverter.g.cs", CreateBlockConverter());
        yield return new("ContentReferenceConverter.g.cs", CreateContentReferenceConverter());
        yield return new("InlineBlockConverter.g.cs", CreateInlineBlockConverter());
        yield return new("LinkConverter.g.cs", CreateLinkConverter());
        yield return new("MediaConverter.g.cs", CreateMediaConverter());
        yield return new("XhtmlStringConverter.g.cs", CreateXhtmlStringConverter());
        yield return new("ContentAreaConverter.g.cs", CreateContentAreaConverter());

        string CreateEnumConverter() =>
            $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Converters;
                
                using {{SharedNamespace}}.Interfaces;
                using EPiServer.Core;

                public class EnumConverter<TEnum> : IEnumConverter<TEnum>
                    where TEnum : Enum
                {
                    public string? GetValue(TEnum property, IContentData content, string propertyName, IContentPipelineContext pipelineContext)
                    {
                        return property.ToString();
                    }
                }
                """;

        string CreateBlockConverter() =>
            $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Converters;
                
                using {{SharedNamespace}}.Interfaces;
                using EPiServer;
                using EPiServer.Core;

                internal class BlockConverter : IBlockConverter
                {
                    public BlockConverter(IContentLoader contentLoader)
                    {
                        ContentLoader = contentLoader;
                    }

                    private IContentLoader ContentLoader { get; }

                    public IContentPipelineModel? GetValue(ContentReference? property, IContentData content, string propertyName, IContentPipelineContext pipelineContext)
                    {
                        if (ContentLoader.TryGet(property, pipelineContext.Language, out IContentData blockContent))
                        {
                            return pipelineContext.ContentPipelineService.ExecutePipeline(blockContent, pipelineContext);
                        }
                        return null;

                    }
                }
                """;

        string CreateContentReferenceConverter() =>
            $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Converters;
                
                using {{SharedNamespace}}.Interfaces;
                using {{SharedNamespace}}.Properties;
                using EPiServer;
                using EPiServer.Core;
                using EPiServer.Web.Routing;

                internal sealed class ContentReferenceConverter : IContentReferenceConverter
                {

                    public ContentReferenceConverter(IUrlResolver urlResolver)
                    {
                        UrlResolver = urlResolver;
                    }

                    private IUrlResolver UrlResolver { get; }


                    public Link GetValue(ContentReference? property, IContentData content, string propertyName, IContentPipelineContext pipelineContext)
                    {
                        return new Link
                        {
                            Url = UrlResolver.GetUrl(property, pipelineContext.Language?.Name)
                        };
                    }
                }
                """;

        string CreateInlineBlockConverter() =>
            $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Converters;
                
                using {{SharedNamespace}}.Interfaces;
                using EPiServer;
                using EPiServer.Core;
                
                internal sealed class InlineBlockConverter : IEmbeddedBlockConverter
                {
                             
                    public IContentPipelineModel? GetValue(BlockData? property, IContentData content, string propertyName, IContentPipelineContext pipelineContext)
                    {
                        if (property is not null)
                        {
                            return pipelineContext.ContentPipelineService.ExecutePipeline(property, pipelineContext);
                        }

                        return null;
                    }
                }
                """;

        string CreateLinkConverter() =>
            $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Converters;
                
                using {{SharedNamespace}}.Interfaces;
                using {{SharedNamespace}}.Properties;
                using EPiServer;
                using EPiServer.Core;
                using EPiServer.Web.Routing;
                
                internal sealed class LinkConverter : ILinkConverter
                {
                
                    public LinkConverter(IUrlResolver urlResolver)
                    {
                        UrlResolver = urlResolver;
                    }
                
                    private IUrlResolver UrlResolver { get; }
                
                    
                    public ILinkPipelineModel GetValue(Url? property, IContentData content, string propertyName, IContentPipelineContext pipelineContext)
                    {

                        if (property is null)
                        {
                            return new Link();
                        }

                        string? friendlyUrl = null;

                        if (property.IsAbsoluteUri is false)
                        {
                            var urlContent = UrlResolver.Route(new UrlBuilder(property.ToString()));
                            if (ContentReference.IsNullOrEmpty(urlContent?.ContentLink) is false)
                            {
                                friendlyUrl = UrlResolver.GetUrl(urlContent?.ContentLink, pipelineContext.Language?.Name);
                            }
                        }

                        if (string.IsNullOrEmpty(friendlyUrl))
                        {
                            friendlyUrl = property.ToString();
                        }

                        return new Link
                        {
                            Url = friendlyUrl
                        };
                    }
                }
                """;

        string CreateMediaConverter() =>
            $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Converters;
                
                using {{SharedNamespace}}.Interfaces;
                using {{SharedNamespace}}.Properties;
                using EPiServer;
                using EPiServer.Core;
                using EPiServer.Web.Routing;

                internal sealed class MediaConverter : IMediaConverter
                {
                    public MediaConverter(IUrlResolver urlResolver, IContentLoader contentLoader)
                    {
                        UrlResolver = urlResolver ?? throw new ArgumentNullException(nameof(urlResolver));
                        ContentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
                    }

                    private IUrlResolver UrlResolver { get; }
                    private IContentLoader ContentLoader { get; }

                    public Media GetValue(ContentReference? property, IContentData content, string propertyName, IContentPipelineContext pipelineContext)
                    {
                        if (ContentLoader.TryGet(property, out IContent media))
                        {
                            var imageProperties = pipelineContext.ContentPipelineService.ExecutePipeline(media, pipelineContext);

                            string type = media.GetOriginalType().Name;

                            return new()
                            {
                                Url = UrlResolver.GetUrl(media.ContentLink, pipelineContext.Language?.Name),
                                Type = type,
                                Properties = imageProperties
                            };
                        }

                        return new();
                    }
                }
                """;

        string CreateXhtmlStringConverter() =>
            $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Converters;
                
                using {{SharedNamespace}}.Interfaces;
                using {{SharedNamespace}}.Properties;
                using EPiServer.Core;

                public class XhtmlStringConverter : IXhtmlStringConverter
                {
                    public XhtmlStringConverter(IXhtmlRenderService xhtmlRenderService)
                    {
                        XhtmlRenderService = xhtmlRenderService;
                    }

                    private IXhtmlRenderService XhtmlRenderService { get; }

                    public string GetValue(XhtmlString? property, IContentData content, string propertyName, IContentPipelineContext pipelineContext)
                    {
                        return XhtmlRenderService.RenderXhtmlString(pipelineContext.HttpContext, property);
                    }
                }
                """;

        string CreateContentAreaConverter() =>
            $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Converters;

                using {{SharedNamespace}}.Interfaces;
                using {{SharedNamespace}}.Properties;
                using EPiServer;
                using EPiServer.Core;
                using EPiServer.Core.Html.StringParsing;
                using System;
                using System.Collections.Generic;
                using System.Linq;

                internal class ContentAreaConverter : IContentAreaConverter
                {
                    public ContentAreaConverter(IContentLoader contentLoader)
                    {
                        ContentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
                    }

                    private IContentLoader ContentLoader { get; }

                    public ContentAreaPipelineModel? GetValue(ContentArea? property, IContentData content, string propertyName, IContentPipelineContext pipelineContext)
                    {
                        return new ContentAreaPipelineModel
                        {
                            Items = GetContentAreaItems(property, pipelineContext, ContentLoader)
                        };

                        static IEnumerable<ContentAreaItemPipelineModel> GetContentAreaItems(ContentArea? contentArea, IContentPipelineContext pipelineContext, IContentLoader contentLoader)
                        {
                            foreach (var item in contentArea?.FilteredItems ?? Enumerable.Empty<ContentAreaItem>())
                            {
                                var model = new ContentAreaItemPipelineModel();
                                
                                if (item.RenderSettings.TryGetValue(ContentFragment.ContentDisplayOptionAttributeName, out var displayOption))
                                {
                                    model.DisplayOption = displayOption.ToString();
                                }

                                if (contentLoader.TryGet(item.ContentLink, out IContent modelContent))
                                {
                                    model.Content = pipelineContext.ContentPipelineService.ExecutePipeline(modelContent, pipelineContext);
                                }

                                yield return model;
                            }
                        }
                    }
                }
                """;
    }
}
