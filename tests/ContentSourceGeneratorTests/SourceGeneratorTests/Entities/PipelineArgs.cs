using System.Globalization;
using EPiServer.Core;
using Microsoft.AspNetCore.Http;

namespace ContentPipeline.Entities;
public partial record PipelineArgs
{
    public static implicit operator PipelineArgs((IContentData content, HttpContext httpContext, CultureInfo? language) values)
    {
        ArgumentNullException.ThrowIfNull(values.content);
        ArgumentNullException.ThrowIfNull(values.httpContext);
        return new PipelineArgs
        {
            Content = values.content,
            HttpContext = values.httpContext,
            Language = values.language
        };
    }
}