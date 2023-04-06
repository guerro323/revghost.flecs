using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using flecs_hub;

namespace revghost2;

public interface IComponent : IStaticEntity, IStaticEntitySetup.WithSelf, IStaticEntityIsType.WithSelf
{
    static unsafe void IStaticEntitySetup.WithSelf.Setup<T>(World world)
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

                Console.WriteLine($"add member [{i}] {field.Name} ({StaticEntityInternal.GetData(field.Type).Name})");
            }

            flecs.ecs_struct_init(world.Handle, &structDesc);
        }
    }

    static StaticEntityTypeData.Field[] IStaticEntityIsType.WithSelf.Members<T>()
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