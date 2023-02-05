namespace ContentPipeline.SourceGenerator;

internal sealed partial class Emitter
{
    internal IEnumerable<CodeSource> GetPropertiesSourceFiles()
    {
        CancellationToken.ThrowIfCancellationRequested();

        yield return new("Link.g.cs", CreatePropertySource("Link"));
        yield return new("Media.g.cs", CreatePropertySource("Media"));
        yield return new("ContentAreaPipelineModel.g.cs", CreatePropertySource("ContentAreaPipelineModel"));

        string CreatePropertySource(string name)
        {
            return
                $$"""
                #nullable enable
                namespace {{SharedNamespace}}.Properties;

                public sealed partial class {{name}}
                {

                }
                """;
        }
    }
}
