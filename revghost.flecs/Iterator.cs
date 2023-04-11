using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static flecs_hub.flecs;

namespace revghost.flecs;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct Iterator
{
    public ecs_iter_t* Handle;

    public World World => World.FromExistingUnsafe(Handle->world);
    public World RealWorld => World.FromExistingUnsafe(Handle->real_world);
    public float DeltaTime => Handle->delta_time;
    public float DeltaSystemTime => Handle->delta_system_time;

    public ReadOnlySpan<EntityId> Entities => new Span<EntityId>(Handle->entities, Handle->count);

    public Span<T> Field<T>(int index)
        where T : IComponent
    {
        return new Span<T>(ecs_field_w_size(Handle, (ulong) Unsafe.SizeOf<T>(), index), Handle->count);
    }
}