using ContentPipeline.Interfaces;
using ContentPipeline.Models.Awesome;
using ContentPipeline.Models.Common;
using ContentPipeline.Properties;
using ContentPipelineSourceGeneratorTests.Utils;
using Xunit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContentPipelineSourceGeneratorTests.Tests.JsonConverterTests;

[Trait("JsonConverter", "")]
public class JsonConverter_Given_Vaild_PipelineModel_Should
{
    [AutoMock, Theory]
    public void Convert_Model_To_Json(ContentBlockPipelineModel blockLink, ContentBlockPipelineModel embeddedBlock)
    {
        blockLink.Link = new ContentPipeline.Properties.Link { Url = "/some/otherPage" };
        IContentPipelineModel contentPageModel = new ContentPagePipelineModel
        {
            ListOfStrings = new List<string> { "1", "2" },
            Title = "Title",
            CustomMapping = false,
            MediaLink = new ContentPipeline.Properties.Media { Url = "/test/image.jpg", Type = "jpg" },
            Url = new ContentPipeline.Properties.Link { Url = "/some/page" },
            BlockLink = blockLink,
            EmbeddedBlock = embeddedBlock

        };
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new ContentPipeline.JsonConverters.ContentPipelineModelJsonConverter());
        options.Converters.Add(new ContentPipeline.JsonConverters.LinkPipelineModelJsonConverter());
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

        var json = JsonSerializer.Serialize(contentPageModel, options);

        Assert.Contains("\"title\":\"Title\"", json);
        Assert.Contains("\"url\":{\"url\":\"/some/page\"}", json);
        Assert.Contains("\"mediaLink\":{\"url\":\"/test/image.jpg\",\"type\":\"jpg\"}", json);
        Assert.Contains($"\"blockLink\":{{\"{nameof(blockLink.Header)}\":\"{blockLink.Header}\",\"text\":\"{blockLink.Text}\",\"link\":{{\"url\":\"{(blockLink.Link as Link)?.Url}\"}}", json, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains($"\"{nameof(ContentPagePipelineModel.EmbeddedBlock)}\":{JsonSerializer.Serialize(embeddedBlock, options)}", json, StringComparison.InvariantCultureIgnoreCase);
    }
}
