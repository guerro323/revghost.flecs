using System.Buffers;

namespace revghost.flecs;

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
    
    public static DisposableArray<T> Create(ReadOnlySpan<T> span)
    {
        var dArray = Rent(span.Length);
        span.CopyTo(dArray.Span);

        return dArray;
    }
    
    public static unsafe DisposableArray<T> Create(T* buffer, int length, int additionalSize)
    {
        var dArray = Rent(length + additionalSize);
        new Span<T>(buffer, length).CopyTo(dArray.Span);

        return dArray;
    }

    public void Dispose()
    {
        if (UnsafeArray == null) return;
        
        ArrayPool<T>.Shared.Return(UnsafeArray);
    }
}