using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using flecs_hub;

namespace revghost2;

public unsafe ref struct NativeStringView
{
    public ref readonly byte FirstChar;

    public readonly bool IsNull => Unsafe.IsNullRef(ref Unsafe.AsRef(in FirstChar));
    
    public readonly int Length
    {
        get
        {
            if (IsNull)
                return 0;
            
            for (var i = 0;; i++)
            {
                if (Unsafe.Add(ref Unsafe.AsRef(in FirstChar), i) == 0)
                    return i;
            }
        }
    }
    
    public readonly ReadOnlySpan<byte> CharsUtf8
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (IsNull)
                return default;
            
            return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in FirstChar), Length);
        }
    }

    public readonly DisposableArray<char> DisposableChars
    {
        get
        {
            if (Length == 0)
                return default;
            
            var utf8 = CharsUtf8;
            var disposable = DisposableArray<char>.Rent(utf8.Length, out var array);
            Encoding.UTF8.GetChars(utf8, array);

            return disposable;
        }
    }

    public override string ToString()
    {
        var disposable = DisposableChars;
        var str = disposable.Span.ToString();
        disposable.Dispose();

        return str;
    }

    public NativeStringView(nint ptr)
    {
        FirstChar = ref *(byte*) ptr;
    }
}