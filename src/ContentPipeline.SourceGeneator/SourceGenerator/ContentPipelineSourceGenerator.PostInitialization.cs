using ContentPipeline.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContentPipeline.SourceGenerator
{
    internal sealed partial class ContentPipelineSourceGenerator
    {
        private static void PostInitializationExecute(IncrementalGeneratorPostInitializationContext context)
        {
            Emitter emitter = new()
            {
                SharedNamespace = "ContentPipeline",
                CancellationToken = context.CancellationToken,
            };

            context.AddSource("ContentPipelineModel.g.cs", SourceText.From(emitter.GetBaseContentPipelineModel(), Encoding.UTF8));
            var codeSources = emitter
                .GetPropertiesSourceFiles()
                .Concat(emitter.GetInterfaceSources())
                .Concat(emitter.GetAttributes())
                .Concat(emitter.GetContentPropertyConverters());

            foreach (var codeSource in codeSources)
            {
                context.AddSource(codeSource.Name, SourceText.From(codeSource.Source, Encoding.UTF8));
            }
        }
    }
}
