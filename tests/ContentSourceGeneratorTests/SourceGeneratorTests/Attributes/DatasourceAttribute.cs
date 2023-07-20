using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentPipeline.Interfaces;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
internal class DatasourceAttribute<TConverter> : Attribute, IContentPipelinePropertyConverterAttribute<TConverter> where TConverter : IContentPropertyConverter
{
    public required string DatasourceName { get; init; }

    public string? DatasourceConfig { get; set; }

    public int Order { get; init; }
}
