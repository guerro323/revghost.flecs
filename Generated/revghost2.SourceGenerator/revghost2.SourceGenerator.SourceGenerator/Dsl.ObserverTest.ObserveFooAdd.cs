
using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using flecs_hub;

namespace revghost2.Tests;


unsafe partial class ObserverTest
{
    unsafe partial struct ObserveFooAdd
    {
        public static ReadOnlySpan<__FLECS__.ecs_term_t> Terms => new __FLECS__.ecs_term_t[]
        {
            new()
            {
                id = (revghost2.StaticEntity<global::revghost2.Tests.ObserverTest.Foo>.Id),
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
        
        public global::revghost2.Entity Entity { get; }
        public global::revghost2.EntityId Id { get; }
        private struct __SYSTEM_STATE__
        {
        }
        public static unsafe void Setup(revghost2.World world)
        {
            var entity = __FLECS__.ecs_get_scope(world.Handle);
            var state = (__SYSTEM_STATE__*) NativeMemory.AllocZeroed((nuint) Unsafe.SizeOf<__SYSTEM_STATE__>());
            ProcessorUtility.SetupObserverManaged(world.Handle, entity, Filter, state, typeof(ObserveFooAdd), &EachUnmanaged);
            _ = nameof(Each); // reference it so the editor and compiler doesn't complain it's unused
        }
        [UnmanagedCallersOnly]
        private static unsafe void EachUnmanaged(__FLECS__.ecs_iter_t* __it__)
        {
            var __state__ = (__SYSTEM_STATE__*) __it__->ctx;
            var entities = __it__->entities;
            var c1 = (global::revghost2.Tests.ObserverTest.Foo*) __FLECS__.ecs_field_w_size(__it__, (ulong) Unsafe.SizeOf<global::revghost2.Tests.ObserverTest.Foo>(), 1);
            for (var i0 = 0; i0 < __it__->count; i0++)
            {
                {
            Console.WriteLine($"{Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref entities[i0])} -> Foo.Value={c1[i0].Value}");
        }
            }
        }
    }
}