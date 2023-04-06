using System.Reflection;
using System.Runtime.CompilerServices;
using flecs_hub;

namespace revghost2;

public interface IStaticEntity
{
}

public interface IStaticEntityReserveId
{
    static abstract EntityId? ReservedId();
}

public class AnyEntity<T> : IStaticEntity, IStaticEntitySetup, IStaticEntityCustomName, IStaticEntityReserveId, IStaticEntityIsType
{
    public static EntityId? ReservedId()
    {
        if (typeof(T) == typeof(float))
        {
            return flecs.pinvoke_ecs_f32_t();
        }

        if (typeof(T) == typeof(int))
        {
            return flecs.pinvoke_ecs_i32_t();
        }

        return null;
    }

    public static StaticEntityTypeData.Field[] Members()
    {
        if (ReservedId() != null)
            return null!;
        
        var fields = typeof(T).GetFields();
        return fields.Select(f =>
        {
            return new StaticEntityTypeData.Field
            {
                Name = f.Name,
                Type = (EntityId) typeof(StaticEntity<>)
                    .MakeGenericType(typeof(AnyEntity<>).MakeGenericType(f.FieldType))
                    .GetField("Id")!
                    .GetValue(null)!
            };
        }).ToArray();
    }

    public static string Name()
    {
        return typeof(T).Name;
    }

    public static unsafe void Setup(World world)
    {
        if (ReservedId() != null)
            return;
        
        var entity = flecs.ecs_get_scope(world.Handle);
        
        var desc = new flecs.ecs_struct_desc_t();

        desc.entity = entity;
        var i = 0;
        foreach (var field in Members())
        {
            // TODO: it need to be freed later
            var nativeName = new NativeString(field.Name);
            desc.members[i++] = new flecs.ecs_member_t
            {
                name = nativeName,
                type = field.Type
            };
        }
        
        var ent = flecs.ecs_struct_init(world.Handle, &desc);
        Console.WriteLine(ent.Data.Data);
    }
}

public interface IStaticEntityIsType
{
    static abstract StaticEntityTypeData.Field[] Members();
    
    public interface WithSelf
    {
        static abstract StaticEntityTypeData.Field[] Members<TSelf>() where TSelf : IStaticEntity;
    }
}

public interface IStaticEntitySetup
{
    static abstract void Setup(World world);
    
    public interface WithSelf
    {
        static abstract void Setup<TSelf>(World world) where TSelf : IStaticEntity;
    }
}

public interface IStaticEntityParent
{
    static abstract EntityId Parent();
}

public interface IStaticEntityParent<T> : IStaticEntityParent
    where T : IStaticEntity
{
    static EntityId IStaticEntityParent.Parent()
    {
        return StaticEntity<T>.Id;
    }
}

public interface IStaticEntityCustomName
{
    static abstract string Name();
    
    public interface WithSelf
    {
        static abstract string Name<TSelf>();
    }
}

struct StaticEntityData
{
    public string Name;
    public string FullPath;
    public EntityId Id;
    public EntityId Parent;

    public Action<World>? Setup;
}

public struct StaticEntityTypeData
{
    public struct Field
    {
        public string Name;
        public EntityId Type;
    }

    public Field[] Fields;
}

static class StaticEntityInternal
{
    private static ulong Next;
    public const int Start = 1000;
    public const int MaxCount = Start + 1000;

    private static Dictionary<(EntityId parent, string name), EntityId> _idMap = new();
    private static Dictionary<EntityId, StaticEntityData> _data = new();
    
    public static Dictionary<EntityId, StaticEntityTypeData> TypeData = new();

    public static EntityId Register(EntityId parent, string name, Action<World>? setup)
    {
        var key = (parent, name);
        if (_idMap.TryGetValue(key, out var id))
            return id;

        if (Next >= MaxCount)
            throw new IndexOutOfRangeException(nameof(Next));

        id.Handle.Data.Data = Next++;
        _idMap.Add(key, id);
        _data.Add(id, new StaticEntityData
        {
            Name = name,
            FullPath = _data.TryGetValue(parent, out var parentData) ? $"{parentData.FullPath}.{name}" : name,
            Id = id,
            Parent = parent,
            Setup = setup
        });

        return id;
    }
    
    public static EntityId Register(EntityId reserved, EntityId parent, string name, Action<World>? setup)
    {
        var key = (parent, name);
        if (_idMap.TryGetValue(key, out var id))
        {
            if (id.Handle.Data.Data != reserved.Handle.Data.Data)
                throw new InvalidOperationException();
            return id;
        }

        if (Next >= MaxCount)
            throw new IndexOutOfRangeException(nameof(Next));

        id = reserved;
        _idMap.Add(key, id);
        _data.Add(id, new StaticEntityData
        {
            Name = name,
            FullPath = _data.TryGetValue(parent, out var parentData) ? $"{parentData.FullPath}.{name}" : name,
            Id = id,
            Parent = parent,
            Setup = setup
        });

        return id;
    }

    static StaticEntityInternal()
    {
        Next = Start;
    }

    public static StaticEntityData GetData(EntityId id)
    {
        return _data[id];
    }
}

public static unsafe class StaticEntity<T>
    where T : IStaticEntity
{
    // IDs are static, and since serializing/deserializing doesn't have IDs, the ID value here should be safe, except when:
    // - loading a module after the user has created entities
    //
    // To fix the first issue, we can reserve the first range of 1000 entities, so user entities will start at FirstId+1000
    
    public static readonly EntityId ParentId;
    public static readonly EntityId Id;
    public static readonly string Name;
    public static readonly string FullPath;
    public static readonly Action<World>? Setup;

    public static readonly StaticEntityTypeData.Field[]? Members;

    struct Callable
    {
        public object Prefix;
        public MethodInfo Original;

        public object Invoke(params object?[]? parameters)
        {
            if (Prefix != null)
            {
                if (parameters == null)
                {
                    return Original.Invoke(null, new[] {Prefix})!;
                }
                return Original.Invoke(null, new[] {Prefix}.Concat(parameters).ToArray())!;
            }
            
            return Original.Invoke(null, parameters)!;
        }
    }

    static StaticEntity()
    {
        var interfaces = typeof(T).GetInterfaces();

        Callable? GetMethodInfo(Type original)
        {
            if (!interfaces.Contains(original)) 
                return null;
            
            var map = typeof(T).GetInterfaceMap(original);
            return new Callable {Original = map.TargetMethods[0]};
        }

        Callable? GetMethodInfoPair(Type original, Type withSelf)
        {
            if (interfaces.Contains(original))
            {
                return GetMethodInfo(original);
            }

            if (interfaces.Contains(withSelf))
            {
                var ret = GetMethodInfo(withSelf);
                return new Callable
                {
                    Original = ret!.Value.Original.GetGenericMethodDefinition().MakeGenericMethod(typeof(T))
                };
            }
            else
                return null;
        }

        if (GetMethodInfo(typeof(IStaticEntityParent)) is { } parent)
        {
            ParentId = (EntityId) parent.Invoke();
        }
        
        if (GetMethodInfoPair(typeof(IStaticEntityIsType), typeof(IStaticEntityIsType.WithSelf)) is { } isType)
        {
            Members = (StaticEntityTypeData.Field[]) isType.Invoke();
        }

        if (GetMethodInfoPair(typeof(IStaticEntityCustomName), typeof(IStaticEntityCustomName.WithSelf)) is { } customName)
        {
            Name = (string) customName.Invoke();
        }
        else
        {
            Name = typeof(T).Name;
            if (typeof(T).GetGenericArguments() is { } args)
            {
                foreach (var arg in args)
                {
                    Name += $"_{arg.Name}";
                }
            }
        }

        if (GetMethodInfoPair(typeof(IStaticEntitySetup), typeof(IStaticEntitySetup.WithSelf)) is { } setup)
        {
            Setup = world =>
            {
                setup.Invoke(world);
            };
        }

        if (GetMethodInfo(typeof(IStaticEntityReserveId)) is { } reserveId)
        {
            var maybeReserved  = (EntityId?) reserveId.Invoke();
            if (maybeReserved is { } reserved)
                Id = StaticEntityInternal.Register(reserved, ParentId, Name, Setup);
            else
                Id = StaticEntityInternal.Register(ParentId, Name, Setup);
        }
        else
        {
            Id = StaticEntityInternal.Register(ParentId, Name, Setup);
        }

        if (Id.Handle.Data.Data == ParentId.Handle.Data.Data)
            throw new InvalidOperationException();

        if (Members != null)
        {
            StaticEntityInternal.TypeData[Id] = new StaticEntityTypeData
            {
                Fields = Members
            };
        }

        FullPath = StaticEntityInternal.GetData(Id).FullPath;
        
        Console.WriteLine($"Register {Name} with Id {Id.Handle.Data.Data} (ParentId={ParentId.Handle.Data.Data}) (FullPath={FullPath})");
    }
}