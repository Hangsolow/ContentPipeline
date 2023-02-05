using Microsoft.CodeAnalysis;

namespace ContentPipeline.SourceGenerator;

internal sealed partial class Parser
{
    private ContentClass GetContentClass(INamedTypeSymbol contentClassSymbol, SemanticModel semanticModel)
    {
        var (_, _, contentApiPropertyConverter, contentType, contentPipelineModel) = GetNamedAttributes(contentClassSymbol.GetAttributes());
        string guid = contentType?.NamedArguments.FirstOrDefault(a => a.Key == "GUID").Value.Value as string ?? Guid.NewGuid().ToString();
        string group = contentPipelineModel?.ConstructorArguments.FirstOrDefault().Value as string ?? "Common";
        
        var contentProperties = GetContentProperties(contentClassSymbol, semanticModel).ToArray();

        return new(Name: contentClassSymbol.Name, Guid: guid , Group: group, FullyQualifiedName: contentClassSymbol.ToString(), ContentProperties: contentProperties);
    }
}
