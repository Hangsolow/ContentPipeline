using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;

namespace ContentPipeline.SourceGenerator;

internal sealed partial class Parser
{
    internal required string InterfaceNamespace { get; init; }

    private static IEnumerable<ContentProperty> GetContentProperties(INamedTypeSymbol contentClassSymbol, SemanticModel semanticModel, ClassDeclarationSyntax classDeclaration, string interfaceNamespace)
    {
        var propertySymbols = semanticModel.LookupSymbols(classDeclaration.SpanStart, contentClassSymbol)
            .Where(s => s.Kind == SymbolKind.Property && !s.ContainingNamespace.ToString().StartsWith("episerver", StringComparison.OrdinalIgnoreCase))
            .OfType<IPropertySymbol>();

        foreach (var propertySymbol in propertySymbols)
        {
            var contentProperty = GetContentProperty(propertySymbol, interfaceNamespace);
            if (contentProperty is not null)
            {
                yield return contentProperty;
            }
        }

        static ContentProperty? GetContentProperty(IPropertySymbol propertySymbol, string interfaceNamespace)
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

            if (TryGetTypeFromAttribute(attributes.ContentPipelinePropertyConverter, out var typeInfo))
            {
                return new(Name: propertySymbol.Name, TypeName: typeInfo.propertyType, ConverterType: converterType, ConverterConfig: typeInfo.converterConfig);
            }

            return namedPropertySymbol switch
            {
                //Mapping of ContentReference
                { Name: "ContentReference", NullableAnnotation: var nullableAnnotation } when uiHint is "mediafile" => new(Name: propertySymbol.Name, TypeName: GetTypeName("Media", nullableAnnotation), ConverterType: converterType),
                { Name: "ContentReference", NullableAnnotation: var nullableAnnotation } when uiHint is "image" => new(Name: propertySymbol.Name, TypeName: GetTypeName("Media", nullableAnnotation), ConverterType: converterType),
                { Name: "ContentReference", NullableAnnotation: var nullableAnnotation } when uiHint is "block" => new(Name: propertySymbol.Name, TypeName: GetTypeName("IContentPipelineModel", nullableAnnotation), ConverterType: converterType),
                { Name: "ContentReference", NullableAnnotation: var nullableAnnotation } => new(Name: propertySymbol.Name, TypeName: GetTypeName($"ILinkPipelineModel", nullableAnnotation), ConverterType: converterType),
                { Name: "PageReference", NullableAnnotation: var nullableAnnotation } => new(Name: propertySymbol.Name, TypeName: GetTypeName($"ILinkPipelineModel", nullableAnnotation), ConverterType: converterType),
                //Mapping of ContentAreas
                { Name: "ContentArea", NullableAnnotation: var nullableAnnotation } => new(Name: propertySymbol.Name, TypeName: GetTypeName("ContentAreaPipelineModel", nullableAnnotation), ConverterType: converterType),
                //Mapping of richtext properties
                { Name: "XhtmlString", NullableAnnotation: var nullableAnnotation } => new(Name: propertySymbol.Name, TypeName: GetTypeName(nameof(String), nullableAnnotation), ConverterType: converterType),
                //mapping of urls
                { Name: "Url", NullableAnnotation: var nullableAnnotation } => new(Name: propertySymbol.Name, TypeName: GetTypeName("ILinkPipelineModel", nullableAnnotation), ConverterType: converterType),
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

        static bool TryGetTypeFromAttribute(AttributeData? contentPipelinePropertyConverter, out (string propertyType, Dictionary<string, string>? converterConfig) typeInfo)
        {
            typeInfo = (string.Empty, null);
            if (contentPipelinePropertyConverter?.AttributeClass?.Interfaces[0] is { TypeArguments.Length: 1 })
            {
                var converter = contentPipelinePropertyConverter.AttributeClass.Interfaces[0].TypeArguments[0];
                var contentPropertyConverterInterface = converter.AllInterfaces.FirstOrDefault(i => i.Name == "IContentPropertyConverter");
                var propertyType = contentPropertyConverterInterface?.TypeArguments[1].ToString() ?? string.Empty;
                typeInfo = (propertyType, GetConverterConfig(contentPipelinePropertyConverter));
            }

            return string.IsNullOrEmpty(typeInfo.propertyType) is false;
        }

        static Dictionary<string,string>? GetConverterConfig(AttributeData contentPipelinePropertyConverter)
        {
            if (contentPipelinePropertyConverter.NamedArguments.Length == 0)
            {
                return null;
            }

            var configDict = new Dictionary<string, string>();

            foreach (var namedArg in contentPipelinePropertyConverter.NamedArguments)
            {
                var key = namedArg.Key;
                var value = namedArg.Value.Value?.ToString() ?? string.Empty;

                configDict.Add(key, value);
            }

            return configDict;
        }

        static string GetConverter(INamedTypeSymbol namedPropertySymbol, AttributeData? contentPipelinePropertyConverter, string? uiHint)
        {
            //gets and returns the converter type from contentPipelinePropertyConverter attribute
            if (contentPipelinePropertyConverter?.AttributeClass?.Interfaces[0] is { TypeArguments.Length: 1 })
            {
                var value = contentPipelinePropertyConverter.AttributeClass.Interfaces[0].TypeArguments[0].ToString();

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
                { Name: "PageReference" } => "IContentReferenceConverter",

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

    private static ContentPipelineAttributes GetNamedAttributes(ImmutableArray<AttributeData> attributes)
    {
        AttributeData? ignore = null;
        AttributeData? uiHint = null;
        AttributeData? contentPipelinePropertyConverter = null;
        AttributeData? contentType = null;
        AttributeData? contentPipelineModel = null;

        foreach (var attribute in attributes)
        {
            switch (attribute)
            {
                case { AttributeClass.Name: "ContentPipelineIgnoreAttribute" }:
                    ignore = attribute;
                    break;
                case { AttributeClass.Name: "UIHintAttribute" }:
                    uiHint = attribute;
                    break;
                case { AttributeClass.Name: "ContentPipelinePropertyConverterAttribute" }:
                    contentPipelinePropertyConverter = attribute;
                    break;
                case { AttributeClass.Name: "ContentTypeAttribute" }:
                    contentType = attribute;
                    break;
                case { AttributeClass.Name: "ContentPipelineModelAttribute" }:
                    contentPipelineModel = attribute;
                    break;
                case { AttributeClass.Interfaces.Length: 1  } when attribute.AttributeClass.Interfaces[0].Name == "IContentPipelinePropertyConverterAttribute":
                    contentPipelinePropertyConverter = attribute;
                    break;

            }
        }
        return new ContentPipelineAttributes(ignore, uiHint, contentPipelinePropertyConverter, contentType, contentPipelineModel);
    }
}
