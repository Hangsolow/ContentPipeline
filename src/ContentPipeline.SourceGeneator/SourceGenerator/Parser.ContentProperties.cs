using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ContentPipeline.SourceGenerator;

internal sealed partial class Parser
{
    internal required string InterfaceNamespace { get; init; }

    private IEnumerable<ContentProperty> GetContentProperties(INamedTypeSymbol contentClassSymbol, SemanticModel semanticModel)
    {
        var propertySymbols = contentClassSymbol.GetMembers()
            .Where(s => s.Kind == SymbolKind.Property && !s.ContainingNamespace.ToString().StartsWith("episerver", StringComparison.OrdinalIgnoreCase))
            .OfType<IPropertySymbol>();

        foreach (var propertySymbol in propertySymbols)
        {
            var contentProperty = GetContentProperty(propertySymbol, semanticModel, InterfaceNamespace);
            if (contentProperty is not null)
            {
                yield return contentProperty;
            }
        }

        static ContentProperty? GetContentProperty(IPropertySymbol propertySymbol, SemanticModel semanticModel, string interfaceNamespace)
        {
            if (propertySymbol.Type is not INamedTypeSymbol namedPropertySymbol)
            {
                return null;
            }
            
            
            var attributes = GetNamedAttributes(propertySymbol.GetAttributes());

            if (attributes.Ignore is not null)
            {
                return null;
            }

            var uiHint = attributes.UiHint?.ConstructorArguments[0].Value as string;

            var (ConverterNamespace, ConverterType) = GetConverter(namedPropertySymbol, attributes.ContentPipelinePropertyConverter, uiHint, interfaceNamespace);

            if (TryGetTypeFromAttribute(attributes.ContentPipelinePropertyConverter, out var propertyType))
            {
                return new(Name: propertySymbol.Name, TypeName: propertyType, ConverterType: ConverterType, ConverterNamespace: ConverterNamespace);
            }

            var isEnumerable = namedPropertySymbol.SpecialType is not SpecialType.System_String && namedPropertySymbol.AllInterfaces.Any(s => s.Name == nameof(IEnumerable)) == true;


            return namedPropertySymbol switch
            {
                //Mapping of ContentReference
                { Name: "ContentReference", NullableAnnotation: var nullableAnnotation } when uiHint is "mediafile" => new(Name: propertySymbol.Name, TypeName: GetTypeName("Media", nullableAnnotation), ConverterType: ConverterType, ConverterNamespace: ConverterNamespace),
                { Name: "ContentReference", NullableAnnotation: var nullableAnnotation } when uiHint is "image" => new(Name: propertySymbol.Name, TypeName: GetTypeName("Media", nullableAnnotation), ConverterType: ConverterType, ConverterNamespace: ConverterNamespace),
                { Name: "ContentReference", NullableAnnotation: var nullableAnnotation } when uiHint is "block" => new(Name: propertySymbol.Name, TypeName: GetTypeName("ContentPipelineModel", nullableAnnotation), ConverterType: ConverterType, ConverterNamespace: ConverterNamespace),
                { Name: "ContentReference", NullableAnnotation: var nullableAnnotation } => new(Name: propertySymbol.Name, TypeName: GetTypeName($"Link", nullableAnnotation), ConverterType: ConverterType, ConverterNamespace: ConverterNamespace),
                //Mapping of ContentAreas
                { Name: "ContentArea", NullableAnnotation: var nullableAnnotation } => new(Name: propertySymbol.Name, TypeName: GetTypeName("ContentAreaPipelineModel", nullableAnnotation), ConverterType: ConverterType, ConverterNamespace: ConverterNamespace),
                //Mapping of richtext properties
                { Name: "XhtmlString", NullableAnnotation: var nullableAnnotation } => new(Name: propertySymbol.Name, TypeName: GetTypeName(nameof(String), nullableAnnotation), ConverterType: ConverterType, ConverterNamespace: ConverterNamespace),
                //mapping of urls
                { Name: "Url", NullableAnnotation: var nullableAnnotation } => new(Name: propertySymbol.Name, TypeName: GetTypeName("Link", nullableAnnotation), ConverterType: ConverterType, ConverterNamespace: ConverterNamespace),
                //Mapping of inline blocks on a page
                { Name: var typeName, BaseType: var baseType, NullableAnnotation: var nullableAnnotation } when string.IsNullOrEmpty(typeName) is false && IsContentBaseType(baseType) => new(Name: propertySymbol.Name, TypeName: GetTypeName("ContentPipelineModel", nullableAnnotation), ConverterType: ConverterType, ConverterNamespace: ConverterNamespace),
                //Mapping of builtin types like string
                { Name: var typeName } when string.IsNullOrEmpty(typeName) is false => new(Name: propertySymbol.Name, TypeName: namedPropertySymbol.ToString(), ConverterType: ConverterType, ConverterNamespace: ConverterNamespace),
                //fallback, should never be hit but compiler gonna compile 
                _ => new(Name: propertySymbol.Name, TypeName: nameof(Object) + "?", ConverterType: ConverterType, ConverterNamespace: ConverterNamespace)
            };
        }

        static bool TryGetTypeFromAttribute(AttributeData? ContentPipelinePropertyConverter, out string propertyType)
        {
            propertyType = string.Empty;
            if (ContentPipelinePropertyConverter?.AttributeClass?.TypeParameters is not null)
            {
                string postfix = ContentPipelinePropertyConverter switch
                {
                    { ConstructorArguments.Length: 1 } a when a.ConstructorArguments[0].Value is byte nullable && (byte)NullableAnnotation.Annotated == nullable => "?",
                    _ => string.Empty
                };

                propertyType = ContentPipelinePropertyConverter.AttributeClass switch
                {
                    { TypeArguments.Length: 2 } attributeClass => attributeClass.TypeArguments[1].ToString() + postfix ?? string.Empty,
                    _ => string.Empty
                };
            }

            return string.IsNullOrEmpty(propertyType) is false;
        }

        static (string ConverterNamespace, string ConverterType) GetConverter(INamedTypeSymbol namedPropertySymbol, AttributeData? ContentPipelinePropertyConverter, string? uiHint, string interfaceNamespace)
        {
            //gets and returns the convertertype from ContentPropertyConverter attribute
            if (ContentPipelinePropertyConverter?.ConstructorArguments.Length >= 2)
            {
                var value = ContentPipelinePropertyConverter.ConstructorArguments[0].Value?.ToString();

                if (value is not null)
                {
                    //converters from attributes does sets ConverterNamespace to empty. 
                    return (string.Empty, value);
                }
            }

            //find default converter
            return namedPropertySymbol switch
            {
                //ContentReference gets special treatment as the ContentReference in itself is almost never what we want in the frontend as a consumer
                { Name: "ContentReference" } when uiHint is "mediafile" => (interfaceNamespace, "IMediaConverter"),
                { Name: "ContentReference" } when uiHint is "image" => (interfaceNamespace, "IMediaConverter"),
                { Name: "ContentReference" } when uiHint is "block" => (interfaceNamespace, "IBlockConverter"),
                { Name: "ContentReference" } => (interfaceNamespace, "IContentReferenceConverter"),

                { Name: "XhtmlString" } => (interfaceNamespace, "IXhtmlStringConverter"),
                //ContentArea needs to be converted to a list of content area items
                { Name: "ContentArea" } => (interfaceNamespace, "IContentAreaConverter"),
                //Url needs to be converted to a link object
                { Name: "Url" } => (interfaceNamespace, "ILinkConverter"),

                //Mapping of inline blocks on a page
                { BaseType: var baseType } when IsContentBaseType(baseType) => (interfaceNamespace, "IEmbeddedBlockConverter"),

                //none in this context means just assign the value from the IContent to the ContentApiModel without a converter for types like string and bool
                _ => (string.Empty, "None")
            };
        }

        

        static bool IsContentBaseType(INamedTypeSymbol? type) => type?.AllInterfaces.Any(i => i.Name.Equals("IContentData")) ?? false;

        static string GetTypeName(string name, NullableAnnotation nullableAnnotation)
        {
            if (NullableAnnotation.Annotated == nullableAnnotation)
            {
                return name + "?";
            }
            return name;
        }
    }

    private static (AttributeData? Ignore, AttributeData? UiHint, AttributeData? ContentPipelinePropertyConverter, AttributeData? ContentType, AttributeData? ContentPipelineModel) GetNamedAttributes(ImmutableArray<AttributeData> attributes)
    {
        AttributeData? ignore = null;
        AttributeData? uiHint = null;
        AttributeData? contentPipelinePropertyConverter = null;
        AttributeData? contentType = null;
        AttributeData? contentPipelineModel = null;
        

        foreach (var attribute in attributes)
        {
            switch (attribute.AttributeClass?.Name)
            {
                case "ContentPipelineIgnoreAttribute":
                    ignore = attribute;
                    break;
                case "UIHintAttribute":
                    uiHint = attribute;
                    break;
                case "ContentPipelinePropertyConverterAttribute":
                    contentPipelinePropertyConverter = attribute;
                    break;
                case ContentTypeAttribute:
                    contentType = attribute;
                    break;
                case ContentPipelineModelAttribute:
                    contentPipelineModel = attribute;
                    break;

            }
        }
        return (ignore, uiHint, contentPipelinePropertyConverter, contentType, contentPipelineModel);
    }
}
