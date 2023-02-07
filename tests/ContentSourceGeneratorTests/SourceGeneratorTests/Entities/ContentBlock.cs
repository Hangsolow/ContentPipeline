using ContentPipeline.Attributes;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.Enums;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities
{
    [ContentType(GUID = "a446798f-e7f5-4f83-a9a2-b72047c7eaa1")]
    [ContentPipelineModel]
    public class ContentBlock : BlockData
    {
        public virtual string? Header { get; set; }

        public virtual XhtmlString? Text { get; set;}

        public virtual ContentReference? Link { get; set; }

        public virtual ColorEnum Color { get; set; } 
    }
}
