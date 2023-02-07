using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ContentPipeline.SourceGenerator;

internal sealed partial class Parser
{
    private ContentClass GetContentClass(INamedTypeSymbol contentClassSymbol, SemanticModel semanticModel, ClassDeclarationSyntax classDeclaration)
    {
        var (_, _, contentApiPropertyConverter, contentType, contentPipelineModel) = GetNamedAttributes(contentClassSymbol.GetAttributes());
        string guid = contentType?.NamedArguments.FirstOrDefault(a => a.Key == "GUID").Value.Value as string ?? Guid.NewGuid().ToString();
        string group = contentPipelineModel?.ConstructorArguments.FirstOrDefault().Value as string ?? "Common";
        
        var contentProperties = GetContentProperties(contentClassSymbol, semanticModel, classDeclaration).ToArray();

        return new(Name: contentClassSymbol.Name, Guid: guid , Group: group, FullyQualifiedName: contentClassSymbol.ToString(), ContentProperties: contentProperties);
    }
}
