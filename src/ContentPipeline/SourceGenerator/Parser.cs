using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ContentPipeline.SourceGenerator;

internal sealed partial class Parser
{
    private const string ContentPipelineModelAttribute = "ContentPipelineModel";
    private const string ContentTypeAttribute = "ContentType";

    internal static bool IsContentClassSyntexForGeneration(SemanticModel semanticModel, ClassDeclarationSyntax targetNode)
    {
        return HasContentPipelineAttribute(semanticModel, targetNode);

        static bool HasContentPipelineAttribute(SemanticModel semanticModel, ClassDeclarationSyntax classDeclarationSyntax)
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
            var type = semanticModel.GetDeclaredSymbol(classDeclarationSyntax)?.BaseType;

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

internal record ContentPipelineAttributes(AttributeData? Ignore, AttributeData? UiHint,
    AttributeData? ContentPipelinePropertyConverter, AttributeData? ContentType, AttributeData? ContentPipelineModel);
