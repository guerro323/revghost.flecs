using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using flecs_hub;

namespace revghost2.Runner;

public unsafe class Prout
{
    public class Stuff
    {
        public int Foo, Bar;
    }
    
    public static void DoMyStuff()
    {
        flecs.ecs_component_desc_t
        
        var array = new Stuff[4];
        for (var i = 0; i < array.Length; i++)
            array[i] = new Stuff {Foo = i + 8, Bar = 1 + i * 2};

        var withHandles = new IntPtr[4];
        for (var i = 0; i < withHandles.Length; i++)
        {
            var target = GCHandle.Alloc(array[i], GCHandleType.Pinned).Target;
            var ptr = (IntPtr*) Unsafe.AsPointer(ref target);
            withHandles[i] = ptr[0];
        }

        var ret = Cast<IntPtr, Stuff>(withHandles);
        Console.WriteLine($"{ret[0].Foo} {ret[1].Bar}");
    }

    public static Span<TTo> Cast<TFrom, TTo>(Span<TFrom> span)
    {
        var caster = new CastTo<TFrom> {Cast = span};
        caster.Validation = 42;
        
        var ret = ((CastTo<TTo>*) &caster);
        if (ret->Validation != 42)
            throw new InvalidOperationException("wut m8");
        
        return ret->Cast;
    }
    
        public ref struct CastTo<T>
        {
            public int Validation;
            public Span<T> Cast;
        }
}