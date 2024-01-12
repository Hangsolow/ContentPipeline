using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ContentPipeline.SourceGenerator;

internal sealed partial class Parser
{
    private ContentClass GetContentClass(INamedTypeSymbol contentClassSymbol, SemanticModel semanticModel, ClassDeclarationSyntax classDeclaration)
    {
        var (_, _, _, contentType, contentPipelineModel) = ParseAttributes(contentClassSymbol);
        string guid = contentType?.NamedArguments.FirstOrDefault(a => a.Key == "GUID").Value.Value as string ?? Guid.NewGuid().ToString();
        string group = contentPipelineModel?.ConstructorArguments.FirstOrDefault().Value as string ?? "Common";
        if (contentPipelineModel?.ConstructorArguments[1].Value is not int order)
        {
            order = 0;
        }
        var contentProperties = GetContentProperties(contentClassSymbol, semanticModel, classDeclaration).ToArray();

        return new(Name: contentClassSymbol.Name, Guid: guid, Group: group, FullyQualifiedName: contentClassSymbol.ToString(), order, ContentProperties: contentProperties);
    }

    private static ContentPipelineAttributes ParseAttributes(ITypeSymbol contentClassSymbol)
    {
        var contentPipelineAttributes = GetNamedAttributes(contentClassSymbol.GetAttributes());
        
        if (contentPipelineAttributes.ContentPipelineModel is null)
        {
            var type = contentClassSymbol.BaseType;
            AttributeData? contentPipelineModelAttribute = null;
            while (type != null && contentPipelineModelAttribute is null)
            {
                var attributes = type.GetAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute.AttributeClass?.Name == "ContentPipelineModelAttribute")
                    {
                        contentPipelineModelAttribute = attribute;
                        break;
                    }
                }

                type = type.BaseType;
            }

            return contentPipelineAttributes with { ContentPipelineModel = contentPipelineModelAttribute };
        }

        return contentPipelineAttributes;
    }
}
