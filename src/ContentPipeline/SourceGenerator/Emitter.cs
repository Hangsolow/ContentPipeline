using System;
using System.Collections.Generic;
using System.Text;

namespace ContentPipeline.SourceGenerator;

internal sealed partial class Emitter
{
    internal required string SharedNamespace { get; init; }

    internal required CancellationToken CancellationToken { get; init; }

    private string GetPipelineModelFullName(ContentClass contentClass) => $"{SharedNamespace}.Models.{contentClass.Group}.{contentClass.Name}PipelineModel";
}

internal record ContentClass(string Name, string Guid, string Group, string FullyQualifiedName, IReadOnlyList<ContentProperty> ContentProperties);
internal record ContentProperty(string Name, string TypeName, string ConverterType, Dictionary<string, string>? ConterterConfig = null);
internal record CodeSource(string Name, string Source);