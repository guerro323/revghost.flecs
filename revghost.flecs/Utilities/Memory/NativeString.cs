using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace revghost.flecs;

using flecs = flecs_hub.flecs;

public unsafe ref struct NativeString
{
    public ref byte FirstChar;

    public byte[] DisposableArray;

    public bool NeedsToBeDisposed => DisposableArray != null;
    public bool IsNull => Unsafe.IsNullRef(ref FirstChar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NativeString(ReadOnlySpan<byte> utf8)
    {
        if (utf8.Length == 0)
        {
            FirstChar = ref Unsafe.NullRef<byte>();
            return;
        }

        FirstChar = ref Unsafe.AsRef(in utf8[0]);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NativeString(ReadOnlySpan<char> utf16)
    {
        if (utf16.Length == 0)
        {
            FirstChar = ref Unsafe.NullRef<byte>();
            return;
        }

        var size = Encoding.UTF8.GetByteCount(utf16);
        DisposableArray = ArrayPool<byte>.Shared.Rent(size + 1);
        
        Encoding.UTF8.GetBytes(utf16, DisposableArray.AsSpan(0, size));
        DisposableArray[size] = 0;

        FirstChar = ref DisposableArray[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator flecs.Runtime.CString(NativeString native)
    {
        flecs.Runtime.CString cString;
        cString._pointer = (IntPtr) Unsafe.AsPointer(ref native.FirstChar);
        return cString;
    }

    public void Dispose()
    {
        if (NeedsToBeDisposed)
            ArrayPool<byte>.Shared.Return(DisposableArray);
    }
}