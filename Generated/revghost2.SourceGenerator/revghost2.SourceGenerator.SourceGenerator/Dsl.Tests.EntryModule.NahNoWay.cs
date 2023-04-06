
using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Runtime.InteropServices;

namespace revghost2.Tests;


unsafe partial class Tests
{
    unsafe partial struct EntryModule
    {
        unsafe partial struct NahNoWay
        {
            public static ReadOnlySpan<__FLECS__.ecs_term_t> Terms => new __FLECS__.ecs_term_t[]
            {
                new()
                {
                    id = (revghost2.StaticEntity<global::revghost2.Tests.Tests.EntryModule.Velocity>.Id),
                    inout = __FLECS__.ecs_inout_kind_t.EcsIn,
                    src = new()
                    {
                    },
                },
            };
            public static __FLECS__.ecs_filter_desc_t Filter = new()
            {
                terms_buffer = (__FLECS__.ecs_term_t*) Unsafe.AsPointer(ref MemoryMarshal.GetReference(Terms)),
                terms_buffer_count = Terms.Length,
            };
            
            public global::revghost2.World RealWorld { get; }
            public global::revghost2.World World { get; }
            public float SystemDeltaTime { get; }
            public global::revghost2.EntityId Entity { get; }
            public static unsafe void Setup(revghost2.World world)
            {
                var entity = __FLECS__.ecs_get_scope(world.Handle);
                EcsSourceGenerator.SetupSystemManaged(world.Handle, entity, Filter, typeof(NahNoWay), &EachUnmanaged);
                _ = nameof(Each); // reference it so the editor and compiler doesn't complain it's unused
            }
            [UnmanagedCallersOnly]
            private static unsafe void EachUnmanaged(__FLECS__.ecs_iter_t* it)
            {
                var entities = it->entities;
                var c1 = (global::revghost2.Tests.Tests.EntryModule.Velocity*) __FLECS__.ecs_field_w_size(it, (ulong) Unsafe.SizeOf<global::revghost2.Tests.Tests.EntryModule.Velocity>(), 1);
                for (var i = 0; i < it->count; i++)
                {
                    {
                Console.WriteLine($"hi {Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref entities[i])}");
            }
                }
            }
        }
    }
}