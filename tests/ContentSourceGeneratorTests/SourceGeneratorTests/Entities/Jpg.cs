using EPiServer.DataAnnotations;
using EPiServer.Framework.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities
{
    [ContentType(DisplayName = "Jpg", GUID = "1AFD8370-CDBE-420A-94D9-2CCBBDB9C09B", Description = "JPG media")]
    [MediaDescriptor(ExtensionString = "jpg")]
    public class Jpg : ImageBase
    {
    }
}
