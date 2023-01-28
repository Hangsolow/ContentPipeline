using ContentPipeline.Interfaces;
using ContentPipeline.Models;
using EPiServer.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ContentPipelineSourceGeneratorTests.SourceGeneratorTests.Services
{
    public partial class ContentPipelineResolver : IContentConvertService
    {
        public ContentPipelineResolver(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        private IServiceProvider ServiceProvider { get; }

        public ContentPipelineModel ConvertConvert(IContentData content, HttpContext httpContext, CultureInfo? language)
        {
            var pipelineContext = new PipelineContext(httpContext, language, this);
            return ContentToPipelineModel(content, pipelineContext);
        }

        private TContentPipelineModel RunPipeline<TContent, TContentPipelineModel>(TContent content, IContentConverterPipelineContext pipelineContext)
            where TContent : IContentData
            where TContentPipelineModel : ContentPipelineModel, new()
            
        {
            return new();
        }
    }

    internal record PipelineContext(HttpContext HttpContext, CultureInfo? Language, IContentConvertService ContentConvertService) : IContentConverterPipelineContext;
}
