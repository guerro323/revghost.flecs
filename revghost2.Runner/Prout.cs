using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using flecs_hub;

namespace revghost2.Runner;

public unsafe class Prout
{
    public class Stuff
    {
        public int Foo, Bar;

        ~Stuff()
        {
            Console.WriteLine($"Finalized foo={Foo}, bar={Bar}");
        }
    }

    private static int _callCount = 0;

    private static IntPtr[] AllocateArrayOfStuff(int count)
    {
        var withHandles = new IntPtr[count];
        var array = new Stuff[count];
        for (var i = 0; i < array.Length; i++)
            array[i] = new Stuff {Foo = i + 8 + _callCount, Bar = 1 + i * 2};
            
        for (var i = 0; i < withHandles.Length; i++)
        {
            var target = GCHandle.Alloc(array[i], GCHandleType.Normal).Target;
            var ptr = (IntPtr*) Unsafe.AsPointer(ref target);
            withHandles[i] = ptr[0];
        }
        
        _callCount++;


        return withHandles;
    }
    
    public static void DoMyStuff()
    {
        var first = AllocateArrayOfStuff(100);
        var firstRet = Cast<IntPtr, Stuff>(first);
        for (var num = 0; num < 100; num++)
        {
            var withHandles = AllocateArrayOfStuff(100);
            for (var i = 0; i < 10; i++)
                GC.Collect();

            var ret = Cast<IntPtr, Stuff>(withHandles);
            Console.WriteLine($"{ret[0].Foo} {ret[1].Bar}   (first: {firstRet[0].Foo} {firstRet[1].Bar})");
        }
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