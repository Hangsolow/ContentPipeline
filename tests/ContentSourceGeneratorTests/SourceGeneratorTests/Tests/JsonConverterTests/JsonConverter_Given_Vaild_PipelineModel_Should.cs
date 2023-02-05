﻿using ContentPipeline.Interfaces;
using ContentPipeline.Models.Awesome;
using ContentPipeline.Models.Common;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Tests.JsonConverterTests
{
    public class JsonConverter_Given_Vaild_PipelineModel_Should
    {
        [Fact]
        public void Convert_Model_To_Json()
        {
            var block = new ContentBlockPipelineModel
            {
                Header = "Header1",
                Link = new ContentPipeline.Properties.Link { Url = "/some/where" },
                Text = "<p>Test paragraph</p>"
            };

            var block2 = new ContentBlockPipelineModel
            {
                Header = "Header2",
                Link = new ContentPipeline.Properties.Link { Url = "/some/where/else" },
                Text = "&lt;p&gt;Test paragraph2&lt;/p&gt;"
            };

            IContentPipelineModel contentPageModel = new ContentPagePipelineModel
            {
                ListOfStrings = new List<string> { "1", "2" },
                Title = "Title",
                CustomMapping = false,
                MediaLink = new ContentPipeline.Properties.Media { Url = "/test/image.jpg", Type = "jpg" },
                Url = new ContentPipeline.Properties.Link { Url = "/some/page" },
                BlockLink = block,
                EmbeddedBlock = block2

            };
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(new ContentPipeline.JsonConverters.ContentPipelineModelJsonConverter());
            options.WriteIndented = true;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
  
            var json = JsonSerializer.Serialize(contentPageModel, options);

            json.Should().Contain("\"title\": \"Title\"");
            json.Should().Contain("\"link\": {\r\n      \"url\": \"/some/where\"\r\n    }");
            json.Should().Contain("\"mediaLink\": {\r\n    \"url\": \"/test/image.jpg\",\r\n    \"type\": \"jpg\"\r\n  }");
            json.Should().Contain("\"blockLink\": {\r\n    \"header\": \"Header1\",\r\n    \"text\": \"\\u003Cp\\u003ETest paragraph\\u003C/p\\u003E\",\r\n    \"link\": {\r\n      \"url\": \"/some/where\"\r\n    }\r\n  }");
        }
    }
}