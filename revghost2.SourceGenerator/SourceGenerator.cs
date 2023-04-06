using System.Diagnostics;
using System.Text;
using ManiaGen.SourceGenerator.SubGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace revghost2.SourceGenerator;

[Generator]
public class SourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // retrieve the populated receiver
        if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
            return;

        var sw = new Stopwatch();
        void start() => sw.Restart();

        void stop(string name)
        {
            sw.Stop();
            receiver.Log.Add($"Elapsed ({name}) " + sw.Elapsed.TotalMilliseconds + "ms");
        }
        
        try
        {
            var compilation = context.Compilation;

            start();
            var scriptSub = SubGenerator.Make(new GenerateDslSubGenerator(), context, receiver, ref compilation);
            stop("scriptSub");
        }
        catch (Exception ex)
        {
            receiver.Log.Add(ex.ToString());
        }

        context.AddSource("Logs",
            SourceText.From(
                $@"/*{Environment.NewLine + string.Join(Environment.NewLine, receiver.Log) + Environment.NewLine}*/",
                Encoding.UTF8));
    }
}