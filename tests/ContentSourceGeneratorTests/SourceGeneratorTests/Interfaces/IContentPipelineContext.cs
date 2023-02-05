using System.Globalization;

namespace ContentPipeline.Interfaces;

public partial interface IContentPipelineContext
{
    IContentPipelineService ContentPipelineService { get; }
    
    CultureInfo? Language { get; }
}