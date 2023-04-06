using System.Reflection;
using revghost2.Utilities.Generator;
using static flecs_hub.flecs;

namespace revghost2;

public unsafe class EcsSourceGenerator
{
    public static void SetupSystemManaged(ecs_world_t* world, EntityId entity, ecs_filter_desc_t filter, void* ctx, Type type, delegate*unmanaged<ecs_iter_t*, void> callback)
    {
        var phase = EcsOnUpdate;
        
        ecs_add_id(world, entity, phase.Data != 0 ? ecs_pair(EcsDependsOn, phase) : default);
        ecs_add_id(world, entity, phase);

        var sysDesc = new ecs_system_desc_t();
        sysDesc.entity = entity;
        sysDesc.ctx = ctx;
        if (filter.terms_buffer_count > 0)
            sysDesc.query.filter = filter;

        sysDesc.callback.Pointer = callback;
        if (type.GetCustomAttribute<MultiThreadedAttribute>() != null)
            sysDesc.multi_threaded = true;
        if (type.GetCustomAttribute<NoReadOnlyAttribute>() != null)
            sysDesc.no_readonly = true;

        ecs_system_init(world, &sysDesc);
    }
    
    public static void SetupObserverManaged(ecs_world_t* world, EntityId entity, ecs_filter_desc_t filter, void* ctx, Type type, delegate*unmanaged<ecs_iter_t*, void> callback)
    {
        var obsDesc = new ecs_observer_desc_t();
        obsDesc.entity = entity;
        obsDesc.ctx = ctx;
        if (filter.terms_buffer_count > 0)
            obsDesc.filter = filter;

        obsDesc.callback.Pointer = callback;

        {
            var i = 0;
            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType || iface.GetGenericTypeDefinition() != typeof(IObserver<>))
                    continue;

                if (i == 8)
                    throw new IndexOutOfRangeException($"Too many subscribers on '{type.FullName}' observer (>8)");

                obsDesc.events[i] = (EntityId) typeof(StaticEntity<>)
                    .MakeGenericType(iface.GenericTypeArguments[0])
                    .GetField("Id")
                    .GetValue(null);
                i += 1;
            }
        }
        
        ecs_observer_init(world, &obsDesc);
    }
}