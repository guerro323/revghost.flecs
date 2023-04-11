
using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using flecs_hub;
using revghost2;
using revghost2.Components;
using revghost2.Utilities.Generator;

namespace revghost2.Runner;


unsafe partial class HierarchyTest
{
    unsafe partial struct WriteSystem
    {
        public static ReadOnlySpan<__FLECS__.ecs_term_t> Terms => new __FLECS__.ecs_term_t[]
        {
            new()
            {
                id = (revghost2.StaticEntity<global::revghost2.Runner.HierarchyTest.Position>.Id),
                inout = __FLECS__.ecs_inout_kind_t.EcsOut,
                src = new()
                {
                    id = default,
                    flags = __FLECS__.EcsIsEntity,
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
        public global::revghost2.Entity Entity { get; }
        public global::revghost2.EntityId Id { get; }
        private struct __SYSTEM_STATE__
        {
        }
        public static unsafe void Setup(revghost2.World world)
        {
            var entity = __FLECS__.ecs_get_scope(world.Handle);
            var state = (__SYSTEM_STATE__*) NativeMemory.AllocZeroed((nuint) Unsafe.SizeOf<__SYSTEM_STATE__>());
            ProcessorUtility.SetupSystemManaged(world.Handle, entity, Filter, state, typeof(WriteSystem), &EachUnmanaged);
            _ = nameof(Each); // reference it so the editor and compiler doesn't complain it's unused
        }
        [UnmanagedCallersOnly]
        private static unsafe void EachUnmanaged(__FLECS__.ecs_iter_t* __it__)
        {
            var __state__ = (__SYSTEM_STATE__*) __it__->ctx;
            var entities = __it__->entities;
            for (var i0 = 0; i0 < __it__->count; i0++)
            {
                {
            _ = Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref entities[i0]).WithWorld(global::revghost2.World.FromExistingUnsafe(__it__->world));
global::revghost2.World.FromExistingUnsafe(__it__->world).New();
        }
            }
        }
    }
}