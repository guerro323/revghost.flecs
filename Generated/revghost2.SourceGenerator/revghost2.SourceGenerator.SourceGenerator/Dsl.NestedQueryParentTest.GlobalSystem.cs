
using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Runtime.CompilerServices;
using revghost2.Utilities.Generator;

namespace revghost2.Tests;


unsafe partial class NestedQueryParentTest
{
    unsafe partial struct GlobalSystem
    {
        public static ReadOnlySpan<__FLECS__.ecs_term_t> Terms => new __FLECS__.ecs_term_t[]
        {
            new()
            {
                id = (revghost2.StaticEntity<global::revghost2.Tests.NestedQueryParentTest.Position>.Id),
                inout = __FLECS__.ecs_inout_kind_t.EcsIn,
                oper = __FLECS__.ecs_oper_kind_t.EcsOptional,
                src = new()
                {
                    flags = new() { Data = __FLECS__.EcsParent|__FLECS__.EcsCascade },
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
            public __FLECS__.ecs_query_t* _query;
        }
        public static unsafe void Setup(revghost2.World world)
        {
            var entity = __FLECS__.ecs_get_scope(world.Handle);
            var state = (__SYSTEM_STATE__*) NativeMemory.AllocZeroed((nuint) Unsafe.SizeOf<__SYSTEM_STATE__>());
            {
                var queryDesc = new __FLECS__.ecs_query_desc_t()
                {
                    filter = global::revghost2.Tests.NestedQueryParentTest.GlobalSystem.NestedQuery.Filter
                };
                state->_query = __FLECS__.ecs_query_init(world.Handle, &queryDesc);
            }
            EcsSourceGenerator.SetupSystemManaged(world.Handle, entity, Filter, state, typeof(GlobalSystem), &EachUnmanaged);
            _ = nameof(Each); // reference it so the editor and compiler doesn't complain it's unused
        }
        [UnmanagedCallersOnly]
        private static unsafe void EachUnmanaged(__FLECS__.ecs_iter_t* __it__)
        {
            var __state__ = (__SYSTEM_STATE__*) __it__->ctx;
            var entities = __it__->entities;
            var c1 = (global::revghost2.Tests.NestedQueryParentTest.Position*) __FLECS__.ecs_field_w_size(__it__, (ulong) Unsafe.SizeOf<global::revghost2.Tests.NestedQueryParentTest.Position>(), 1);
            for (var i0 = 0; i0 < __it__->count; i0++)
            {
                {
var __it___query_noptr = __FLECS__.ecs_query_iter(__it__->world, __state__->_query);
var __it___query = (__FLECS__.ecs_iter_t*) Unsafe.AsPointer(ref __it___query_noptr);
while (__FLECS__.ecs_query_next(__it___query)) {
    
    var c_query1 = (global::revghost2.Tests.NestedQueryParentTest.Position*) __FLECS__.ecs_field_w_size(__it___query, (ulong) Unsafe.SizeOf<global::revghost2.Tests.NestedQueryParentTest.Position>(), 1);
    
    for (var i2 = 0; i2 < __it___query->count; i2++) {
        {
                if (Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref entities[i0])<= Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref __it___query->entities[i2]))
                    continue;

                if (c1 != null&& c_query1 != null)
                {
                    if (Vector2.Distance(c1[i0].Value, c_query1[i2].Value) < 1f)
                    {
                        Console.WriteLine($"{Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref entities[i0])} and {Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref __it___query->entities[i2])} has close parents!");
                    }
                }
            }
    }
}
        }
            }
        }
    }
}