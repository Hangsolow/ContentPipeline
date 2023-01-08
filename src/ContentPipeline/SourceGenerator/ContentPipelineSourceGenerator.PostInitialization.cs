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
        private static void PostInitializationExcute(IncrementalGeneratorPostInitializationContext context)
        {
            Emitter emitter = new()
            {
                SharedNamespace = "ContentPipeline",
                CancellationToken = context.CancellationToken,
            };

            context.AddSource("ContentPipelineModel.g.cs", SourceText.From(emitter.GetBaseContentPipelineModel(), Encoding.UTF8));
        }
    }
}
