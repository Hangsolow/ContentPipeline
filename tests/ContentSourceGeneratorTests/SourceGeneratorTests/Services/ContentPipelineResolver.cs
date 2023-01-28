using ContentPipeline.Interfaces;
using ContentPipeline.Models;
using ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Entities;
using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Services
{
    public partial class ContentPipelineResolver
    {
        ContentPipelineModel ContentToPipelineModel(IContentData contentData, IContentConverterPipelineContext pipelineContext)
        {
            return contentData switch
            {
                ContentPage content => RunPipeline<ContentPage, ContentPagePipelineModel>(content, pipelineContext),
                _ => new ContentPipelineModel(),
            };
        }
    }
}
