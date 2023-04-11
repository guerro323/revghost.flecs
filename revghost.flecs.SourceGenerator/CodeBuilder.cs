using System.Text;

namespace revghost.flecs.SourceGenerator;

public class CodeBuilder
{
    public readonly StringBuilder StringBuilder = new();

    private int _scopeLevel = 0;

    public CodeBuilder()
    {
        
    }

    public CodeBuilder(CodeBuilder copy)
    {
        _scopeLevel = copy._scopeLevel;
    }
    
    public void BeginScope()
    {
        _scopeLevel++;
    }

    public void BeginBracket()
    {
        AppendLine("{");
        BeginScope();
    }

    public void EndBracket()
    {
        EndScope();
        AppendLine("}");
    }

    public string GetLinePrefix()
    {
        return $"\n{string.Join(null, Enumerable.Repeat("    ", _scopeLevel))}";
    }

    public void Append(char value) => StringBuilder.Append(value);
    public void Append(string value) => StringBuilder.Append(value);

    public void AppendLine()
    {
        StringBuilder.Append(GetLinePrefix());
    }
    
    public void AppendLine(string text)
    {
        StringBuilder.Append(GetLinePrefix());
        StringBuilder.Append(text);
    }

    public void EndScope()
    {
        _scopeLevel--;
    }

    public void FlushScope()
    {
        _scopeLevel = 0;
    }

    public void Flush()
    {
        while (_scopeLevel > 0)
            EndBracket();
    }

    public override string ToString()
    {
        Flush();
        return StringBuilder.ToString();
    }
}