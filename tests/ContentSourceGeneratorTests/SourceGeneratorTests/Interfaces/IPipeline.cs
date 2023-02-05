using EPiServer.Core;

namespace ContentPipeline.Interfaces;

public interface IContentPipeline<TContent, TPipelineModel>
    where TContent : IContentData
    where TPipelineModel : IContentPipelineModel, new()
{
    /// <summary>
    /// Runs the pipeline for the content
    /// </summary>
    /// <param name="content"></param>
    /// <param name="pipelineContext"></param>
    /// <returns></returns>
    TPipelineModel Run(TContent content, IContentPipelineContext pipelineContext);
}