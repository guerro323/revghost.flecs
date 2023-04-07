/*
Generate DSLs
	Work on worldPos (+Parent worldPos = localPos)
	.worldPos == Entity
	.worldPos == Id
	.worldPos == SystemDeltaTime
	.worldPos == DeltaTime
	.worldPos == World
	.worldPos == RealWorld
	.worldPos == localPos
	.worldPos == worldPos
	Work on localPos (+Parent worldPos = localPos)
	.localPos == Entity
	.localPos == Id
	.localPos == SystemDeltaTime
	.localPos == DeltaTime
	.localPos == World
	.localPos == RealWorld
	.localPos == localPos
	Span: [828..841)
	.maybeParentPos == Entity
	.maybeParentPos == Id
	.maybeParentPos == SystemDeltaTime
	.maybeParentPos == DeltaTime
	.maybeParentPos == World
	.maybeParentPos == RealWorld
	.maybeParentPos == localPos
	.maybeParentPos == worldPos
	.maybeParentPos == maybeParentPos
	 searching: worldPos.Value
	  maybe: revghost2.Runner.HierarchyTest.TestSystem.worldPos (worldPos.Value)
	   candidate found... containing=revghost2.Runner.HierarchyTest.TestSystem
	   predicted = revghost2.Runner.HierarchyTest.TestSystem.worldPos
	>> worldPos.Value -> (, revghost2.SourceGenerator.FieldInfo)
	.worldPos == Entity
	.worldPos == Id
	.worldPos == SystemDeltaTime
	.worldPos == DeltaTime
	.worldPos == World
	.worldPos == RealWorld
	.worldPos == localPos
	.worldPos == worldPos
	Component Access .worldPos.1
	replace with c2[i0].Value
	 searching: parentPos.Value
	  maybe: parentPos (parentPos.Value)
	   candidate found... containing=revghost2.Runner.HierarchyTest.TestSystem
	Parent pattern span: [828..841) True
	>> parentPos.Value -> (, revghost2.SourceGenerator.FieldInfo)
	.maybeParentPos == Entity
	.maybeParentPos == Id
	.maybeParentPos == SystemDeltaTime
	.maybeParentPos == DeltaTime
	.maybeParentPos == World
	.maybeParentPos == RealWorld
	.maybeParentPos == localPos
	.maybeParentPos == worldPos
	.maybeParentPos == maybeParentPos
	Component Access .maybeParentPos.2
	replace with c3[i0].Value

using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using flecs_hub;
using revghost2.Components;
using revghost2.Utilities.Generator;

namespace revghost2.Runner;


unsafe partial class HierarchyTest
{
    unsafe partial struct TestSystem
    {
        public static ReadOnlySpan<__FLECS__.ecs_term_t> Terms => new __FLECS__.ecs_term_t[]
        {
            new()
            {
                id = __FLECS__.ecs_pair((revghost2.StaticEntity<global::revghost2.Runner.HierarchyTest.Local>.Id),(revghost2.StaticEntity<global::revghost2.Runner.HierarchyTest.Position>.Id)),
                inout = __FLECS__.ecs_inout_kind_t.EcsIn,
                src = new()
                {
                },
            },
            new()
            {
                id = __FLECS__.ecs_pair((revghost2.StaticEntity<global::revghost2.Runner.HierarchyTest.World>.Id),(revghost2.StaticEntity<global::revghost2.Runner.HierarchyTest.Position>.Id)),
                inout = __FLECS__.ecs_inout_kind_t.EcsInOut,
                src = new()
                {
                },
            },
            new()
            {
                id = __FLECS__.ecs_pair((revghost2.StaticEntity<global::revghost2.Runner.HierarchyTest.World>.Id),(revghost2.StaticEntity<global::revghost2.Runner.HierarchyTest.Position>.Id)),
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
        }
        public static unsafe void Setup(revghost2.World world)
        {
            var entity = __FLECS__.ecs_get_scope(world.Handle);
            var state = (__SYSTEM_STATE__*) NativeMemory.AllocZeroed((nuint) Unsafe.SizeOf<__SYSTEM_STATE__>());
            EcsSourceGenerator.SetupSystemManaged(world.Handle, entity, Filter, state, typeof(__THIS__), &EachUnmanaged);
            _ = nameof(Each); // reference it so the editor and compiler doesn't complain it's unused
        }
        [UnmanagedCallersOnly]
        private static unsafe void EachUnmanaged(__FLECS__.ecs_iter_t* __it__)
        {
            var __state__ = (__SYSTEM_STATE__*) __it__->ctx;
            var entities = __it__->entities;
            var c1 = (global::revghost2.Runner.HierarchyTest.Position*) __FLECS__.ecs_field_w_size(__it__, (ulong) Unsafe.SizeOf<global::revghost2.Runner.HierarchyTest.Position>(), 1);
            var c2 = (global::revghost2.Runner.HierarchyTest.Position*) __FLECS__.ecs_field_w_size(__it__, (ulong) Unsafe.SizeOf<global::revghost2.Runner.HierarchyTest.Position>(), 2);
            var c3 = (global::revghost2.Runner.HierarchyTest.Position*) __FLECS__.ecs_field_w_size(__it__, (ulong) Unsafe.SizeOf<global::revghost2.Runner.HierarchyTest.Position>(), 3);
            for (var i0 = 0; i0 < __it__->count; i0++)
            {
                {
            c2[i0] = c1[i0];
            if (c3 != null)
            {
                c2[i0] .Value += c3[i0].Value;
            }
        }
            }
        }
    }
}
	 searching: pos.Value
	  maybe: revghost2.Runner.SystemTest.MoveSystem.pos (pos.Value)
	   candidate found... containing=revghost2.Runner.SystemTest.MoveSystem
	   predicted = revghost2.Runner.SystemTest.MoveSystem.pos
	>> pos.Value -> (, revghost2.SourceGenerator.FieldInfo)
	.pos == Entity
	.pos == Id
	.pos == SystemDeltaTime
	.pos == DeltaTime
	.pos == World
	.pos == RealWorld
	.pos == pos
	Component Access .pos.0
	replace with c1[i0].Value
	 searching: vel.Value
	  maybe: revghost2.Runner.SystemTest.MoveSystem.vel (vel.Value)
	   candidate found... containing=revghost2.Runner.SystemTest.MoveSystem
	   predicted = revghost2.Runner.SystemTest.MoveSystem.vel
	>> vel.Value -> (, revghost2.SourceGenerator.FieldInfo)
	.vel == Entity
	.vel == Id
	.vel == SystemDeltaTime
	.vel == DeltaTime
	.vel == World
	.vel == RealWorld
	.vel == pos
	.vel == vel
	Component Access .vel.1
	replace with c2[i0].Value
	Work on SystemDeltaTime (+Parent vel.Value * SystemDeltaTime)
	.SystemDeltaTime == Entity
	.SystemDeltaTime == Id
	.SystemDeltaTime == SystemDeltaTime

using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using flecs_hub;
using revghost2.Utilities.Generator;

namespace revghost2.Runner;


unsafe partial class SystemTest
{
    unsafe partial struct MoveSystem
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
            EcsSourceGenerator.SetupSystemManaged(world.Handle, entity, Filter, state, typeof(__THIS__), &EachUnmanaged);
            _ = nameof(Each); // reference it so the editor and compiler doesn't complain it's unused
        }
        [UnmanagedCallersOnly]
        private static unsafe void EachUnmanaged(__FLECS__.ecs_iter_t* __it__)
        {
            var __state__ = (__SYSTEM_STATE__*) __it__->ctx;
            var entities = __it__->entities;
            var c1 = (global::revghost2.Runner.SystemTest.Position*) __FLECS__.ecs_field_w_size(__it__, (ulong) Unsafe.SizeOf<global::revghost2.Runner.SystemTest.Position>(), 1);
            var c2 = (global::revghost2.Runner.SystemTest.Velocity*) __FLECS__.ecs_field_w_size(__it__, (ulong) Unsafe.SizeOf<global::revghost2.Runner.SystemTest.Velocity>(), 2);
            for (var i0 = 0; i0 < __it__->count; i0++)
            {
                {
            c1[i0] .Value += c2[i0] .Value * __it__->delta_system_time;
        }
            }
        }
    }
}
	 searching: Console.WriteLine
	  maybe: System.Console (Console.WriteLine)
	   candidate found... containing=
	Work on Console (+Parent Console.WriteLine)
	Work on WriteLine (+Parent Console.WriteLine)
	 searching: pos.Value
	  maybe: revghost2.Runner.SystemTest.PrintPositionSystem.pos (pos.Value)
	   candidate found... containing=revghost2.Runner.SystemTest.PrintPositionSystem
	   predicted = revghost2.Runner.SystemTest.PrintPositionSystem.pos
	>> pos.Value -> (, revghost2.SourceGenerator.FieldInfo)
	.pos == Entity
	.pos == Id
	.pos == SystemDeltaTime
	.pos == DeltaTime
	.pos == World
	.pos == RealWorld
	.pos == pos
	Component Access .pos.0
	replace with c1[i0].Value

using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using flecs_hub;
using revghost2.Utilities.Generator;

namespace revghost2.Runner;


unsafe partial class SystemTest
{
    unsafe partial struct PrintPositionSystem
    {
        public static ReadOnlySpan<__FLECS__.ecs_term_t> Terms => new __FLECS__.ecs_term_t[]
        {
            new()
            {
                id = (revghost2.StaticEntity<global::revghost2.Runner.SystemTest.Position>.Id),
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
        public global::revghost2.Entity Entity { get; }
        public global::revghost2.EntityId Id { get; }
        private struct __SYSTEM_STATE__
        {
        }
        public static unsafe void Setup(revghost2.World world)
        {
            var entity = __FLECS__.ecs_get_scope(world.Handle);
            var state = (__SYSTEM_STATE__*) NativeMemory.AllocZeroed((nuint) Unsafe.SizeOf<__SYSTEM_STATE__>());
            EcsSourceGenerator.SetupSystemManaged(world.Handle, entity, Filter, state, typeof(__THIS__), &EachUnmanaged);
            _ = nameof(Each); // reference it so the editor and compiler doesn't complain it's unused
        }
        [UnmanagedCallersOnly]
        private static unsafe void EachUnmanaged(__FLECS__.ecs_iter_t* __it__)
        {
            var __state__ = (__SYSTEM_STATE__*) __it__->ctx;
            var entities = __it__->entities;
            var c1 = (global::revghost2.Runner.SystemTest.Position*) __FLECS__.ecs_field_w_size(__it__, (ulong) Unsafe.SizeOf<global::revghost2.Runner.SystemTest.Position>(), 1);
            for (var i0 = 0; i0 < __it__->count; i0++)
            {
                {
            Console.WriteLine(c1[i0].Value);
        }
            }
        }
    }
}
Attribute WriteAttribute found
	gen: revghost2.Runner.HierarchyTest.Position (Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.NonErrorNamedTypeSymbol)
Finish term? True, Name: WriteAttribute
Finished current term
	Work on _ (+Parent _ = Entity)
	._ == Entity
	._ == Id
	._ == SystemDeltaTime
	._ == DeltaTime
	._ == World
	._ == RealWorld
	Work on Entity (+Parent _ = Entity)
	.Entity == Entity
	 searching: World.New
	  maybe: revghost2.Runner.HierarchyTest.World (World.New)
	   candidate found... containing=revghost2.Runner.HierarchyTest
	>> World.New -> (, revghost2.SourceGenerator.FieldInfo)
	.World == Entity
	.World == Id
	.World == SystemDeltaTime
	.World == DeltaTime
	.World == World
	Component Access .World.-2

using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using flecs_hub;
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
            EcsSourceGenerator.SetupSystemManaged(world.Handle, entity, Filter, state, typeof(__THIS__), &EachUnmanaged);
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
Elapsed (scriptSub) 9,1397ms
*/