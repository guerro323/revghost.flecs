using System.Reflection;
using System.Runtime.CompilerServices;

namespace revghost.flecs;

public static class ManagedTypeData<T>
{
    public static readonly int Size;
    public static readonly bool IsValueType;
    public static readonly bool ContainsReference;

    static ManagedTypeData()
    {
        Size = IsZeroSizeStruct(typeof(T)) ? 0 : Unsafe.SizeOf<T>();
        IsValueType = Size == 0 || typeof(T).IsValueType;
        ContainsReference = !IsValueType || RuntimeHelpers.IsReferenceOrContainsReferences<T>();
    }

    private static bool IsZeroSizeStruct(Type t)
    {
        return t.IsValueType && !t.IsPrimitive &&
               t.GetFields((BindingFlags) 0x34).All(fi => IsZeroSizeStruct(fi.FieldType));
    }
}