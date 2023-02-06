using ContentPipeline.CodeBuilders;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace ContentPipeline.SourceGenerator
{
    internal partial class Emitter
    {
        internal IEnumerable<CodeSource> GetServiceRegistrations(IEnumerable<ContentClass> contentClasses)
        {
            yield return new("PipelineStepServiceRegistrations.g.cs", CreatePipelineRegistrations(contentClasses));
            yield return new("ContentPipelineServiceRegistrations.g.cs", CreateServiceRegistrations());
            
            string CreatePipelineRegistrations(IEnumerable<ContentClass> contentClasses) =>
                CSharpCodeBuilder.Create()
                .Line("#nullable enable")
                .Using("Microsoft.Extensions.DependencyInjection")
                .Namespace($"{SharedNamespace}.ServiceCollectionExtensions")
                .Class("public static class PipelineStepsServiceCollectionExtensions")
                .Tab()
                .NewLine()
                .Method("public static IServiceCollection AddContentPipelineGeneratedSteps(this IServiceCollection services)", methodBuilder => methodBuilder
                    .Tab()
                    .Line("return services")
                    .Tab()
                    .Foreach(contentClasses, (b, c) => b.Line($".AddSingleton<{SharedNamespace}.Interfaces.IContentPipelineStep<{c.FullyQualifiedName}, {GetPipelineModelFullName(c)}>, {GetPipelineStepFullName(c)}>()"))
                    .Line(";"))
                .Build();


            string GetPipelineStepFullName(ContentClass contentClass) => $"{SharedNamespace}.Pipelines.{contentClass.Group}.Steps.{contentClass.Name}PipelineStep";

            string CreateServiceRegistrations() =>
                $$"""
                #nullable enable
                namespace {{SharedNamespace}}.ServiceCollectionExtensions;

                using Microsoft.Extensions.DependencyInjection;
                using {{SharedNamespace}}.Interfaces;
                using {{SharedNamespace}}.Services;
                using {{SharedNamespace}}.Converters;

                public static class ContentPipelineServiceCollectionExtensions
                {
                    public static IServiceCollection AddContentPipelineServices(this IServiceCollection services)
                    {
                        return services
                            .AddContentPipelineGeneratedSteps()
                            .AddTransient<IXhtmlRenderService, XhtmlRenderService>()
                            .AddSingleton<IContentPipelineService, ContentPipelineService>()
                            .AddSingleton<IBlockConverter, BlockConverter>()
                            .AddSingleton<IEmbeddedBlockConverter, InlineBlockConverter>()
                            .AddSingleton<IContentAreaConverter, ContentAreaConverter>()
                            .AddSingleton<ILinkConverter, LinkConverter>()
                            .AddSingleton<IXhtmlStringConverter, XhtmlStringConverter>()
                            .AddSingleton<IContentReferenceConverter, ContentReferenceConverter>()
                            .AddSingleton<IMediaConverter, MediaConverter>()
                            .AddSingleton(typeof(IContentPipeline<,>), typeof(DefaultContentPipeline<,>))
                            ;
                    }
                }
                """;
        }
    }
}
