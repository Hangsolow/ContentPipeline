using ContentPipeline.SourceGenerator;
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

        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(static (s, _) => Parser.IsSyntaxTargetForGeneration(s), static (ctx, _) => Parser.GetSemanticTargetForGeneration(ctx))
            .Where(static c => c is not null)!;

        IncrementalValueProvider<(Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider config, (Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes) Right)> compilationAndClasses = context
            .AnalyzerConfigOptionsProvider
                .Combine(context
                    .CompilationProvider
                    .Combine(classDeclarations.Collect()));

        context.RegisterPostInitializationOutput(static callback => PostInitializationExecute(callback));
        context.RegisterSourceOutput(compilationAndClasses, static (spc, settings) => Execute(settings.Right.compilation, settings.Right.classes, spc, settings.config));
    }



    /// <summary>
    /// This is where the heavy work should be
    /// </summary>
    /// <param name="compilation"></param>
    /// <param name="classes"></param>
    /// <param name="sourceProductionContext"></param>
    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext sourceProductionContext, Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider config)
    {
        const string sharedNamespace = "ContentPipeline";
        //var options = config.GetOptions(classes.First().SyntaxTree);
        //options.TryGetValue("contentpipeline_namespace", out var sharedNamespace);

        Parser parser = new()
        {
            Compilation = compilation,
            CancellationToken = sourceProductionContext.CancellationToken,
            ReportDiagnostic = sourceProductionContext.ReportDiagnostic,
            InterfaceNamespace = $"{sharedNamespace}.Interfaces"
        };

        var contentClasses = parser.GetContentClasses(classes);

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

        var jsonConverter = emitter.GetJsonConverter(contentClasses);
        sourceProductionContext.AddSource(jsonConverter.Name, SourceText.From(jsonConverter.Source, Encoding.UTF8));

        foreach (var contentClass in contentClasses)
        {
            var contentModelSource = emitter.GetContentModel(contentClass);
            var pipelineSource = emitter.GetPipeline(contentClass);
            var uniqueId = contentClass.Guid.Substring(0, 8);

            sourceProductionContext.AddSource($"{contentClass.Group}_{contentClass.Name}_ContentModel_{uniqueId}.g.cs", SourceText.From(contentModelSource, Encoding.UTF8));
            sourceProductionContext.AddSource($"{contentClass.Group}_{contentClass.Name}_Pipeline_{uniqueId}.g.cs", SourceText.From(pipelineSource, Encoding.UTF8));
        }
    }
}
