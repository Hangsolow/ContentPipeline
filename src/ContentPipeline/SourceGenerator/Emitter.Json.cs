using ContentPipeline.CodeBuilders;

namespace ContentPipeline.SourceGenerator;

internal sealed partial class Emitter
{
    internal IEnumerable<CodeSource> GetJsonConverter(IEnumerable<ContentClass> contentClasses)
    {
        yield return new CodeSource("ContentPipelineModelJsonConverter.g.cs", CreateContentJsonConverter(contentClasses));
        yield return new CodeSource("LinkPipelineModelJsonConverter.g.cs", CreateLinkJsonConverter(contentClasses));

        string CreateContentJsonConverter(IEnumerable<ContentClass> contentClasses) =>
            CSharpCodeBuilder.Create()
            .Line("#nullable enable")
            .Using("System")
            .Using("System.Text.Json")
            .Using("System.Text.Json.Serialization")
            .Namespace($"{SharedNamespace}.JsonConverters")
            .Class($"internal class ContentPipelineModelJsonConverter : JsonConverter<{SharedNamespace}.Interfaces.IContentPipelineModel>")
            .Tab()
            .Line("private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web)")
                .CodeBlock(b => b.Line("DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull", indentation: 1), end: "};")
            .NewLine()
            .Method($"public override {SharedNamespace}.Interfaces.IContentPipelineModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)", methodBuilder => methodBuilder
                .Line("throw new NotImplementedException();", indentation: 1))
            .NewLine()
            .Method($"public override void Write(Utf8JsonWriter writer, {SharedNamespace}.Interfaces.IContentPipelineModel value, JsonSerializerOptions options)", methodBuilder => methodBuilder
                .Line("switch (value)", indentation: 1)
                    .CodeBlock(end: "};")
                    .Tab()
                    .Foreach(contentClasses, (b, contentClass) => b.Line($"case {SharedNamespace}.Models.{contentClass.Group}.{contentClass.Name}PipelineModel model: JsonSerializer.Serialize<{SharedNamespace}.Models.{contentClass.Group}.{contentClass.Name}PipelineModel>(writer, model, options); break;"))
                    .Line("default:")
                        .Line("JsonSerializer.Serialize<object>(writer, value, jsonSerializerOptions);", indentation: 1)
                        .Line("break;"))
            .NewLine()
            .Line("public override bool HandleNull => false;")
            .Build();

        string CreateLinkJsonConverter(IEnumerable<ContentClass> contentClasses) =>
            CSharpCodeBuilder.Create()
            .Line("#nullable enable")
            .Using("System")
            .Using("System.Text.Json")
            .Using("System.Text.Json.Serialization")
            .Namespace($"{SharedNamespace}.JsonConverters")
            .Class($"internal class LinkPipelineModelJsonConverter : JsonConverter<{SharedNamespace}.Interfaces.ILinkPipelineModel>")
            .Tab()
            .Line("private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web)")
                .CodeBlock(b => b.Line("DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull", indentation: 1), end: "};")
            .NewLine()
            .Method($"public override {SharedNamespace}.Interfaces.ILinkPipelineModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)", methodBuilder => methodBuilder
                .Line("throw new NotImplementedException();", indentation: 1))
            .NewLine()
            .Method($"public override void Write(Utf8JsonWriter writer, {SharedNamespace}.Interfaces.ILinkPipelineModel value, JsonSerializerOptions options)", methodBuilder => methodBuilder
                .Line("switch (value)", indentation: 1)
                    .CodeBlock(end: "};")
                    .Tab()
                    //.Foreach(contentClasses, (b, contentClass) => b.Line($"case {SharedNamespace}.Models.{contentClass.Group}.{contentClass.Name}PipelineModel model: JsonSerializer.Serialize<{SharedNamespace}.Models.{contentClass.Group}.{contentClass.Name}PipelineModel>(writer, model, options); break;"))
                    .Line("default:")
                        .Line("JsonSerializer.Serialize<object>(writer, value, jsonSerializerOptions);", indentation: 1)
                        .Line("break;"))
            .NewLine()
            .Line("public override bool HandleNull => false;")
            .Build();
    }
}

