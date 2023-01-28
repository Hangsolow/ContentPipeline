﻿using System.ComponentModel.DataAnnotations;
using ContentPipeline.Attributes;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Web;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities;

[ContentType(GUID = "308068d7-e9b1-4958-b13b-bc612707cb85")]
[ContentPipelineModel("Awesome")]
public class ContentPage : PageData
{
    public virtual string? Title { get; set; }

    public virtual Url? Url { get; set; }

    [ContentPipelineIgnore]
    public virtual ContentReference? Link { get; set; }
    
    [UIHint(UIHint.Image)]
    public virtual ContentReference? MediaLink { get; set; }
    
    [UIHint(UIHint.Block)]
    public virtual ContentReference? BlockLink { get; set; }

    public virtual IList<string>? ListOfStrings { get; set; }

    public virtual ContentBlock? EmbeddedBlock { get; set; } 
}