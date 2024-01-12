using System.ComponentModel.DataAnnotations;
using ContentPipeline.Attributes;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Attributes;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.ContentPropertyConverters;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.Datasources;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Web;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities;

[ContentType(GUID = "308068d7-e9b1-4958-b13b-bc612707cb85")]
[ContentPipelineModel("Awesome", 100)]
public class ContentPage : PageData
{
    public virtual string? Title { get; set; }

    public virtual Url? Url { get; set; }

    [ContentPipelineIgnore]
    public virtual ContentReference? IgnoreLink { get; set; }

    [UIHint(UIHint.Image)]
    public virtual ContentReference? MediaLink { get; set; }

    [UIHint(UIHint.Block)]
    public virtual ContentReference? BlockLink { get; set; }

    public virtual IList<string>? ListOfStrings { get; set; }

    public virtual PageReference? LinkToPage { get; set; }

    public virtual ContentReference? Link { get; set; }

    public virtual ContentBlock? EmbeddedBlock { get; set; }

    [ContentPipelinePropertyConverter<CustomConverter>]
    public virtual XhtmlString? CustomMapping { get; set; }

    [Datasource(DatasourceName = "TestDatasource", DatasourceConfig = "Config", Order = 40)]
    [Ignore]
    public virtual Datasource? CustomMappingWithCustomAttribute { get; set; }
}