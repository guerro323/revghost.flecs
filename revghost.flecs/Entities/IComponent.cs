using System.Diagnostics;

namespace revghost.flecs;

using flecs = flecs_hub.flecs;

public interface IComponent : IStaticEntity, IStaticEntitySetup, IStaticEntityIsType
{
    
}

public interface IComponentAuto : IComponent, IStaticEntitySetupWithSelf, IStaticEntityIsTypeWithSelf
{
    static void IStaticEntitySetup.Setup(World world) => throw new UnreachableException();
    static StaticEntityTypeData.Field[] IStaticEntityIsType.Members() => throw new UnreachableException();
    
    static unsafe void IStaticEntitySetupWithSelf.Setup<T>(World world)
    {
        var entity = flecs.ecs_get_scope(world.Handle);
        var desc = new flecs.ecs_component_desc_t
        {
            entity = entity,
            type = new flecs.ecs_type_info_t
            {
                size = ManagedTypeData<T>.Size,
                alignment = ManagedTypeData<T>.Size == 0 ? 0 : 4
            }
        };
        
        var ret = flecs.ecs_component_init(world.Handle, &desc);

        if (ret.Data.Data != entity.Data.Data)
            throw new InvalidOperationException();

        if (StaticEntity<T>.Members is {Length: > 0} members)
        {
            foreach (var field in members)
            {
                world.Register(field.Type);
            }
            
            var structDesc = new flecs.ecs_struct_desc_t();
            structDesc.entity = entity;
            
            var i = 0;
            foreach (var field in members)
            {
                // TODO: dispose it (outside of this scope)
                var nativeName = new NativeString(field.Name);
                structDesc.members[i++] = new flecs.ecs_member_t
                {
                    name = nativeName,
                    type = field.Type
                };

                // todo: better logging
                // Console.WriteLine($"add member [{i}] {field.Name} ({StaticEntity.GetData(field.Type).Name})");
            }

            flecs.ecs_struct_init(world.Handle, &structDesc);
        }
    }

    static StaticEntityTypeData.Field[] IStaticEntityIsTypeWithSelf.Members<T>()
    {
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
}

public interface IComponent<TModule> : IComponent, IStaticEntityParent<TModule>
    where TModule : IModule
{
    
}

public interface ITag : IComponent {}

public interface ITag<TModule> : IComponent<TModule>
    where TModule : IModule
{
}