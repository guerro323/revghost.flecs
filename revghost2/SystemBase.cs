using System.Text;
using static flecs_hub.flecs;

namespace revghost2;

/// <summary>
/// Provide an abstraction for easily implementing <see cref="ISystem"/> on top of a class
/// </summary>
public abstract class SystemBase : IDisposable
{
    public static Entity Build(ref SystemBase self, World world)
    {
        if (self is null)
            throw new NullReferenceException("A managed system must be constructed beforehand");

        if (self._entity is { } entity && entity.World != world)
            throw new InvalidOperationException("An entity was given to be this system one, but is from another world.");

        return default;
    }

    protected readonly World World;

    public SystemBase(World world)
    {
        World = world;
    }

    private Entity? _entity;
    private (StringBuilder builder, string? cached) _filter = new();

    public ReadOnlySpan<char> Expression
    {
        get
        {
            if (_filter.cached is { } cached)
                return cached;

            throw new InvalidOperationException($"The system should have been built (current expression = {_filter.builder})");
        }
    }

    protected SystemBase SetSystemEntity(Entity entity)
    {
        _entity = entity;
        return this;
    }

    private void BeginExpr()
    {
        if (_filter.builder.Length > 0)
            _filter.builder.Append(", ");
    }

    protected SystemBase Write<T>()
        where T : IComponent
    {
        BeginExpr();
        // _filter.builder.Append(World.Get<T>().Name());
        
        return this;
    }

    ~SystemBase()
    {
        ReleaseUnmanagedResources();
    }

    private void ReleaseUnmanagedResources()
    {
        
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
}