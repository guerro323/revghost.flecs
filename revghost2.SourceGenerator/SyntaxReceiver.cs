using Microsoft.CodeAnalysis;

namespace revghost2.SourceGenerator;

public class SyntaxReceiver : ISyntaxContextReceiver
{
    public readonly List<string> Log = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        
    }
}