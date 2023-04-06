
using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using flecs_hub;
using revghost2.Utilities.Generator;

namespace revghost2.Runner;


unsafe partial class SystemTest
{
    unsafe partial struct MoveSystem3
    {
        public static ReadOnlySpan<__FLECS__.ecs_term_t> Terms => new __FLECS__.ecs_term_t[]
        {
            new()
            {
                id = (revghost2.StaticEntity<global::revghost2.Runner.SystemTest.Position>.Id),
                inout = __FLECS__.ecs_inout_kind_t.EcsInOut,
                src = new()
                {
                },
            },
            new()
            {
                id = (revghost2.StaticEntity<global::revghost2.Runner.SystemTest.Velocity>.Id),
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
        
        public float DeltaTime { get; }
        public float SystemDeltaTime { get; }
        public EntityId Entity { get; }
        public static unsafe void Setup(revghost2.World world)
        {
            var entity = __FLECS__.ecs_get_scope(world.Handle);
            EcsSourceGenerator.SetupSystemManaged(world.Handle, entity, Filter, typeof(MoveSystem3), &EachUnmanaged);
            _ = nameof(Each); // reference it so the editor and compiler doesn't complain it's unused
        }
        [UnmanagedCallersOnly]
        private static unsafe void EachUnmanaged(__FLECS__.ecs_iter_t* it)
        {
            var entities = it->entities;
            var c1 = (global::revghost2.Runner.SystemTest.Position*) __FLECS__.ecs_field_w_size(it, (ulong) Unsafe.SizeOf<global::revghost2.Runner.SystemTest.Position>(), 1);
            var c2 = (global::revghost2.Runner.SystemTest.Velocity*) __FLECS__.ecs_field_w_size(it, (ulong) Unsafe.SizeOf<global::revghost2.Runner.SystemTest.Velocity>(), 2);
            for (var i = 0; i < it->count; i++)
            {
                {
            c1[i].Value += c2[i].Value * it->delta_system_time;
        }
            }
        }
    }
}