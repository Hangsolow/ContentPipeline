using ContentPipeline.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace ContentPipeline.SourceGenerator;

[Generator]
internal sealed partial class ContentPipelineSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#pragma warning disable CS8619 // the where statement ensures ClassDeclarationSyntax is not null but the compiler can't see that.
        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(static (s, _) => Parser.IsSyntaxTargetForGeneration(s), static (ctx, _) => Parser.GetSemanticTargetForGeneration(ctx))
            .Where(static c => c is not null);
#pragma warning restore CS8619

        IncrementalValueProvider<(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes)> compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());
        context.RegisterPostInitializationOutput(static callback => PostInitializationExcute(callback));
        context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.compilation, source.classes, spc));
    }

    

    /// <summary>
    /// This is where the heavy work should be
    /// </summary>
    /// <param name="compilation"></param>
    /// <param name="classes"></param>
    /// <param name="context"></param>
    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        Parser parser = new()
        {
            Compilation = compilation,
            CancellationToken = context.CancellationToken,
            ReportDiagnostic = context.ReportDiagnostic,
            InterfaceNamespace = "ContentPipeline.Interfaces"
        };

        var contentClasses = parser.GetContentClasses(classes);

        Emitter emitter = new()
        {
            SharedNamespace = "ContentPipeline",
            CancellationToken= context.CancellationToken,
        };

        foreach (var contentClass in contentClasses)
        {
            var contentModelSource = emitter.GetContentModel(contentClass);
            var pipelineSource = emitter.GetPipeline(contentClass);
            var uniqueId = contentClass.Guid.Substring(0, 8);

            context.AddSource($"{contentClass.Name}_ContentModel_{uniqueId}.g.cs", SourceText.From(contentModelSource, Encoding.UTF8));
            context.AddSource($"{contentClass.Name}_Pipeline_{uniqueId}.g.cs", SourceText.From(pipelineSource, Encoding.UTF8));
        }
    }
}
