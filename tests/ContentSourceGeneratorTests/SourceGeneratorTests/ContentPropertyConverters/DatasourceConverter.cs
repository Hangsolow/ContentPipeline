using ContentPipeline.Interfaces;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.Datasources;
using EPiServer.Core;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.ContentPropertyConverters;

public class DatasourceConverter : IContentPropertyConverter<Datasource?, Datasource?>
{
    public Datasource? GetValue(Datasource? property, IContentData content, string propertyName,
        IContentPipelineContext pipelineContext, Dictionary<string, string>? config = null)
    {
        if (config is null)
        {
            return null;
        }
        
        if (config.TryGetValue("DatasourceName", out var name) &&
            config.TryGetValue("DatasourceConfig", out var dataConfig))
        {
            return new Datasource()
            {
                Id = name,
                Url = dataConfig,
                ErrorMessage = null
            }; 
        }

        return null;
    }
}