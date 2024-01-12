using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentPipeline.Interfaces;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.ContentPropertyConverters;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
internal class DatasourceAttribute : Attribute, IContentPipelinePropertyConverterAttribute<DatasourceConverter>
{
    public required string DatasourceName { get; init; }

    public string? DatasourceConfig { get; set; }

    public int Order { get; init; }
}
