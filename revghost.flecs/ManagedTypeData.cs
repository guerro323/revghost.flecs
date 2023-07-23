using System.Reflection;
using System.Runtime.CompilerServices;

namespace revghost.flecs;

public static class ManagedTypeData<T>
{
    public static readonly string CSharpName;
    public static readonly int Size;
    public static readonly int Alignment;
    public static readonly bool IsValueType;
    public static readonly bool ContainsReference;

    static ManagedTypeData()
    {
        CSharpName = typeof(T).Name;
        if (CSharpName.IndexOf('`') is var index and >= 0)
        {
            CSharpName = CSharpName[..index];
            if (typeof(T).GetGenericArguments() is { } args)
            {
                foreach (var arg in args)
                {
                    CSharpName += $"_{arg.Name}";
                }
            }
        }
        
        Size = IsZeroSizeStruct(typeof(T)) ? 0 : Unsafe.SizeOf<T>();
        IsValueType = Size == 0 || typeof(T).IsValueType;
        ContainsReference = !IsValueType || RuntimeHelpers.IsReferenceOrContainsReferences<T>();

        if (ContainsReference)
            Alignment = 4; // "default" alignment
        else
        {
            Alignment = Size == 0 ? 0 : 4;
            foreach (var field in typeof(T).GetFields((BindingFlags) 0x34))
            {
                var align = 0;
                // integers
                if (field.FieldType == typeof(byte) || field.FieldType == typeof(sbyte))
                    align = 1;
                else if (field.FieldType == typeof(ushort) || field.FieldType == typeof(short))
                    align = 2;
                else if (field.FieldType == typeof(uint) || field.FieldType == typeof(int))
                    align = 4;
                else if (field.FieldType == typeof(ulong) || field.FieldType == typeof(long))
                    align = 8;
                else if (field.FieldType == typeof(nuint) || field.FieldType == typeof(nint))
                    align = Unsafe.SizeOf<nint>();
                // floating
                else if (field.FieldType == typeof(Half))
                    align = 2;
                else if (field.FieldType == typeof(float))
                    align = 4;
                else if (field.FieldType == typeof(double))
                    align = 8;
                else
                {
                    align = (int) typeof(ManagedTypeData<>)
                        .MakeGenericType(field.FieldType)
                        .GetField(nameof(Alignment))!
                        .GetValue(null)!;
                }

                Alignment = Math.Max(align, Alignment);
            }
        }
    }

    private static bool IsZeroSizeStruct(Type t)
    {
        return t.IsValueType && !t.IsPrimitive &&
               t.GetFields((BindingFlags) 0x34).All(fi => IsZeroSizeStruct(fi.FieldType));
    }
}