namespace ContentPipeline.SourceGenerator;

internal sealed partial class Emitter
{
    internal IEnumerable<CodeSource> GetPropertiesSourceFiles()
    {
        CancellationToken.ThrowIfCancellationRequested();

        yield return new("Link.g.cs", CreateLink());
        yield return new("Media.g.cs", CreateMedia());
        yield return new("ContentAreaItemPipelineModel.g.cs", CreateContentAreaItemPipelineModel());
        yield return new("ContentAreaPipelineModel.g.cs", CreateContentAreaPipelineModel());

        string CreateLink() =>
            $$"""
            #nullable enable
            namespace {{SharedNamespace}}.Properties;
            
            public sealed partial class Link : {{SharedNamespace}}.Interfaces.ILinkPipelineModel
            {
                public string? Url { get; set; }
            }
            """;

        string CreateMedia() =>
            $$"""
            #nullable enable
            namespace {{SharedNamespace}}.Properties;
            
            using {{SharedNamespace}}.Interfaces;

            public sealed partial class Media
            {
                public string? Url { get; set; }

                public string? Type { get; set; }

                public IContentPipelineModel? Properties { get; set; }

            }
            """;

        string CreateContentAreaItemPipelineModel() =>
            $$"""
            #nullable enable
            namespace {{SharedNamespace}}.Properties;

            using {{SharedNamespace}}.Interfaces;

            public sealed partial class ContentAreaItemPipelineModel
            {
                public string? DisplayOption { get; set; }

                public IContentPipelineModel? Content { get; set; }
            }
            """;

        string CreateContentAreaPipelineModel() =>
            $$"""
            #nullable enable
            namespace {{SharedNamespace}}.Properties;

            using System.Collections.Generic;

            public sealed partial class ContentAreaPipelineModel
            {
                public IEnumerable<ContentAreaItemPipelineModel>? Items { get; set; }
            }
            """;
    }
}
