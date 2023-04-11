using Microsoft.CodeAnalysis;

namespace revghost.flecs.SourceGenerator;

public static class TypeSymbolExtension
{
    public static string GetTypeName(this ITypeSymbol type)
    {
        return (type.TypeKind is TypeKind.TypeParameter || type.SpecialType is > 0 and <= SpecialType.System_String
            ? type.ToString()
            : $"global::{type}")!;
    }
    
    public static string GetTypeNameWithoutAnnotations(this ITypeSymbol type)
    {
        return GetTypeName(type).Replace("?", string.Empty);
    }
}