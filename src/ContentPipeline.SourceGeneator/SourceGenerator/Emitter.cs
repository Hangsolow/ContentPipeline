﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ContentPipeline.SourceGenerator;

internal sealed partial class Emitter
{
    internal required string SharedNamespace { get; init; }

    internal required CancellationToken CancellationToken { get; init; }
}

internal record ContentClass(string Name, string Guid, string Group, string FullyQualifiedName, IReadOnlyList<ContentProperty> ContentProperties);
internal record ContentProperty(string Name, string TypeName, string ConverterType, string? ConverterNamespace);
internal record CodeSource(string Name, string Source);