namespace ContentPipeline.SourceGenerator;

internal sealed partial class Emitter
{
    internal IEnumerable<CodeSource> GetInterfaceSources()
    {
        CancellationToken.ThrowIfCancellationRequested();

        yield return new("IBlockConverter.g.cs", CreateInterface("IBlockConverter", "EPiServer.Core.ContentReference?", $"{SharedNamespace}.Models.ContentPipelineModel?"));
        yield return new("IEmbeddedBlockConverter.g.cs", CreateInterface("IEmbeddedBlockConverter", "EPiServer.Core.BlockData?", $"{SharedNamespace}.Models.ContentPipelineModel?"));
        yield return new("IContentReferenceConverter.g.cs", CreateInterface("IContentReferenceConverter", "EPiServer.Core.ContentReference?", $"{SharedNamespace}.Properties.Link"));
        yield return new("IContentAreaConverter.g.cs", CreateInterface("IContentAreaConverter", "EPiServer.Core.ContentArea?", $"{SharedNamespace}.Properties.ContentAreaPipelineModel?"));
        yield return new("ILinkConverter.g.cs", CreateInterface("ILinkConverter", "EPiServer.Url?", $"{SharedNamespace}.Properties.Link?"));
        yield return new("IMediaConverter.g.cs", CreateInterface("IMediaConverter", "EPiServer.Core.ContentReference?", $"{SharedNamespace}.Properties.Media?"));
        yield return new("IXhtmlStringConverter.g.cs", CreateInterface("IXhtmlStringConverter", "EPiServer.Core.ContentReference?", "string"));

        yield return new("IContentPropertyConverter.g.cs", CreatePropertyConverterSource());
        yield return new("IContentConverterPipelineContext.g.cs", CreateConverterPipelineContextSource());
        yield return new("IContentConverterPipelineStep.g.cs", CreateConverterPipelineStep());

        string CreateInterface(string name, string typeProperty, string typeValue)
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

        string CreatePropertyConverterSource()
        {
            return
                $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Interfaces;
                
                using EPiServer.Core;

                public partial interface IContentPropertyConverter<TProperty, out TValue>
                {
                    TValue GetValue(TProperty property, IContentData content, string propertyName, IContentConverterPipelineContext pipelineContext);
                }
                """;
        }

        string CreateConverterPipelineContextSource()
        {
            return
                $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Interfaces;

                using Microsoft.AspNetCore.Http;

                public partial interface IContentConverterPipelineContext
                {
                    HttpContext HttpContext { get; }
                }
                """;
        }

        string CreateConverterPipelineStep()
        {
            return
                $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Interfaces;
                
                using EPiServer.Core;

                public partial interface IContentConverterPipelineStep<TContent, TContentPipelineModel>
                    where TContent : IContentData
                    where TContentPipelineModel : {{SharedNamespace}}.Models.ContentPipelineModel
                {
                    int Order { get; }
                    
                    void Execute(TContent content, TContentPipelineModel contentPipelineModel, {{SharedNamespace}}.Interfaces.IContentConverterPipelineContext pipelineContext);
                }
                """;
        }
    }
}


