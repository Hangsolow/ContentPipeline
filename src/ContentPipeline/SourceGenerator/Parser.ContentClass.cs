using Microsoft.CodeAnalysis;

namespace ContentPipeline.SourceGenerator;

internal sealed partial class Parser
{
    private ContentClass GetContentClass(INamedTypeSymbol contentClassSymbol, SemanticModel semanticModel)
    {
        var (Ignore, UiHint, ContentApiPropertyConverter, ContentType) = GetNamedAttributes(contentClassSymbol.GetAttributes());
        var attribute = contentClassSymbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass?.Name == nameof(ContentTypeAttribute));
        string guid = ContentType?.NamedArguments.FirstOrDefault(a => a.Key == "GUID").Value.Value as string ?? Guid.NewGuid().ToString();
        string? group = ContentApiPropertyConverter?.NamedArguments.FirstOrDefault(a => a.Key == "Group").Value.Value as string ?? guid.Substring(0, 8);
        
        var contentProperties = GetContentProperties(contentClassSymbol, semanticModel).ToArray();

        return new(Name: contentClassSymbol.Name, Guid: guid , FullyQualifiedName: contentClassSymbol.ToString(), ContentProperties: contentProperties);
    }
}
