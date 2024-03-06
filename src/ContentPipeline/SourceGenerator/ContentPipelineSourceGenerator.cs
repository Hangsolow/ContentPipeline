using ContentPipeline.SourceGenerator;
using ContentPipeline.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace ContentPipeline;

[Generator]
internal sealed partial class ContentPipelineSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static callback => PostInitializationExecute(callback));

        IncrementalValuesProvider<ContentClass> contentToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "EPiServer.DataAnnotations.ContentTypeAttribute",
                predicate: static (_, _) => true,
                transform: static (ctx, _) => GetContentToGenerate(ctx.SemanticModel, ctx.TargetNode))
            .Where(static m => m is not null)!;

        var options = GetGeneratorOptions(context);

        context.RegisterSourceOutput(contentToGenerate.Collect().Combine(options), Execute);
    }

    /// <summary>
    /// This is where the heavy work should be
    /// </summary>
    /// <param name="compilation"></param>
    /// <param name="classes"></param>
    /// <param name="sourceProductionContext"></param>
    private static void Execute(SourceProductionContext sourceProductionContext, (ImmutableArray<ContentClass>, GeneratorOptions) args)
    {
        var (contentClasses, options) = args;

        if (options.FormsEnabled)
        {
            var builder = ImmutableArray.CreateBuilder<ContentClass>();
            builder.AddRange(contentClasses);
            ContentClass FormClass = new(Name: "FormContainerBlock", Guid: "02EC61FF-819F-4978-ADD6-A097F5BD944E", Group: "Form", FullyQualifiedName: "EPiServer.Forms.Implementation.Elements.FormContainerBlock", 4000, ContentProperties: new EquatableArray<ContentProperty>());
            builder.Add(FormClass);
            contentClasses = builder.ToImmutable();
        }

        const string sharedNamespace = "ContentPipeline";

        Emitter emitter = new()
        {
            SharedNamespace = sharedNamespace,
            CancellationToken = sourceProductionContext.CancellationToken,
        };

        var codeSources = emitter
            .GetServiceCodeSources(contentClasses)
            .Concat(emitter.GetServiceRegistrations(contentClasses));

        foreach (var codeSource in codeSources)
        {
            sourceProductionContext.AddSource(codeSource.Name, SourceText.From(codeSource.Source, Encoding.UTF8));
        }

        var jsonConverters = emitter.GetJsonConverter(contentClasses);
        foreach (var jsonConverter in jsonConverters)
        {
            sourceProductionContext.AddSource(jsonConverter.Name, SourceText.From(jsonConverter.Source, Encoding.UTF8));
        }

        var distantGroups = contentClasses.Select(c => c.Group).Distinct();

        foreach (var codeSource in emitter.GetGroupInterfaceSources(distantGroups))
        {
            sourceProductionContext.AddSource(codeSource.Name, SourceText.From(codeSource.Source, Encoding.UTF8));
        }

        foreach (var contentClass in contentClasses)
        {
            var contentModelSource = emitter.GetContentModel(contentClass);
            var pipelineSource = emitter.GetPipeline(contentClass);
            var uniqueId = contentClass.Guid.Substring(0, 8);

            sourceProductionContext.AddSource($"{contentClass.Group}_{contentClass.Name}_ContentModel_{uniqueId}.g.cs", SourceText.From(contentModelSource, Encoding.UTF8));
            sourceProductionContext.AddSource($"{contentClass.Group}_{contentClass.Name}_Pipeline_{uniqueId}.g.cs", SourceText.From(pipelineSource, Encoding.UTF8));
        }
    }

    private static ContentClass? GetContentToGenerate(SemanticModel semanticModel, SyntaxNode targetNode)
    {
        if (targetNode is ClassDeclarationSyntax classDeclaration && Parser.IsContentClassSyntexForGeneration(semanticModel, classDeclaration))
        {
            var contentClassSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            if (contentClassSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                return Parser.GetContentClass(namedTypeSymbol, semanticModel, classDeclaration, "ContentPipeline.Interfaces");
            }
        }

        return null;
    }

    private IncrementalValueProvider<GeneratorOptions> GetGeneratorOptions(IncrementalGeneratorInitializationContext context)
    {
        return context.AnalyzerConfigOptionsProvider
                      .Select((options, _) =>
                      {
                          options.GlobalOptions.TryGetValue("build_property.ContentPipeline_EnableForms", out var counterEnabledValue);
                          return new GeneratorOptions(counterEnabledValue);
                      });
    }
}