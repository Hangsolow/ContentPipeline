﻿namespace ContentPipeline.SourceGenerator;

internal sealed partial class Emitter
{
    internal IEnumerable<CodeSource> GetInterfaceSources()
    {
        CancellationToken.ThrowIfCancellationRequested();

        yield return new("IBlockConverter.g.cs", CreatePropertyConverterInterface("IBlockConverter", "EPiServer.Core.ContentReference?", $"{SharedNamespace}.Interfaces.IContentPipelineModel?"));
        yield return new("IEmbeddedBlockConverter.g.cs", CreatePropertyConverterInterface("IEmbeddedBlockConverter", "EPiServer.Core.BlockData?", $"{SharedNamespace}.Interfaces.IContentPipelineModel?"));
        yield return new("IContentReferenceConverter.g.cs", CreatePropertyConverterInterface("IContentReferenceConverter", "EPiServer.Core.ContentReference?", $"{SharedNamespace}.Properties.Link"));
        yield return new("IContentAreaConverter.g.cs", CreatePropertyConverterInterface("IContentAreaConverter", "EPiServer.Core.ContentArea?", $"{SharedNamespace}.Properties.ContentAreaPipelineModel?"));
        yield return new("ILinkConverter.g.cs", CreatePropertyConverterInterface("ILinkConverter", "EPiServer.Url?", $"{SharedNamespace}.Interfaces.ILinkPipelineModel?"));
        yield return new("IMediaConverter.g.cs", CreatePropertyConverterInterface("IMediaConverter", "EPiServer.Core.ContentReference?", $"{SharedNamespace}.Properties.Media?"));
        yield return new("IEnumConverter.g.cs", CreateEnumConverter());
        yield return new("IXhtmlStringConverter.g.cs", CreatePropertyConverterInterface("IXhtmlStringConverter", "EPiServer.Core.XhtmlString?", "string"));
        yield return new("IXhtmlRenderService.g.cs", CreateXhtmlRenderService());

        yield return new("IContentPropertyConverter.g.cs", CreateGenericPropertyConverterSource());
        yield return new("IContentPipelinePropertyConverterAttribute.g.cs", CreateContentPipelinePropertyConverterAttributeInterface());
        yield return new("IContentPipelineContext.g.cs", CreatePipelineContextSource());
        yield return new("IContentPipelineStep.g.cs", CreatePipelineStep());
        yield return new("IContentPipelineService.g.cs", CreatePipelineService());
        yield return new("IContentPipeline.g.cs", CreateContentPipeline());
        yield return new("IContentPipelineModel.g.cs", CreateContentPipelineModel());
        yield return new("ILinkPipelineModel.g.cs", CreateLinkPipelineModel());

        string CreatePropertyConverterInterface(string name, string typeProperty, string typeValue)
        {
            return
                $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Interfaces;

                using EPiServer.Core;
                
                internal partial interface {{name}} : IContentPropertyConverter<{{typeProperty}}, {{typeValue}}>
                {
                
                }
                """;
        }

        string CreateEnumConverter() =>
                $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Interfaces;

                using EPiServer.Core;
                
                internal partial interface IEnumConverter<TEnum> : IContentPropertyConverter<TEnum, string?>
                    where TEnum : Enum
                {
                
                }
                """;

        string CreateGenericPropertyConverterSource()
        {
            return
                $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Interfaces;
                
                using EPiServer.Core;

                /// <summary>
                /// Marker interface for the generic version
                /// </summary>
                public interface IContentPropertyConverter
                {
                }

                public partial interface IContentPropertyConverter<TProperty, out TValue> : IContentPropertyConverter
                {
                    TValue GetValue(TProperty property, IContentData content, string propertyName, IContentPipelineContext pipelineContext, Dictionary<string, string>? config = null);
                }
                """;
        }

        string CreatePipelineContextSource()
        {
            return
                $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Interfaces;

                using Microsoft.AspNetCore.Http;
                using System.Globalization;

                public partial interface IContentPipelineContext
                {
                    HttpContext HttpContext { get; }
                    
                    IContentPipelineService ContentPipelineService { get; }
                    
                    CultureInfo? Language { get; }
                }
                """;
        }

        string CreatePipelineStep()
        {
            return
                $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Interfaces;
                
                using EPiServer.Core;

                public partial interface IContentPipelineStep<in TContent, in TContentPipelineModel>
                    where TContent : IContentData
                    where TContentPipelineModel : {{SharedNamespace}}.Interfaces.IContentPipelineModel
                {
                    /// <summary>
                    /// The order for the pipeline step, the sort order goes from low to high
                    /// </summary>
                    int Order { get; }
                    
                    /// <summary>
                    /// Runs the pipeline step
                    /// </summary>
                    /// <param name="content"></param>
                    /// <param name="contentPipelineModel"></param>
                    /// <param name="pipelineContext"></param>
                    void Execute(TContent content, TContentPipelineModel contentPipelineModel, {{SharedNamespace}}.Interfaces.IContentPipelineContext pipelineContext);
                }
                """;
        }

        string CreatePipelineService()
        {
            return
                $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Interfaces;

                using EPiServer.Core;
                using {{SharedNamespace}}.Entities;

                /// <summary>
                /// service for running a pipeline for a given content
                /// </summary>
                public interface IContentPipelineService
                {
                    /// <summary>
                    /// runs the pipeline for the given content
                    /// </summary>
                    /// <param name="content"></param>
                    /// <param name="pipelineContext"></param>
                    /// <returns></returns>
                    IContentPipelineModel ExecutePipeline(IContentData content, IContentPipelineContext pipelineContext);
    
                    /// <summary>
                    /// runs the pipeline for the given pipeline args
                    /// </summary>
                    /// <param name="pipelineArgs"></param>
                    /// <returns></returns>
                    IContentPipelineModel ExecutePipeline(PipelineArgs pipelineArgs);
                }
                """;
        }

        string CreateContentPipeline() =>
            $$"""
            #nullable enable
            using EPiServer.Core;

            namespace {{SharedNamespace}}.Interfaces;

            public interface IContentPipeline<TContent, TPipelineModel>
                where TContent : IContentData
                where TPipelineModel : IContentPipelineModel, new()
            {
                /// <summary>
                /// Runs the pipeline for the content
                /// </summary>
                /// <param name="content"></param>
                /// <param name="pipelineContext"></param>
                /// <returns>The Content Pipeline Model</returns>
                TPipelineModel Run(TContent content, IContentPipelineContext pipelineContext);
            }
            """;

        string CreateContentPipelineModel() =>
            $$"""
            #nullable enable
            namespace {{SharedNamespace}}.Interfaces;

            /// <summary>
            /// Marker interface for content pipeline models
            /// </summary>
            public interface IContentPipelineModel
            {

            }
            """;

        string CreateLinkPipelineModel() =>
            $$"""
            #nullable enable
            namespace {{SharedNamespace}}.Interfaces;

            /// <summary>
            /// Marker interface for Link pipeline models
            /// </summary>
            public interface ILinkPipelineModel
            {

            }
            """;

        string CreateXhtmlRenderService() =>
            $$"""
            #nullable enable
            namespace {{SharedNamespace}}.Interfaces;

            using EPiServer.Core;
            using Microsoft.AspNetCore.Http;

            public interface IXhtmlRenderService
            {
                /// <summary>
                /// Render a XhtmlString to a string
                /// </summary>
                /// <param name="context"></param>
                /// <param name="xhtmlString"></param>
                /// <returns>An string containg the html from the xhtmlString field</returns>
                string RenderXhtmlString(HttpContext? context, XhtmlString? xhtmlString);
            }

            """;

        string CreateContentPipelinePropertyConverterAttributeInterface() =>
            $$"""
            #nullable enable
            namespace {{SharedNamespace}}.Interfaces;

            /// <summary>
            /// Marker interface for custom ContentPropertyConverter attributes
            /// </summary>
            public interface IContentPipelinePropertyConverterAttribute<TConverter>
                where TConverter : IContentPropertyConverter
            { }

            """;
    }

    internal IEnumerable<CodeSource> GetGroupInterfaceSources(IEnumerable<string> groups)
    {
        foreach (var group in groups)
        {
            yield return new CodeSource($"I{group}ContentPipelineModel.g.cs", CreateGroupContentPipelineModelInterface(group));
        }
        
        string CreateGroupContentPipelineModelInterface(string group) =>
            $$"""
              #nullable enable
              namespace {{SharedNamespace}}.Interfaces;

              /// <summary>
              /// Marker interface for content pipeline models in the {{group}} group
              /// </summary>
              public interface I{{group}}ContentPipelineModel : IContentPipelineModel
              {
              }
              """;
    }
}