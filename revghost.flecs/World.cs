using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
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
    
    public Entity CreateStatic(EntityId id, EntityId parent, string name)
    {
        if (Exists(id))
            throw new InvalidOperationException($"The static entity '{name}' ({id}) already exists.");

        if (ecs_stage_is_readonly(Handle))
            throw new InvalidOperationException($"Couldn't register '{StaticEntity.GetData(id).FullPath}', world is read-only");

        if (ecs_is_deferred(Handle))
            throw new InvalidOperationException($"Couldn't register '{StaticEntity.GetData(id).FullPath}', world is in a deferred state");

        if (parent != default && !Exists(parent))
        {
            var data = StaticEntity.GetData(parent);
            GetStatic(data.Id);
        }
        
        using var nativeName = new NativeString(name);

        ecs_ensure(Handle, id.Handle);
        ecs_set_name(Handle, id.Handle, nativeName);

        if (parent.Handle.Data.Data != 0)
            ecs_add_id(Handle, id.Handle, ecs_pair(EcsChildOf, parent.Handle));
        
        ecs_add_id(Handle, id, EcsModule);
        
        if (StaticEntity.GetData(id).Setup is { } setup)
        {
            var prev = ecs_set_scope(Handle, id.Handle);
            setup(this);
            ecs_set_scope(Handle, prev);
        }

        return new Entity
        {
            Id = id,
            World = this
        };
    }

    public bool Exists(EntityId id)
    {
        return id != default && ecs_exists(Handle, id);
    }

    public bool Exists<T>()
        where T : IStaticEntity
    {
        return Exists(StaticEntity<T>.Id);
    }

    public Entity Register(EntityId type)
    {
        if (Exists(type))
            return new Entity {Id = type, World = this};
    
        var data = StaticEntity.GetData(type);
        CreateStatic(type, data.Parent, data.Name);
        return new Entity
        {
            World = this,
            Id = type
        };
    }
    
    public Entity Register<T>()
        where T : IStaticEntity
    {
        return Register(StaticEntity<T>.Id);
    }

    /// <summary>
    /// Get (or create if it can be automatically registered) a static entity.
    /// </summary>
    /// <returns>The static entity in the world.</returns>
    /// <exception cref="KeyNotFoundException">The entity was not found and couldn't be registered.</exception>
    public Entity GetStatic(EntityId id)
    {
        if (!Exists(id))
        {
            var data = StaticEntity.GetData(id);
            if (data.CanBeAutoRegistered)
            {
                Console.WriteLine("Auto register " + data.FullPath);
                return Register(id);
            }
            
            throw new KeyNotFoundException($"Static Entity {data.Name} ({id}) not found.");
        }
        
        return new Entity
        {
            Id = id,
            World = this
        };
    }

    /// <summary>
    /// Get (or create if it can be automatically registered) a static entity.
    /// </summary>
    /// <returns>The static entity in the world.</returns>
    /// <exception cref="KeyNotFoundException">The entity was not found and couldn't be registered.</exception>
    public Entity Get<T>()
        where T : IStaticEntity
    {
        return GetStatic(StaticEntity<T>.Id);
    }
    
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
        // desc.sep = sep;
        // desc.root_sep = sep;
        
        Entity result;
        result.World = this;
        result.Id.Handle = ecs_entity_init(Handle, &desc);
        
        // Dispose our native stuff
        nativeName.Dispose();

        return result;
    }

    private static ReadOnlySpan<byte> NullBytes => "\0"u8;
    
    private EntityId LookupFor(ReadOnlySpan<byte> view, EntityId parent)
    {
        Span<byte> cpy = stackalloc byte[view.Length + 1];
        view.CopyTo(cpy);
        cpy[^1] = 0;

        view = cpy;

        if (parent == default)
            return ecs_lookup(Handle, new NativeString(view));
        
        return ecs_lookup_child(Handle, parent, new NativeString(view));
    }
    
    private Entity LookupRecursive(NativeStringView nativeString, EntityId parent, bool recursive)
    {
        Entity ret;
        ret.World = this;

        //var view = new NativeStringView((IntPtr) Unsafe.AsPointer(ref nativeString.FirstChar));
        EntityId curr = parent;
        if (curr == default)
        {
            curr = Scope.GetValueOrDefault().Id;
            parent = curr;
        }

        var originalScope = Scope.GetValueOrDefault();
        using (BeginScope(default))
        {
            while (true)
            {
                var chars = nativeString.CharsUtf8;
                while (chars.Length > 0)
                {
                    var end = chars.IndexOf("."u8[0]);
                    if (end < 0)
                        end = chars.Length;

                    // Console.WriteLine($"lookup in {curr} ({chars.Length})");
                    curr = LookupFor(chars[..(end)], curr);
                    if (curr == default)
                        break;

                    if (end >= chars.Length)
                        break;
                    
                    chars = chars[(end + 1)..];
                }

                if (curr != default)
                    break;
                
                if (parent == default || !recursive)
                    break;

                if (parent == default)
                {
                    parent = originalScope;
                    originalScope = default;
                }

                parent = ecs_get_parent(Handle, parent);
                curr = parent;
            }
        }

        ret.Id = curr;

        return ret;
    }
    
    public Entity Lookup(ReadOnlySpan<char> path, bool recursive = true)
    {
        var nstring = new NativeString(path);
        var ret = LookupRecursive(nstring, default, recursive);
        
        nstring.Dispose(); // don't use 'using' so that we don't have performance penalty with 'try finally'

        return ret;
    }
    
    public Entity Lookup(ReadOnlySpan<byte> path, bool recursive = true)
    {
        var nstring = new NativeString(path);
        var ret = LookupRecursive(nstring, default, recursive);

        if (ret.Id == default)
        {
            foreach (var p in path)
            {
                Console.WriteLine($"-> {(char) p}");
            }
        }

        nstring.Dispose(); // don't use 'using' so that we don't have performance penalty with 'try finally'

        return ret;
    }
    
    public Entity Lookup(Entity parent, ReadOnlySpan<char> path, bool recursive = true)
    {
        // Console.WriteLine($"Lookup {path} in {parent}");
        
        var nstring = new NativeString(path);
        var ret = LookupRecursive(nstring, parent, recursive);
        
        nstring.Dispose(); // don't use 'using' so that we don't have performance penalty with 'try finally'

        return ret;
    }
    
    public Entity Lookup(Entity parent, ReadOnlySpan<byte> path, bool recursive = true)
    {
        var nstring = new NativeString(path);
        var ret = LookupRecursive(nstring, parent, recursive);

        nstring.Dispose(); // don't use 'using' so that we don't have performance penalty with 'try finally'

        return ret;
    }
    
    public Entity this[ReadOnlySpan<char> path] => Lookup(path, true);
    public Entity this[ReadOnlySpan<byte> path] => Lookup(path, true);

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

    public Entity? Scope
    {
        get
        {
            var id = ecs_get_scope(Handle);
            if (id.Data.Data == 0)
                return null;

            return new Entity
            {
                World = this,
                Id = id
            };
        }
        set => ecs_set_scope(Handle, value?.Id ?? default);
    }

    public ScopeDisposable BeginScope(Entity entity)
    {
        var previous = Scope;
        Scope = entity;
        
        return new ScopeDisposable {World = this, Previous = previous };
    }

    public struct ScopeDisposable : IDisposable
    {
        public World World;
        public Entity? Previous;
        
        public void Dispose()
        {
            World.Scope = Previous;
        }
    }
    
    public void Emit(Entity @event, Entity target, ReadOnlySpan<EntityId> componentIds)
    {
        var ids = new ecs_type_t
        {
            count = componentIds.Length,
            array = (ecs_id_t*) Unsafe.AsPointer(ref MemoryMarshal.GetReference(componentIds))
        };
        var desc = new ecs_event_desc_t
        {
            @event = @event.Id,
            ids = &ids,
            entity = target.Id
        };
        
        ecs_emit(Handle, &desc);
    }

    public void Emit<T>(Entity target, ReadOnlySpan<EntityId> componentIds)
        where T : IObserveAction
    {
        Emit(Get<T>(), target, componentIds);
    }

    public void SendQuitRequest()
    {
        ecs_quit(Handle);
    }
}