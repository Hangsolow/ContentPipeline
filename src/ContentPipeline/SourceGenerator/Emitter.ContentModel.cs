using ContentPipeline.CodeBuilders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContentPipeline.SourceGenerator
{
    internal sealed partial class Emitter
    {
        internal string GetContentModel(ContentClass contentClass)
        {
            CancellationToken.ThrowIfCancellationRequested();

            return CSharpCodeBuilder.Create()
                .Line("#nullable enable")
                .Using("System")
                .Using("System.Collections.Generic")
                .Namespace($"{SharedNamespace}.Models")
                .Tab()
                //.Line($"[{SharedNamespace}.Attributes.GenerateTypescriptDefinition()]")
                .Tab(-1)
                .Class($"internal partial class {contentClass.Name}Model : {SharedNamespace}.Models.ContentPipelineModel")
                .Tab()
                .Foreach(contentClass.ContentProperties, (pBuilder, prop) => pBuilder.Property(prop.Name, prop.TypeName, true))
                .Build();
        }
    }
}
