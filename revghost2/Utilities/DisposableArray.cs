using System.Buffers;

namespace revghost2;

public struct DisposableArray<T> : IDisposable
{
    public T[] UnsafeArray;
    public int Length;

    public Span<T> Span => UnsafeArray.AsSpan(0, Length);

    public static DisposableArray<T> Rent(int size)
    {
        return new DisposableArray<T> {UnsafeArray = ArrayPool<T>.Shared.Rent(size), Length = size};
    }

    public static DisposableArray<T> Rent(int size, out T[] bytes)
    {
        return new DisposableArray<T> {UnsafeArray = bytes = ArrayPool<T>.Shared.Rent(size), Length = size};
    }

    public void Dispose()
    {
        if (UnsafeArray == null) return;
        
        ArrayPool<T>.Shared.Return(UnsafeArray);
    }
}