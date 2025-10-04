using ContentPipeline.Interfaces;
using ContentPipeline.Models.Awesome;
using ContentPipeline.Models.Common;
using ContentPipeline.Models.MediaContent;
using ContentPipeline.Properties;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.Datasources;
using Xunit;

namespace ContentPipelineSourceGeneratorTests.Tests.Models;

[Trait("Models", "")]
public class ContentPipelineModel_Given_ContentModel
{
    [Theory]
    [InlineData(typeof(ContentPagePipelineModel), typeof(string), "Title")]
    [InlineData(typeof(ContentPagePipelineModel), typeof(ILinkPipelineModel), "Url")]
    [InlineData(typeof(ContentPagePipelineModel), typeof(ILinkPipelineModel), "LinkToPage")]
    [InlineData(typeof(ContentPagePipelineModel), typeof(Media), "MediaLink")]
    [InlineData(typeof(ContentPagePipelineModel), typeof(IContentPipelineModel), "BlockLink")]
    [InlineData(typeof(ContentPagePipelineModel), typeof(IList<string>), "ListOfStrings")]
    [InlineData(typeof(ContentPagePipelineModel), typeof(IContentPipelineModel), "EmbeddedBlock")]
    [InlineData(typeof(ContentPagePipelineModel), typeof(bool), "CustomMapping")]
    [InlineData(typeof(ContentBlockPipelineModel), typeof(string), "Header")]
    [InlineData(typeof(ContentBlockPipelineModel), typeof(string), "Text")]
    [InlineData(typeof(ContentBlockPipelineModel), typeof(ILinkPipelineModel), "Link")]
    [InlineData(typeof(ContentBlockPipelineModel), typeof(string), "Color")]
    [InlineData(typeof(ContentPagePipelineModel), typeof(Datasource), "CustomMappingWithCustomAttribute")]
    [InlineData(typeof(JpgPipelineModel), typeof(string), "Title")]
    [InlineData(typeof(JpgPipelineModel), typeof(string), "Copyright")]
    [InlineData(typeof(JpgPipelineModel), typeof(string), "AltText")]
    public void Should_Have_Properties_From_ContentModel(Type contentPipelineModelType, Type returnType, string property)
    {
        Assert.Equal(returnType, contentPipelineModelType.GetProperty(property)?.PropertyType);
    }

    [Theory]
    [InlineData(typeof(ContentPagePipelineModel))]
    [InlineData(typeof(ContentBlockPipelineModel))]
    public void Should_Only_Have_Writeable_Properties(Type type)
    {
        foreach (var prop in type.GetProperties())
        {
            Assert.True(prop.CanWrite, $"Property {prop.Name} should be writable to play nice with the json serializer");
        }
    }
}
