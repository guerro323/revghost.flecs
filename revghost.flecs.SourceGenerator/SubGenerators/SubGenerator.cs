using Microsoft.CodeAnalysis;

namespace revghost.flecs.SourceGenerator.SubGenerators;

public abstract class SubGenerator
{
    public GeneratorExecutionContext Context { get; private set; }
    public SyntaxReceiver Receiver { get; private set; }

    // settable
    public Compilation Compilation { get; protected set; }

    protected virtual void Log<T>(T obj, int indent = 0)
    {
        Receiver.Log.Add(string.Join(null, Enumerable.Repeat("\t", indent)) + obj);
    }

    protected abstract void Generate();

    public static T Make<T>(T generator,
        GeneratorExecutionContext context, SyntaxReceiver receiver, ref Compilation compilation)
        where T : SubGenerator
    {
        generator.Context = context;
        generator.Receiver = receiver;
        generator.Compilation = compilation;

        try
        {
            generator.Generate();
        }
        catch (IgnoreFailFastException)
        {
            // ignored
        }

        return generator;
    }

    protected void ThrowError(string message, int code = 0, string title = "Unexpected error!", string? description = null, Location? location = null)
    {
        Context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
            $"MG{code}",
            title,
            message,
            "?",
            DiagnosticSeverity.Error,
            true,
            description
        ), location));
        throw new IgnoreFailFastException();
    }

    protected class IgnoreFailFastException : Exception
    {
    }
}