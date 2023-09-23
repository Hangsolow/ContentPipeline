using ContentPipeline.Interfaces;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.Datasources;

public class Datasource
{
    public string? Id { get; set; }
    public string? Url { get; set; }
    public IContentPipelineModel? ErrorMessage { get; set;}
}