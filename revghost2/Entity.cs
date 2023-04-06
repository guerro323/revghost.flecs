using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static flecs_hub.flecs;
using JetBrains.Annotations;

namespace revghost2;

[PublicAPI]
public unsafe struct Entity : IDisposable
{
    public World World;
    public EntityId Id;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Pair With(Entity other)
    {
        return Pair.From(this, other);
    }

    public readonly NativeStringView Name => new(ecs_get_name(World.Handle, Id));

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
    public ref Entity Add<T>()
        where T : IComponent
    {
        ecs_add_id(World.Handle, Id, StaticEntity<T>.Id);
        return ref this;
    }
    
    [UnscopedRef]
    public ref Entity Add<TLeft, TRight>()
        where TLeft : IComponent
        where TRight : IComponent
    {
        ecs_add_id(World.Handle, Id, PairId.From(StaticEntity<TLeft>.Id, StaticEntity<TRight>.Id).Handle);
        return ref this;
    }
    
    [UnscopedRef]
    public ref Entity Set<T>(in T data)
        where T : IComponent
    {
        ecs_set_id(
            World.Handle,
            Id, StaticEntity<T>.Id,
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
            Id, ecs_pair(StaticEntity<TLeft>.Id, StaticEntity<TRight>.Id),
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
            Id, ecs_pair(StaticEntity<TLeft>.Id, StaticEntity<TRight>.Id),
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
            Id, ecs_pair(StaticEntity<TLeft>.Id, StaticEntity<TRight>.Id)
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
            Id, ecs_pair(StaticEntity<TRight>.Id, StaticEntity<TLeft>.Id)
        );
        return ref Unsafe.AsRef<TRight>(data);
    }
    
    [UnscopedRef]
    public ref T Get<T>()
        where T : IComponent
    {
        var data = ecs_get_id(
            World.Handle,
            Id, StaticEntity<T>.Id
        );
        return ref Unsafe.AsRef<T>(data);
    }
    
    [UnscopedRef]
    public ref T Has<T>()
        where T : IComponent
    {
        var data = ecs_get_id(
            World.Handle,
            Id, StaticEntity<T>.Id
        );
        return ref Unsafe.AsRef<T>(data);
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
        return Handle.Equals(other.Handle);
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