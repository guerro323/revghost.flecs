using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static flecs_hub.flecs;
using JetBrains.Annotations;

namespace revghost.flecs;

[PublicAPI]
public unsafe struct Entity : IDisposable
{
    public World World;
    public EntityId Id;

    public bool Exists => Id != default && ecs_exists(World.Handle, Id);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Pair With(Entity other)
    {
        return Pair.From(this, other);
    }

    public readonly NativeStringView Name => new(ecs_get_name(World.Handle, Id));

    public Entity this[ReadOnlySpan<char> path] => World.Lookup(this, path);
    public Entity this[ReadOnlySpan<byte> path] => World.Lookup(this, path);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator EntityId(Entity entity)
    {
        return entity.Id;
    }

    [Conditional("DEBUG")]
    private void ThrowOnWorldMismatch(World other)
    {
        if (World != other)
            throw new InvalidOperationException("not the same world");
    }
    
    /// <summary>
    /// Ensure that this entity exists
    /// </summary>
    [UnscopedRef]
    public ref Entity Ensure()
    {
        ecs_ensure(World.Handle, Id);
        return ref this;
    }
    
    [UnscopedRef]
    public ref Entity Add(Pair pair)
    {
        ThrowOnWorldMismatch(pair.World);
        
        ecs_add_id(World.Handle, Id, pair.Id.Handle);
        return ref this;
    }

    [UnscopedRef]
    public ref Entity Add(Entity entity)
    {
        ThrowOnWorldMismatch(entity.World);
        
        ecs_add_id(World.Handle, Id, entity.Id);
        return ref this;
    }
    
    [UnscopedRef]
    public ref Entity Add(Identifier identifier)
    {
        ecs_add_id(World.Handle, Id, identifier.Handle);
        return ref this;
    }
    
    [UnscopedRef]
    public ref Entity Add<T>()
        where T : IComponent
    {
        ecs_add_id(World.Handle, Id, World.Get<T>().Id);
        return ref this;
    }
    
    [UnscopedRef]
    public ref Entity Add<TLeft, TRight>()
        where TLeft : IComponent
        where TRight : IComponent
    {
        ecs_add_id(World.Handle, Id, PairId.From(World.Get<TLeft>().Id, World.Get<TRight>().Id).Handle);
        return ref this;
    }

    [UnscopedRef]
    public ref Entity Set<T>(in T data)
        where T : IComponent
    {
        ecs_set_id(
            World.Handle,
            Id, World.Get<T>().Id,
            (ulong) Unsafe.SizeOf<T>(), Unsafe.AsPointer(ref Unsafe.AsRef(in data))
        );

        return ref this;
    }
    
    [UnscopedRef]
    public ref Entity Set<T>(T data)
        where T : IComponent
    {
#pragma warning disable CS9087
        return ref Set(in data);
#pragma warning restore CS9087
    }

    [UnscopedRef]
    public ref Entity Set<TLeft, TRight>(in TRight data)
        where TLeft : IComponent
        where TRight : IComponent
    {
        ecs_set_id(
            World.Handle,
            Id, ecs_pair(World.Get<TLeft>().Id, World.Get<TRight>().Id),
            (ulong) Unsafe.SizeOf<TRight>(), Unsafe.AsPointer(ref Unsafe.AsRef(in data))
        );
        return ref this;
    }

    [UnscopedRef]
    public ref Entity Set<TLeft, TRight>(TRight data)
        where TLeft : IComponent
        where TRight : IComponent
    {
#pragma warning disable CS9087
        return ref Set<TLeft, TRight>(in data);
#pragma warning restore CS9087
    }
    
    [UnscopedRef]
    public ref Entity Set<TLeft, TRight>(in TLeft data)
        where TLeft : IComponent
        where TRight : IComponent
    {
        ecs_set_id(
            World.Handle,
            Id, ecs_pair(World.Get<TLeft>().Id, World.Get<TRight>().Id),
            (ulong) Unsafe.SizeOf<TRight>(), Unsafe.AsPointer(ref Unsafe.AsRef(in data))
        );
        return ref this;
    }

    [UnscopedRef]
    public ref Entity Set<TLeft, TRight>(TLeft data)
        where TLeft : IComponent
        where TRight : IComponent
    {
#pragma warning disable CS9087
        return ref Set<TLeft, TRight>(in data);
#pragma warning restore CS9087
    }
    
    [UnscopedRef]
    public ref TLeft GetFirst<TLeft, TRight>()
        where TLeft : IComponent
        where TRight : IComponent
    {
        var data = ecs_get_id(
            World.Handle,
            Id, ecs_pair(World.Get<TLeft>().Id, World.Get<TRight>().Id)
        );
        return ref Unsafe.AsRef<TLeft>(data);
    }
    
    [UnscopedRef]
    public ref TLeft GetFirst<TLeft>(Identifier right)
        where TLeft : IComponent
    {
        var data = ecs_get_id(
            World.Handle,
            Id, ecs_pair(World.Get<TLeft>().Id, right.Handle)
        );
        return ref Unsafe.AsRef<TLeft>(data);
    }
    
    [UnscopedRef]
    public ref TRight GetSecond<TLeft, TRight>()
        where TLeft : IComponent
        where TRight : IComponent
    {
        var data = ecs_get_id(
            World.Handle,
            Id, ecs_pair(World.Get<TLeft>().Id, World.Get<TRight>().Id)
        );
        return ref Unsafe.AsRef<TRight>(data);
    }
    
    [UnscopedRef]
    public ref TRight GetSecond<TRight>(Identifier left)
        where TRight : IComponent
    {
        var data = ecs_get_id(
            World.Handle,
            Id, ecs_pair(left.Handle, World.Get<TRight>().Id)
        );
        return ref Unsafe.AsRef<TRight>(data);
    }

    public Entity GetTarget<T>(int index = 0)
        where T : IComponent
    {
        var data = ecs_get_target(
            World.Handle,
            Id, World.Get<T>().Id,
            index
        );
        return this with {Id = data};
    }
    
    [UnscopedRef]
    public ref T Get<T>()
        where T : IComponent
    {
        var data = ecs_get_id(
            World.Handle,
            Id, World.Get<T>().Id
        );
        return ref Unsafe.AsRef<T>(data);
    }
    
    public T GetOrDefault<T>(T whenNotFound = default)
        where T : IComponent
    {
        if (!Has<T>())
            return whenNotFound;
        
        var data = ecs_get_id(
            World.Handle,
            Id, World.Get<T>().Id
        );
        return Unsafe.AsRef<T>(data);
    }
    
    public bool Has<T>()
        where T : IComponent
    {
        return ecs_has_id(
            World.Handle,
            Id, World.Get<T>().Id
        );
    }
    
    public bool Has(Pair pair)
    {
        ThrowOnWorldMismatch(pair.World);
        
        return ecs_has_id(
            World.Handle,
            Id, pair.Id.Handle
        );
    }
    
    public bool Has(PairId pair)
    {
        return ecs_has_id(
            World.Handle,
            Id, pair.Handle
        );
    }
    
    public bool Has(Identifier id)
    {
        return ecs_has_id(
            World.Handle,
            Id, id.Handle
        );
    }
    
    [UnscopedRef]
    public ref Entity Remove(Pair pair)
    {
        ThrowOnWorldMismatch(pair.World);
        
        ecs_remove_id(World.Handle, Id, pair.Id.Handle);
        return ref this;
    }

    [UnscopedRef]
    public ref Entity Remove(Entity entity)
    {
        ThrowOnWorldMismatch(entity.World);
        
        ecs_remove_id(World.Handle, Id, entity.Id);
        return ref this;
    }
    
    [UnscopedRef]
    public ref Entity Remove<T>()
        where T : IComponent
    {
        ecs_remove_id(World.Handle, Id, World.Get<T>().Id);
        return ref this;
    }
    
    [UnscopedRef]
    public ref Entity Remove<TLeft, TRight>()
        where TLeft : IComponent
        where TRight : IComponent
    {
        ecs_remove_id(World.Handle, Id, PairId.From(World.Get<TLeft>().Id, World.Get<TRight>().Id).Handle);
        return ref this;
    }

    public void Dispose()
    {
        ecs_delete(World.Handle, Id);
    }

    public override string ToString()
    {
        var nstring = new NativeStringView(ecs_entity_str(World.Handle, Id));
        
        return $"({Id.Handle.Data.Data}) {nstring.ToString()}";
    }
}

public struct EntityId : IEquatable<EntityId>
{
    public bool Equals(EntityId other)
    {
        return Handle.Data.Data.Equals(other.Handle.Data.Data);
    }

    public override bool Equals(object? obj)
    {
        return obj is EntityId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Handle.GetHashCode();
    }

    public static bool operator ==(EntityId left, EntityId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(EntityId left, EntityId right)
    {
        return !left.Equals(right);
    }
    
    public static bool operator >(EntityId left, EntityId right)
    {
        return left.Handle.Data < right.Handle.Data;
    }
    
    public static bool operator <(EntityId left, EntityId right)
    {
        return left.Handle.Data > right.Handle.Data;
    }
    
    public static bool operator >=(EntityId left, EntityId right)
    {
        return left.Handle.Data >= right.Handle.Data;
    }
    
    public static bool operator <=(EntityId left, EntityId right)
    {
        return left.Handle.Data <= right.Handle.Data;
    }

    public static readonly EntityId ChildOf = new() {Handle = EcsChildOf};
    
    public ecs_entity_t Handle;

    public bool Is<TStatic>()
        where TStatic : IStaticEntity
    {
        return this == StaticEntity<TStatic>.Id;
    }
    
    public bool IsNot<TStatic>()
        where TStatic : IStaticEntity
    {
        return this != StaticEntity<TStatic>.Id;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly PairId With(EntityId other)
    {
        return PairId.From(this, other);
    }

    public readonly Entity WithWorld(World world)
    {
        Entity ret;
        ret.World = world;
        ret.Id = this;
        return ret;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ecs_entity_t(EntityId id) => id.Handle;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator EntityId(ecs_entity_t id)
    {
        EntityId ret;
        ret.Handle = id;
        return ret;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator EntityId(PairId pair)
    {
        EntityId ret;
        ret.Handle = pair.Handle;
        return ret;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ecs_id_t(EntityId id) => id.Handle.Data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator EntityId(ecs_id_t id)
    {
        EntityId ret;
        ret.Handle.Data = id;
        return ret;
    }

    public override string ToString()
    {
        return $"EID({Handle.Data.Data})";
    }
}