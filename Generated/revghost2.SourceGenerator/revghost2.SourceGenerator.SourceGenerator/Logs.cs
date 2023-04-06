﻿/*
Generate DSLs

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
            public Span<__THIS__>.Enumerator GetEnumerator() => throw new global::System.Diagnostics.UnreachableException("This code should have been replaced");
        }
    }
}
	replace foreach
	Work on Id (+Parent Id <= it.Id)
	.Id == Entity
	.Id == Id
	 searching: it.Id
	  maybe: it (it.Id)
	   candidate found... containing=revghost2.Tests.NestedQueryParentTest.GlobalSystem
	>> it.Id -> (_query, revghost2.SourceGenerator.FieldInfo)
	_query.Id == Entity
	_query.Id == Id
	Component Access _query.Id.-2
	Span: [723..736)
	.maybeParentPos == Entity
	.maybeParentPos == Id
	.maybeParentPos == SystemDeltaTime
	.maybeParentPos == DeltaTime
	.maybeParentPos == World
	.maybeParentPos == RealWorld
	.maybeParentPos == maybeParentPos
	Span: [761..776)
	 searching: it.maybeParentPos
	  maybe: it (it.maybeParentPos)
	   candidate found... containing=revghost2.Tests.NestedQueryParentTest.GlobalSystem
	_query.maybeParentPos == Entity
	_query.maybeParentPos == Id
	_query.maybeParentPos == maybeParentPos
	 searching: Vector2.Distance
	  maybe: System.Numerics.Vector2 (Vector2.Distance)
	   candidate found... containing=
	Work on Vector2 (+Parent Vector2.Distance)
	Work on Distance (+Parent Vector2.Distance)
	 searching: parentPos.Value
	  maybe: parentPos (parentPos.Value)
	   candidate found... containing=revghost2.Tests.NestedQueryParentTest.GlobalSystem
	Parent pattern span: [723..736) True
	>> parentPos.Value -> (, revghost2.SourceGenerator.FieldInfo)
	.maybeParentPos == Entity
	.maybeParentPos == Id
	.maybeParentPos == SystemDeltaTime
	.maybeParentPos == DeltaTime
	.maybeParentPos == World
	.maybeParentPos == RealWorld
	.maybeParentPos == maybeParentPos
	Component Access .maybeParentPos.0
	replace with c1[i0].Value
	 searching: itParentPos.Value
	  maybe: itParentPos (itParentPos.Value)
	   candidate found... containing=revghost2.Tests.NestedQueryParentTest.GlobalSystem
	Parent pattern span: [761..776) True
	>> itParentPos.Value -> (_query, revghost2.SourceGenerator.FieldInfo)
	_query.maybeParentPos == Entity
	_query.maybeParentPos == Id
	_query.maybeParentPos == maybeParentPos
	Component Access _query.maybeParentPos.0
	replace with c_query1[i2].Value
	 searching: Console.WriteLine
	  maybe: System.Console (Console.WriteLine)
	   candidate found... containing=
	Work on Console (+Parent Console.WriteLine)
	Work on WriteLine (+Parent Console.WriteLine)
	Work on Id (+Parent {Id})
	.Id == Entity
	.Id == Id
	 searching: it.Id
	  maybe: it (it.Id)
	   candidate found... containing=revghost2.Tests.NestedQueryParentTest.GlobalSystem
	>> it.Id -> (_query, revghost2.SourceGenerator.FieldInfo)
	_query.Id == Entity
	_query.Id == Id
	Component Access _query.Id.-2

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
            EcsSourceGenerator.SetupSystemManaged(world.Handle, entity, Filter, state, typeof(__THIS__), &EachUnmanaged);
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
	 searching: Console.WriteLine
	  maybe: System.Console (Console.WriteLine)
	   candidate found... containing=
	Work on Console (+Parent Console.WriteLine)
	Work on WriteLine (+Parent Console.WriteLine)
	Work on Id (+Parent {Id})
	.Id == Entity
	.Id == Id
	 searching: foo.Value
	  maybe: revghost2.Tests.ObserverTest.ObserveFooAdd.foo (foo.Value)
	   candidate found... containing=revghost2.Tests.ObserverTest.ObserveFooAdd
	   predicted = revghost2.Tests.ObserverTest.ObserveFooAdd.foo
	>> foo.Value -> (, revghost2.SourceGenerator.FieldInfo)
	.foo == Entity
	.foo == Id
	.foo == SystemDeltaTime
	.foo == DeltaTime
	.foo == World
	.foo == RealWorld
	.foo == foo
	Component Access .foo.0
	replace with c1[i0].Value

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
            EcsSourceGenerator.SetupObserverManaged(world.Handle, entity, Filter, state, typeof(__THIS__), &EachUnmanaged);
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
Attribute WriteAttribute found
	gen: revghost2.Tests.Tests.EntryModule.Position (Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.NonErrorNamedTypeSymbol)
Finish term? True, Name: WriteAttribute
Finished current term
	 searching: Console.WriteLine
	  maybe: System.Console (Console.WriteLine)
	   candidate found... containing=
	Work on Console (+Parent Console.WriteLine)
	Work on WriteLine (+Parent Console.WriteLine)
	 searching: typeof(T0).Name
	  maybe:  (typeof(T0).Name)
	Work on T0 (+Parent typeof(T0))
	.T0 == Entity
	.T0 == Id
	.T0 == SystemDeltaTime
	.T0 == DeltaTime
	.T0 == World
	.T0 == RealWorld
	.T0 == Vector
	Work on Name (+Parent typeof(T0).Name)
	Work on Id (+Parent {Id})
	.Id == Entity
	.Id == Id
	Work on Vector (+Parent {Vector})
	.Vector == Entity
	.Vector == Id
	.Vector == SystemDeltaTime
	.Vector == DeltaTime
	.Vector == World
	.Vector == RealWorld
	.Vector == Vector
	 searching: Entity.Set
	  maybe: revghost2.Entity (Entity.Set)
	   candidate found... containing=
	>> Entity.Set -> (, revghost2.SourceGenerator.FieldInfo)
	.Entity == Entity
	Component Access .Entity.-2
	Work on Position (+Parent new Position { Value = new Vector3(4) })
	.Position == Entity
	.Position == Id
	.Position == SystemDeltaTime
	.Position == DeltaTime
	.Position == World
	.Position == RealWorld
	.Position == Vector
	Work on Value (+Parent Value = new Vector3(4))
	.Value == Entity
	.Value == Id
	.Value == SystemDeltaTime
	.Value == DeltaTime
	.Value == World
	.Value == RealWorld
	.Value == Vector
	Work on Vector3 (+Parent new Vector3(4))
	.Vector3 == Entity
	.Vector3 == Id
	.Vector3 == SystemDeltaTime
	.Vector3 == DeltaTime
	.Vector3 == World
	.Vector3 == RealWorld
	.Vector3 == Vector

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
                    id = (revghost2.StaticEntity<global::revghost2.Tests.Tests.EntryModule.Position>.Id),
                    inout = __FLECS__.ecs_inout_kind_t.EcsOut,
                    src = new()
                    {
                        id = default,
                        flags = __FLECS__.EcsIsEntity,
                    },
                },
                new()
                {
                    id = (revghost2.StaticEntity<T0>.Id),
                    inout = __FLECS__.ecs_inout_kind_t.EcsInOut,
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
                EcsSourceGenerator.SetupSystemManaged(world.Handle, entity, Filter, state, typeof(__THIS__), (delegate*unmanaged<__FLECS__.ecs_iter_t*, void>) Marshal.GetFunctionPointerForDelegate(EachUnmanaged));
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

using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace revghost2.Tests;


unsafe partial class NestedQueryTest
{
    unsafe partial struct GlobalSystem
    {
        unsafe partial struct NestedQuery
        {
            public static ReadOnlySpan<__FLECS__.ecs_term_t> Terms => new __FLECS__.ecs_term_t[]
            {
                new()
                {
                    id = (revghost2.StaticEntity<global::revghost2.Tests.NestedQueryTest.Position>.Id),
                    inout = __FLECS__.ecs_inout_kind_t.EcsIn,
                    src = new()
                    {
                    },
                },
                new()
                {
                    id = (revghost2.StaticEntity<global::revghost2.Tests.NestedQueryTest.HitSize>.Id),
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
            ///<summary>
            /// Iterate entities from a filter
            ///</summary>
            ///<remarks>
            ///This should only be called in a System or a Processor
            ///</remarks>
            public Span<__THIS__>.Enumerator GetEnumerator() => throw new global::System.Diagnostics.UnreachableException("This code should have been replaced");
        }
    }
}
	 searching: Console.WriteLine
	  maybe: System.Console (Console.WriteLine)
	   candidate found... containing=
	Work on Console (+Parent Console.WriteLine)
	Work on WriteLine (+Parent Console.WriteLine)
	Work on Id (+Parent {Id})
	.Id == Entity
	.Id == Id
	replace foreach
	Work on Id (+Parent Id <= it.Id)
	.Id == Entity
	.Id == Id
	 searching: it.Id
	  maybe: it (it.Id)
	   candidate found... containing=revghost2.Tests.NestedQueryTest.GlobalSystem
	>> it.Id -> (_query, revghost2.SourceGenerator.FieldInfo)
	_query.Id == Entity
	_query.Id == Id
	Component Access _query.Id.-2
	 searching: Vector2.Distance
	  maybe: System.Numerics.Vector2 (Vector2.Distance)
	   candidate found... containing=
	Work on Vector2 (+Parent Vector2.Distance)
	Work on Distance (+Parent Vector2.Distance)
	 searching: pos.Value
	  maybe: revghost2.Tests.NestedQueryTest.GlobalSystem.pos (pos.Value)
	   candidate found... containing=revghost2.Tests.NestedQueryTest.GlobalSystem
	   predicted = revghost2.Tests.NestedQueryTest.GlobalSystem.pos
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
	 searching: it.pos.Value
	  maybe: revghost2.Tests.NestedQueryTest.GlobalSystem.NestedQuery.pos (it.pos.Value)
	   candidate found... containing=revghost2.Tests.NestedQueryTest.GlobalSystem.NestedQuery
	   break now! found a good candidate _query <=> pos
	>> it.pos.Value -> (_query, revghost2.SourceGenerator.FieldInfo)
	_query.pos == Entity
	_query.pos == Id
	_query.pos == pos
	Component Access _query.pos.0
	replace with c_query1[i2].Value
	 searching: size.Value
	  maybe: revghost2.Tests.NestedQueryTest.GlobalSystem.size (size.Value)
	   candidate found... containing=revghost2.Tests.NestedQueryTest.GlobalSystem
	   predicted = revghost2.Tests.NestedQueryTest.GlobalSystem.size
	>> size.Value -> (, revghost2.SourceGenerator.FieldInfo)
	.size == Entity
	.size == Id
	.size == SystemDeltaTime
	.size == DeltaTime
	.size == World
	.size == RealWorld
	.size == pos
	.size == size
	Component Access .size.1
	replace with c2[i0].Value
	 searching: it.size.Value
	  maybe: revghost2.Tests.NestedQueryTest.GlobalSystem.NestedQuery.size (it.size.Value)
	   candidate found... containing=revghost2.Tests.NestedQueryTest.GlobalSystem.NestedQuery
	   break now! found a good candidate _query <=> size
	>> it.size.Value -> (_query, revghost2.SourceGenerator.FieldInfo)
	_query.size == Entity
	_query.size == Id
	_query.size == pos
	_query.size == size
	Component Access _query.size.1
	replace with c_query2[i2].Value
	 searching: Console.WriteLine
	  maybe: System.Console (Console.WriteLine)
	   candidate found... containing=
	Work on Console (+Parent Console.WriteLine)
	Work on WriteLine (+Parent Console.WriteLine)
	Work on Id (+Parent {Id})
	.Id == Entity
	.Id == Id
	 searching: it.Id
	  maybe: it (it.Id)
	   candidate found... containing=revghost2.Tests.NestedQueryTest.GlobalSystem
	>> it.Id -> (_query, revghost2.SourceGenerator.FieldInfo)
	_query.Id == Entity
	_query.Id == Id
	Component Access _query.Id.-2

using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace revghost2.Tests;


unsafe partial class NestedQueryTest
{
    unsafe partial struct GlobalSystem
    {
        public static ReadOnlySpan<__FLECS__.ecs_term_t> Terms => new __FLECS__.ecs_term_t[]
        {
            new()
            {
                id = (revghost2.StaticEntity<global::revghost2.Tests.NestedQueryTest.Position>.Id),
                inout = __FLECS__.ecs_inout_kind_t.EcsIn,
                src = new()
                {
                },
            },
            new()
            {
                id = (revghost2.StaticEntity<global::revghost2.Tests.NestedQueryTest.HitSize>.Id),
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
            public __FLECS__.ecs_query_t* _query;
        }
        public static unsafe void Setup(revghost2.World world)
        {
            var entity = __FLECS__.ecs_get_scope(world.Handle);
            var state = (__SYSTEM_STATE__*) NativeMemory.AllocZeroed((nuint) Unsafe.SizeOf<__SYSTEM_STATE__>());
            {
                var queryDesc = new __FLECS__.ecs_query_desc_t()
                {
                    filter = global::revghost2.Tests.NestedQueryTest.GlobalSystem.NestedQuery.Filter
                };
                state->_query = __FLECS__.ecs_query_init(world.Handle, &queryDesc);
            }
            EcsSourceGenerator.SetupSystemManaged(world.Handle, entity, Filter, state, typeof(__THIS__), &EachUnmanaged);
            _ = nameof(Each); // reference it so the editor and compiler doesn't complain it's unused
        }
        [UnmanagedCallersOnly]
        private static unsafe void EachUnmanaged(__FLECS__.ecs_iter_t* __it__)
        {
            var __state__ = (__SYSTEM_STATE__*) __it__->ctx;
            var entities = __it__->entities;
            var c1 = (global::revghost2.Tests.NestedQueryTest.Position*) __FLECS__.ecs_field_w_size(__it__, (ulong) Unsafe.SizeOf<global::revghost2.Tests.NestedQueryTest.Position>(), 1);
            var c2 = (global::revghost2.Tests.NestedQueryTest.HitSize*) __FLECS__.ecs_field_w_size(__it__, (ulong) Unsafe.SizeOf<global::revghost2.Tests.NestedQueryTest.HitSize>(), 2);
            for (var i0 = 0; i0 < __it__->count; i0++)
            {
                {
            Console.WriteLine($"{Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref entities[i0])} will start colliding...");
var __it___query_noptr = __FLECS__.ecs_query_iter(__it__->world, __state__->_query);
var __it___query = (__FLECS__.ecs_iter_t*) Unsafe.AsPointer(ref __it___query_noptr);
while (__FLECS__.ecs_query_next(__it___query)) {
    
    var c_query1 = (global::revghost2.Tests.NestedQueryTest.Position*) __FLECS__.ecs_field_w_size(__it___query, (ulong) Unsafe.SizeOf<global::revghost2.Tests.NestedQueryTest.Position>(), 1);
    var c_query2 = (global::revghost2.Tests.NestedQueryTest.HitSize*) __FLECS__.ecs_field_w_size(__it___query, (ulong) Unsafe.SizeOf<global::revghost2.Tests.NestedQueryTest.HitSize>(), 2);
    
    for (var i2 = 0; i2 < __it___query->count; i2++) {
        {
                if (Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref entities[i0])<= Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref __it___query->entities[i2]))
                    continue;

                if (Vector2.Distance(c1[i0].Value, c_query1[i2].Value) > c2[i0] .Value + c_query2[i2].Value)
                    continue;

                Console.WriteLine($"  {Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref entities[i0])} and {Unsafe.As<__FLECS__.ecs_entity_t, EntityId>(ref __it___query->entities[i2])} collided!");
            }
    }
}
        }
            }
        }
    }
}
Elapsed (scriptSub) 71,9139ms
*/