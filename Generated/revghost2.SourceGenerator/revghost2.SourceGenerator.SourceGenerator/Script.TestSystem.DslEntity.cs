
using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using flecs_hub;
using revghost2.Utilities.Generator;

namespace revghost2.Runner;


partial class HierarchyTest
{
    partial struct TestSystem
    {
        public partial struct DslEntity
        {
            public readonly static string Expression = $"[in]({(revghost2.StaticEntity<global::revghost2.Runner.HierarchyTest.Local>.FullPath)}, {(revghost2.StaticEntity<global::revghost2.Runner.HierarchyTest.Position>.FullPath)}),\n[inout]({(revghost2.StaticEntity<global::revghost2.Runner.HierarchyTest.World>.FullPath)}, {(revghost2.StaticEntity<global::revghost2.Runner.HierarchyTest.Position>.FullPath)}),\n[in]({(revghost2.StaticEntity<global::revghost2.Runner.HierarchyTest.World>.FullPath)}, {(revghost2.StaticEntity<global::revghost2.Runner.HierarchyTest.Position>.FullPath)})";
            
            
            public readonly global::revghost2.Runner.HierarchyTest.Position localPos;
            public global::revghost2.Runner.HierarchyTest.Position worldPos;
            public readonly global::revghost2.Runner.HierarchyTest.Position parentPos;
        }
        public static unsafe void Setup(revghost2.World world)
        {
            var entity = __FLECS__.ecs_get_scope(world.Handle);
            EcsSourceGenerator.SetupSystemManaged(world.Handle, entity, DslEntity.Expression, &EachUnmanaged);
            _ = nameof(Each); // reference it so the editor and compiler doesn't complain it's unused
        }
        [UnmanagedCallersOnly]
        private static unsafe void EachUnmanaged(__FLECS__.ecs_iter_t* it)
        {
            var entities = it->entities;
            var c1 = (global::revghost2.Runner.HierarchyTest.Position*) __FLECS__.ecs_field_w_size(it, (ulong) Unsafe.SizeOf<global::revghost2.Runner.HierarchyTest.Position>(), 1);
            var c2 = (global::revghost2.Runner.HierarchyTest.Position*) __FLECS__.ecs_field_w_size(it, (ulong) Unsafe.SizeOf<global::revghost2.Runner.HierarchyTest.Position>(), 2);
            var c3 = (global::revghost2.Runner.HierarchyTest.Position*) __FLECS__.ecs_field_w_size(it, (ulong) Unsafe.SizeOf<global::revghost2.Runner.HierarchyTest.Position>(), 3);
            for (var i = 0; i < it->count; i++)
            {
                {
            // entity.worldPos = entity.localPos;
            // if (entity.parentPos is { } parentPos)
            // {
            //     entity.worldPos.Value += parentPos.Value;
            // }
        }
            }
        }
        private partial void Each(DslEntity entity);
    }
}