
using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using flecs_hub;

namespace revghost2.Runner;


unsafe partial class TickSystems
{
    unsafe partial struct FastTick
    {
        public static ReadOnlySpan<__FLECS__.ecs_term_t> Terms => new __FLECS__.ecs_term_t[]
        {
            new()
            {
                id = (revghost2.StaticEntity<global::revghost2.Runner.TickSystems.PrintLog>.Id),
                inout = __FLECS__.ecs_inout_kind_t.EcsInOut,
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
            EcsSourceGenerator.SetupSystemManaged(world.Handle, entity, Filter, typeof(FastTick), &EachUnmanaged);
            _ = nameof(Each); // reference it so the editor and compiler doesn't complain it's unused
        }
        [UnmanagedCallersOnly]
        private static unsafe void EachUnmanaged(__FLECS__.ecs_iter_t* it)
        {
            var entities = it->entities;
            var c1 = (global::revghost2.Runner.TickSystems.PrintLog*) __FLECS__.ecs_field_w_size(it, (ulong) Unsafe.SizeOf<global::revghost2.Runner.TickSystems.PrintLog>(), 1);
            for (var i = 0; i < it->count; i++)
            {
                {
            Console.WriteLine("FastTick");
        }
            }
        }
    }
}