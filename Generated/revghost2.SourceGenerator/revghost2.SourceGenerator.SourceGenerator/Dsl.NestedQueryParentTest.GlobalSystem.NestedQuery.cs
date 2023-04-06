
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
        unsafe partial struct NestedQuery
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
            
            public global::revghost2.Entity Entity { get; }
            public global::revghost2.EntityId Id { get; }
            ///<summary>
            /// Iterate entities from a filter
            ///</summary>
            ///<remarks>
            ///This should only be called in a System or a Processor
            ///</remarks>
            public Span<NestedQuery>.Enumerator GetEnumerator() => throw new global::System.Diagnostics.UnreachableException("This code should have been replaced");
        }
    }
}