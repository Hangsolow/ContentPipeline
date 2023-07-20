using System.Text;

namespace ContentPipeline.CodeBuilders;

public class CSharpCodeBuilder : IDisposable
{
    public CSharpCodeBuilder(StringBuilder codeBuilder, string start, string end = "", int indentation = 0)
    {
        CodeBuilder = codeBuilder ?? throw new ArgumentNullException(nameof(codeBuilder));
        End = end;
        _ = start ?? throw new ArgumentNullException(nameof(codeBuilder));
        Indentation = indentation;
        Spaces = indentation > 0 ? new string(' ', 4 * Indentation) : string.Empty;

        if (string.IsNullOrEmpty(start) is not true)
        {
            if (start.Equals(CodeConsts.NewLine, StringComparison.OrdinalIgnoreCase))
            {
                CodeBuilder.AppendLine(Spaces);
            }
            else
            {
                CodeBuilder.AppendLine(Spaces + start);
            }
        }

    }

    public StringBuilder CodeBuilder { get; }
    public string End { get; init; }
    public int Indentation { get; set; }

    public string Spaces { get; }

    public virtual void Dispose()
    {
        if (string.IsNullOrEmpty(End) is not true)
        {
            CodeBuilder.AppendLine(Spaces + End);
        }
    }

    public static CSharpCodeBuilder Create()
    {
        return new CSharpCodeBuilder(new StringBuilder(), string.Empty);
    }
}

public class CSharpCodeBuilder<T> : CSharpCodeBuilder
{
    public CSharpCodeBuilder(StringBuilder codeBuilder, string start, string end = "", int indentation = 0, CSharpCodeBuilder? parent = null) : base(codeBuilder, start, end, indentation)
    {
        Parent = parent;
    }

    public CSharpCodeBuilder? Parent { get; }

    public override void Dispose()
    {
        base.Dispose();

        if (Parent is not null)
        {
            Parent.Dispose();
            Indentation = Parent.Indentation;
        }
    }
}

public static class CSharpCodeBuilderExtentions
{
    /// <summary>
    /// Adds a using statement
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="usingNamespace"></param>
    /// <returns></returns>
    public static CSharpCodeBuilder Using(this CSharpCodeBuilder builder, string usingNamespace)
    {
        return new CSharpCodeBuilder(builder.CodeBuilder, $"using {usingNamespace};");
    }

    /// <summary>
    /// Adds Namespace
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="namespace"></param>
    /// <returns></returns>
    public static CSharpCodeBuilder<Namespace> Namespace(this CSharpCodeBuilder builder, string @namespace)
    {
        builder.CodeBuilder.AppendLine();
        return new CSharpCodeBuilder<Namespace>(builder.CodeBuilder, $"namespace {@namespace}").CodeBlock();
    }

    /// <summary>
    /// Adds a Class
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="className"></param>
    /// <returns></returns>
    public static CSharpCodeBuilder<Class> Class(this CSharpCodeBuilder<Namespace> builder, string className)
    {
        return new CSharpCodeBuilder<Class>(builder.CodeBuilder, $"{className}", "", builder.Indentation + 1, builder).CodeBlock();
    }

    /// <summary>
    /// Adds a property
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="propertyName"></param>
    /// <param name="propertyType"></param>
    /// <param name="setter"></param>
    /// <returns></returns>
    public static CSharpCodeBuilder<Class> Property(this CSharpCodeBuilder<Class> builder, string propertyName, string propertyType, bool setter = false, bool isPublic = true)
    {
        var getAndSetter = setter ? "get; set;" : "get;";
        var accessor = isPublic ? "public" : "private";
        return new CSharpCodeBuilder<Class>(builder.CodeBuilder, $"{accessor} {propertyType} {propertyName} {{ {getAndSetter} }}", indentation: builder.Indentation, parent: builder);
    }

    public static CSharpCodeBuilder<Class> Method(this CSharpCodeBuilder<Class> builder, string method, Func<CSharpCodeBuilder<Method>, CSharpCodeBuilder<Method>> methodFunc)
    {
        var methodBuilder = new CSharpCodeBuilder<Method>(builder.CodeBuilder, method, indentation: builder.Indentation).CodeBlock();
        methodBuilder = methodFunc.Invoke(methodBuilder);
        methodBuilder.End();

        return builder;
    }

    public static CSharpCodeBuilder<T> NewLine<T>(this CSharpCodeBuilder<T> builder)
    {
        return new CSharpCodeBuilder<T>(builder.CodeBuilder, CodeConsts.NewLine, indentation: builder.Indentation, parent: builder);
    }

    public static CSharpCodeBuilder<T> Line<T>(this CSharpCodeBuilder<T> builder, string line = "", int indentation = 0)
    {
        return new CSharpCodeBuilder<T>(builder.CodeBuilder, line, indentation: builder.Indentation + indentation, parent: builder);
    }

    public static CSharpCodeBuilder Line(this CSharpCodeBuilder builder, string line = "", int indentation = 0)
    {
        return new CSharpCodeBuilder(builder.CodeBuilder, line, indentation: builder.Indentation + indentation);
    }

    public static CSharpCodeBuilder<T> Tab<T>(this CSharpCodeBuilder<T> builder, int indentation = 1)
    {
        return new CSharpCodeBuilder<T>(builder.CodeBuilder, string.Empty, indentation: builder.Indentation + indentation, parent: builder);
    }

    public static CSharpCodeBuilder<T> CodeBlock<T>(this CSharpCodeBuilder<T> builder, int indentation = 0, string? end = null)
    {
        return new CSharpCodeBuilder<T>(builder.CodeBuilder, "{", end ?? "}", indentation: builder.Indentation + indentation, builder);
    }

    public static CSharpCodeBuilder<T> CodeBlock<T>(this CSharpCodeBuilder<T> builder, Func<CSharpCodeBuilder<T>, CSharpCodeBuilder<T>> codeBlockFunc, int indentation = 0, string? end = null)
    {
        var codeBlockBuilder = new CSharpCodeBuilder<T>(builder.CodeBuilder, string.Empty, indentation: builder.Indentation).CodeBlock(end: end);
        codeBlockBuilder = codeBlockFunc.Invoke(codeBlockBuilder);
        codeBlockBuilder.End();

        return builder;
    }

    public static CSharpCodeBuilder<T> Foreach<T, TValue>(this CSharpCodeBuilder<T> builder, IEnumerable<TValue> list, Func<CSharpCodeBuilder<T>, TValue, CSharpCodeBuilder<T>> foreachFunc)
    {
        var foreachBuilder = builder;

        foreach (var value in list)
        {
            foreachBuilder = foreachFunc.Invoke(foreachBuilder, value);
        }

        return foreachBuilder;
    }

    /// <summary>
    /// Ends the builder, intended for use in Builder methods that uses Func's to limit scopes for things like CodeBlock
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    private static void End<T>(this CSharpCodeBuilder<T> builder)
    {
        builder.Dispose();
    }

    /// <summary>
    /// returns the build result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    public static string Build<T>(this CSharpCodeBuilder<T> builder)
    {
        builder.Dispose();
        return builder.CodeBuilder.ToString();
    }

}

public class Namespace { }
public class Class { }
public class Method { }
