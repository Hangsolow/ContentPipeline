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
            var contentProperty = GetContentProperty(propertySymbol, ReportDiagnostic, InterfaceNamespace);
            if (contentProperty is not null)
            {
                yield return contentProperty;
            }
        }

        static ContentProperty? GetContentProperty(IPropertySymbol propertySymbol, Action<Diagnostic> reportDiagnostic, string interfaceNamespace)
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

            var converterType = GetConverter(namedPropertySymbol, attributes.ContentPipelinePropertyConverter, uiHint);

            if (TryGetTypeFromAttribute(attributes.ContentPipelinePropertyConverter, reportDiagnostic, out var propertyType))
            {
                return new(Name: propertySymbol.Name, TypeName: propertyType, ConverterType: converterType);
            }

            return namedPropertySymbol switch
            {
                //Mapping of ContentReference
                { Name: "ContentReference", NullableAnnotation: var nullableAnnotation } when uiHint is "mediafile" => new(Name: propertySymbol.Name, TypeName: GetTypeName("Media", nullableAnnotation), ConverterType: converterType),
                { Name: "ContentReference", NullableAnnotation: var nullableAnnotation } when uiHint is "image" => new(Name: propertySymbol.Name, TypeName: GetTypeName("Media", nullableAnnotation), ConverterType: converterType),
                { Name: "ContentReference", NullableAnnotation: var nullableAnnotation } when uiHint is "block" => new(Name: propertySymbol.Name, TypeName: GetTypeName("IContentPipelineModel", nullableAnnotation), ConverterType: converterType),
                { Name: "ContentReference", NullableAnnotation: var nullableAnnotation } => new(Name: propertySymbol.Name, TypeName: GetTypeName($"Link", nullableAnnotation), ConverterType: converterType),
                //Mapping of ContentAreas
                { Name: "ContentArea", NullableAnnotation: var nullableAnnotation } => new(Name: propertySymbol.Name, TypeName: GetTypeName("ContentAreaPipelineModel", nullableAnnotation), ConverterType: converterType),
                //Mapping of richtext properties
                { Name: "XhtmlString", NullableAnnotation: var nullableAnnotation } => new(Name: propertySymbol.Name, TypeName: GetTypeName(nameof(String), nullableAnnotation), ConverterType: converterType),
                //mapping of urls
                { Name: "Url", NullableAnnotation: var nullableAnnotation } => new(Name: propertySymbol.Name, TypeName: GetTypeName("Link", nullableAnnotation), ConverterType: converterType),
                //mapping of enums
                { TypeKind: TypeKind.Enum } => new(Name: propertySymbol.Name, TypeName: "string?", ConverterType: converterType),
                //Mapping of inline blocks on a page
                { Name: var typeName, BaseType: var baseType, NullableAnnotation: var nullableAnnotation } when string.IsNullOrEmpty(typeName) is false && IsContentBaseType(baseType) => new(Name: propertySymbol.Name, TypeName: GetTypeName("IContentPipelineModel", nullableAnnotation), ConverterType: converterType),
                //Mapping of builtin types like string
                { Name: var typeName } when string.IsNullOrEmpty(typeName) is false => new(Name: propertySymbol.Name, TypeName: namedPropertySymbol.ToString(), ConverterType: converterType),
                //fallback, should never be hit but compiler gonna compile 
                _ => new(Name: propertySymbol.Name, TypeName: nameof(Object) + "?", ConverterType: converterType)
            };
        }

        static bool TryGetTypeFromAttribute(AttributeData? contentPipelinePropertyConverter,
            Action<Diagnostic> reportDiagnostic, out string propertyType)
        {
            propertyType = string.Empty;
            if (contentPipelinePropertyConverter?.AttributeClass is { TypeArguments.Length: 1 })
            {
                var converter = contentPipelinePropertyConverter.AttributeClass.TypeArguments[0];
                var contentPropertyConverterInterface = converter.Interfaces.FirstOrDefault(i => i.Name == "IContentPropertyConverter");
                propertyType = contentPropertyConverterInterface?.TypeArguments[1].ToString() ?? string.Empty;
            }

            return string.IsNullOrEmpty(propertyType) is false;
        }

        static string GetConverter(INamedTypeSymbol namedPropertySymbol, AttributeData? contentPipelinePropertyConverter, string? uiHint)
        {
            //gets and returns the converter type from contentPipelinePropertyConverter attribute
            if (contentPipelinePropertyConverter is { AttributeClass.TypeArguments.Length: 1 })
            {
                var value = contentPipelinePropertyConverter.AttributeClass.TypeArguments[0].ToString();

                if (value is not null)
                {
                    //converters from attributes does sets ConverterNamespace to empty. 
                    return value;
                }
            }

            //find default converter
            return namedPropertySymbol switch
            {
                //ContentReference gets special treatment as the ContentReference in itself is almost never what we want in the frontend as a consumer
                { Name: "ContentReference" } when uiHint is "mediafile" => "IMediaConverter",
                { Name: "ContentReference" } when uiHint is "image" => "IMediaConverter",
                { Name: "ContentReference" } when uiHint is "block" => "IBlockConverter",
                { Name: "ContentReference" } => "IContentReferenceConverter",

                { Name: "XhtmlString" } => "IXhtmlStringConverter",
                //ContentArea needs to be converted to a list of content area items
                { Name: "ContentArea" } => "IContentAreaConverter",
                //Url needs to be converted to a link object
                { Name: "Url" } => "ILinkConverter",
                //mapping of enums
                { TypeKind: TypeKind.Enum } => $"IEnumConverter<{namedPropertySymbol.ToString()}>",
                //Mapping of inline blocks on a page
                { BaseType: var baseType } when IsContentBaseType(baseType) => "IEmbeddedBlockConverter",

                //none in this context means just assign the value from the IContent to the ContentApiModel without a converter for types like string and bool
                _ => "None"
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
                case "ContentTypeAttribute":
                    contentType = attribute;
                    break;
                case "ContentPipelineModelAttribute":
                    contentPipelineModel = attribute;
                    break;

            }
        }
        return (ignore, uiHint, contentPipelinePropertyConverter, contentType, contentPipelineModel);
    }
}
