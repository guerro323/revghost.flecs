
using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Runtime.InteropServices;
using revghost2.Utilities.Generator;

namespace revghost2.Tests;


unsafe partial class Tests
{
    unsafe partial struct EntryModule
    {
        unsafe partial struct Hello<T0>
        {
            public static ReadOnlySpan<__FLECS__.ecs_term_t> Terms => new __FLECS__.ecs_term_t[]
            {
                new()
                {
                    id = (revghost2.StaticEntity<T0>.Id),
                    inout = __FLECS__.ecs_inout_kind_t.EcsInOut,
                    src = new()
                    {
                    },
                },
                new()
                {
                    id = (revghost2.StaticEntity<global::revghost2.Tests.Tests.EntryModule.Position>.Id),
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
                ProcessorUtility.SetupSystemManaged(world.Handle, entity, Filter, state, typeof(Hello<T0>), (delegate*unmanaged<__FLECS__.ecs_iter_t*, void>) Marshal.GetFunctionPointerForDelegate(EachUnmanaged));
                _ = nameof(Each); // reference it so the editor and compiler doesn't complain it's unused
            }
            private static unsafe void EachUnmanaged(__FLECS__.ecs_iter_t* __it__)
            {
                var __state__ = (__SYSTEM_STATE__*) __it__->ctx;
                var entities = __it__->entities;
                var c1 = (T0*) __FLECS__.ecs_field_w_size(__it__, (ulong) Unsafe.SizeOf<T0>(), 1);
                for (var i0 = 0; i0 < __it__->count; i0++)
                {
                    {
                Console.WriteLine($"{typeof(T0).Name} for {Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref entities[i0])} is {c1[i0]}");
Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref __it__->entities[i0]).WithWorld(global::revghost2.World.FromExistingUnsafe(__it__->world)).Set(new Position { Value = new Vector3(4) });
            }
                }
            }
        }
    }
}