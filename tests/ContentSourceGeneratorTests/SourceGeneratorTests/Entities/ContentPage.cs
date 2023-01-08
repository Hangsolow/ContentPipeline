using ContentSourceGeneratorTests.SourceGeneratorTests.Attributes;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentSourceGeneratorTests.SourceGeneratorTests.Entities
{
    [ContentType(GUID = "308068d7-e9b1-4958-b13b-bc612707cb85")]
    public partial class ContentPage : PageData
    {
        public virtual string? Title { get; set; }

        [ContentPipelinePropertyConverter<string, string>]
        public virtual IList<string>? ListOfStrings { get; set; }
    }

    public class ContentPage1 : ContentPage
    {
    }
}
