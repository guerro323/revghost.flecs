﻿
using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using flecs_hub;
using revghost2.Utilities.Generator;

namespace revghost2.Runner;


partial class SystemTest
{
    partial struct MoveSystem
    {
        public partial struct DslEntity
        {
            public readonly static string Expression = $"[inout]{(revghost2.StaticEntity<global::revghost2.Runner.SystemTest.Position>.FullPath)},\n[in]{(revghost2.StaticEntity<global::revghost2.Runner.SystemTest.Velocity>.FullPath)}";
            
            
            public global::revghost2.Runner.SystemTest.Position pos;
            public readonly global::revghost2.Runner.SystemTest.Velocity vel;
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
            var c1 = (global::revghost2.Runner.SystemTest.Position*) __FLECS__.ecs_field_w_size(it, (ulong) Unsafe.SizeOf<global::revghost2.Runner.SystemTest.Position>(), 1);
            var c2 = (global::revghost2.Runner.SystemTest.Velocity*) __FLECS__.ecs_field_w_size(it, (ulong) Unsafe.SizeOf<global::revghost2.Runner.SystemTest.Velocity>(), 2);
            for (var i = 0; i < it->count; i++)
            {
                {
            c1[i].Value += c2[i].Value;
        }
            }
        }
        private partial void Each(DslEntity entity);
    }
}