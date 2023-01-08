using ContentPipeline.CodeBuilders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContentPipeline.SourceGenerator
{

    /// <summary>
    /// The base class for all content pipeline models, it is partial in order to make it easy to add properties
    /// </summary>
    internal sealed partial class Emitter
    {
        internal string GetBaseContentPipelineModel()
        {
            return $$"""
            #nullable enable
            namespace {{SharedNamespace}}.Models
            {
                /// <summary>
                /// The base class for all content pipeline models, it is partial in order to make it easy to add properties
                /// </summary>
                internal abstract partial class ContentPipelineModel
                {
                }
            }
            """;
        }
    }
}
