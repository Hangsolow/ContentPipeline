using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentPipeline.Attributes;
using EPiServer.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities
{
    [ContentPipelineModel]
    public class ImageBase : ImageData
    {
        [CultureSpecific]
        [Display(Name = "Title", GroupName = "Content", Order = 10)]
        public virtual string? Title { get; set; }

        [CultureSpecific]
        [Display(Name = "Copyright tekst", GroupName = "Content", Order = 20)]
        public virtual string? Copyright { get; set; }

        [CultureSpecific]
        [Display(Name = "Alt tekst", GroupName = "Content", Order = 30)]
        public virtual string? AltText { get; set; }
    }
}
