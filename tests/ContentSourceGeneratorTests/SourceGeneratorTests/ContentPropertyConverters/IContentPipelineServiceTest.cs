using ContentPipeline.Entities;
using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentPipeline.Interfaces;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.ContentPropertyConverters;
public partial class ContentPipelineService : ContentPipeline.Services.BaseContentPipelineService
{

    public ContentPipelineService(
        IContentPipeline<ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.Jpg, ContentPipeline.Models.MediaContent.JpgPipelineModel> MediaContent1AFD8370Jpg,
        IContentPipeline<ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.ContentBlock, ContentPipeline.Models.Common.ContentBlockPipelineModel> Commona446798fContentBlock,
        IContentPipeline<EPiServer.Forms.Implementation.Elements.FormContainerBlock, ContentPipeline.Models.Forms.FormContainerBlockPipelineModel> Forms02EC61FFFormContainerBlock,
        IContentPipeline<ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.ContentPage, ContentPipeline.Models.Awesome.ContentPagePipelineModel> Awesome308068d7ContentPage)
    {
        this.MediaContent1AFD8370Jpg = MediaContent1AFD8370Jpg;
        this.Commona446798fContentBlock = Commona446798fContentBlock;
        this.Forms02EC61FFFormContainerBlock = Forms02EC61FFFormContainerBlock;
        this.Awesome308068d7ContentPage = Awesome308068d7ContentPage;
    }
    private IContentPipeline<ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.Jpg, ContentPipeline.Models.MediaContent.JpgPipelineModel> MediaContent1AFD8370Jpg { get; }
    private IContentPipeline<ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.ContentBlock, ContentPipeline.Models.Common.ContentBlockPipelineModel> Commona446798fContentBlock { get; }
    private IContentPipeline<EPiServer.Forms.Implementation.Elements.FormContainerBlock, ContentPipeline.Models.Forms.FormContainerBlockPipelineModel> Forms02EC61FFFormContainerBlock { get; }
    private IContentPipeline<ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.ContentPage, ContentPipeline.Models.Awesome.ContentPagePipelineModel> Awesome308068d7ContentPage { get; }
    protected override IContentPipelineModel RunPipelineForContent(IContentData content, IContentPipelineContext context)
    {
        return content switch
        {
            ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.Jpg castContent => MediaContent1AFD8370Jpg.Run(castContent, context),
            ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.ContentBlock castContent => Commona446798fContentBlock.Run(castContent, context),
            EPiServer.Forms.Implementation.Elements.FormContainerBlock castContent => Forms02EC61FFFormContainerBlock.Run(castContent, context),
            ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.ContentPage castContent => Awesome308068d7ContentPage.Run(castContent, context),
            _ => new ContentPipeline.Models.ContentPipelineModel()
        };
    }

    protected override async Task<IContentPipelineModel> RunPipelineForContentAsync(IContentData content, IContentPipelineContext context)
    {
        return content switch
        {
            ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.Jpg castContent => await MediaContent1AFD8370Jpg.RunAsync(castContent, context),
            ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.ContentBlock castContent => await Commona446798fContentBlock.RunAsync(castContent, context),
            EPiServer.Forms.Implementation.Elements.FormContainerBlock castContent => Forms02EC61FFFormContainerBlock.Run(castContent, context),
            ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities.ContentPage castContent => Awesome308068d7ContentPage.Run(castContent, context),
            _ => new ContentPipeline.Models.ContentPipelineModel()
        };
    }

}
