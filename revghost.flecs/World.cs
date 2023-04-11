using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static flecs_hub.flecs;

namespace revghost.flecs;

public unsafe struct World : IDisposable
{
    public bool Equals(World other)
    {
        return Handle == other.Handle;
    }

    public override bool Equals(object? obj)
    {
        return obj is World other && Equals(other);
    }

    public override int GetHashCode()
    {
        return unchecked((int) (long) Handle);
    }

    public static bool operator ==(World left, World right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(World left, World right)
    {
        return !left.Equals(right);
    }

    public readonly ecs_world_t* Handle;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public World(ecs_world_t* existing)
    {
        Handle = existing;
        ecs_set_entity_range(Handle, new ecs_entity_t {Data = StaticEntity.MaxCount}, default);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static World FromExistingUnsafe(ecs_world_t* existing)
    {
        var ptr = (IntPtr) existing;
        return Unsafe.As<IntPtr, World>(ref ptr);
    }

    private void Register(EntityId id, EntityId parent, string name)
    {
        if (ecs_is_deferred(Handle))
            throw new InvalidOperationException("Cannot register a static entity while the world is in deferred mode");
        
        if (parent.Handle.Data.Data != 0)
        {
            var data = StaticEntity.GetData(parent);
            Register(data.Id, data.Parent, data.Name);
        }

        using var nativeName = new NativeString(name);

        var didExists = ecs_exists(Handle, id.Handle);
        ecs_ensure(Handle, id.Handle);
        ecs_set_name(Handle, id.Handle, nativeName);

        if (parent.Handle.Data.Data != 0)
            ecs_add_id(Handle, id.Handle, ecs_pair(EcsChildOf, parent.Handle));

        if (!didExists)
        {
            if (StaticEntity.GetData(id).Setup is { } setup)
            {
                var prev = ecs_set_scope(Handle, id.Handle);
                setup(this);
                ecs_set_scope(Handle, prev);
            }
        }
    }

    public Entity Register(EntityId type)
    {
        var data = StaticEntity.GetData(type);
        Register(type, data.Parent, data.Name);
        return new Entity
        {
            World = this,
            Id = type
        };
    }
    
    public Entity Register<T>()
        where T : IStaticEntity
    {
        Register(StaticEntity<T>.Id, StaticEntity<T>.ParentId, StaticEntity<T>.Name);
        return new Entity
        {
            World = this,
            Id = StaticEntity<T>.Id
        };
    }
    
    // public Entity RegisterSystem<TSystem>(in TSystem system = default)
    //     where TSystem : unmanaged, ISystem<TSystem>
    // {
    //     return TSystem.Build(ref Unsafe.AsRef(in system), this);
    // }

    public Entity New(EntityCreation info = default)
    {
        ecs_entity_desc_t desc = default;

        if (info.Add.Length > desc.add.Length)
            throw new InvalidOperationException("'info.Add' too big");

        MemoryMarshal.Cast<Identifier, ecs_id_t>(info.Add).CopyTo(desc.add);

        var nativeName = info.Name.Length > 0
            ? new NativeString(info.Name)
            : new NativeString(info.NameUtf8);

        desc.name = nativeName;
        
        Entity result;
        result.World = this;
        result.Id.Handle = ecs_entity_init(Handle, &desc);
        
        // Dispose our native stuff
        nativeName.Dispose();

        return result;
    }

    public ref struct EntityCreation
    {
        public ReadOnlySpan<char> Name;
        public ReadOnlySpan<byte> NameUtf8; // prefer using utf8 to not have conversion from utf16 to utf8
        public ReadOnlySpan<Identifier> Add;

        // kinda risky no?
        public static implicit operator EntityCreation(ReadOnlySpan<byte> nameUtf8) => new()
        {
            NameUtf8 = nameUtf8
        };
        
        public static implicit operator EntityCreation(ReadOnlySpan<char> name) => new()
        {
            Name = name
        };
        
        public static implicit operator EntityCreation(string name) => new()
        {
            Name = name
        };
        
        public static implicit operator EntityCreation(ReadOnlySpan<Identifier> add) => new()
        {
            Add = add
        };
    }

    public bool Progress(float deltaTime)
    {
        return ecs_progress(Handle, deltaTime);
    }

    public void Dispose()
    {
        _ = ecs_fini(Handle);
    }
}