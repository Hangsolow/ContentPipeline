using ContentPipeline.CodeBuilders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContentPipeline.SourceGenerator;

internal sealed partial class Emitter
{
    internal string GetContentModel(ContentClass contentClass)
    {
        CancellationToken.ThrowIfCancellationRequested();

        return CSharpCodeBuilder.Create()
            .Line("#nullable enable")
            .Using("System")
            .Using("System.Collections.Generic")
            .Using($"{SharedNamespace}.Interfaces")
            .Using($"{SharedNamespace}.Properties")
            .Namespace($"{SharedNamespace}.Models.{contentClass.Group}")
            .Tab()
            //.Line($"[{SharedNamespace}.Attributes.GenerateTypescriptDefinition()]")
            .Tab(-1)
            .Class($"public partial class {contentClass.Name}PipelineModel : {SharedNamespace}.Models.ContentPipelineModel, {SharedNamespace}.Interfaces.IContentPipelineModel, {SharedNamespace}.Interfaces.I{contentClass.Group}ContentPipelineModel")
            .Tab()
            .Foreach(contentClass.ContentProperties, (pBuilder, prop) => pBuilder.Property(prop.Name, prop.TypeName, true))
            .Build();
    }
}
