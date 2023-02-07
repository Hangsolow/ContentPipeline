using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ContentPipeline.SourceGenerator;

internal sealed partial class Parser
{
    private const string ContentPipelineModelAttribute = "ContentPipelineModel";
    private const string ContentTypeAttribute = "ContentType";
    /// <summary>
    /// The current Compilation
    /// </summary>
    internal required Compilation Compilation { get; init; }

    /// <summary>
    /// CancellationToken for the parser
    /// </summary>
    internal required CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// Report Diagnostic action for the parser
    /// </summary>
    internal required Action<Diagnostic> ReportDiagnostic { get; init; }

    internal IReadOnlyList<ContentClass> GetContentClasses(IEnumerable<ClassDeclarationSyntax> classes)
    {
        var results = new List<ContentClass>(classes.Count());
        foreach (var group in classes.GroupBy(x => x.SyntaxTree))
        {
            SemanticModel? semanticModel = null;
            foreach (var classDeclaration in group)
            {
                CancellationToken.ThrowIfCancellationRequested();
                semanticModel ??= Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var contentClassSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

                if (contentClassSymbol is INamedTypeSymbol namedTypeSymbol)
                {
                    results.Add(GetContentClass(namedTypeSymbol, semanticModel, classDeclaration));
                }
            }
        }

        return results;
    }

    /// <summary>
    /// checks if the node should be used for generation of code
    /// </summary>
    /// <param name="node"></param>
    /// <returns>true if node is an class with attributes else false</returns>
    internal static bool IsSyntaxTargetForGeneration(SyntaxNode node) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    /// <summary>
    /// Finds ClassDeclarationSyntax that have the ContentType attribute
    /// </summary>
    /// <param name="context"></param>
    /// <returns>the ClassDeclarationSyntax if it contains ContentPipelineModel attribute else null</returns>
    internal static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        foreach (var attributeListSyntax in classDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (attributeSyntax.Name.ToString() == ContentTypeAttribute && HasContentPipelineAttribute(context, classDeclarationSyntax))
                {
                    return classDeclarationSyntax;
                }
            }

        }

        return null;

        static bool HasContentPipelineAttribute(GeneratorSyntaxContext context, ClassDeclarationSyntax classDeclarationSyntax)
        {
            foreach (var attributeListSyntax in classDeclarationSyntax.AttributeLists)
            {
                foreach (var attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (attributeSyntax.Name.ToString() == ContentPipelineModelAttribute)
                    {
                        return true;
                    }
                }
            }

            //search for the Attribute in base classes
            var type = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax)?.BaseType;

            while (type != null)
            {
                var attributes = type.GetAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute.AttributeClass?.Name == "ContentPipelineModelAttribute")
                    {
                        return true;
                    }
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}
