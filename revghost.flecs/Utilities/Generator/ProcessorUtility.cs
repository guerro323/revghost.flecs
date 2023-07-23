using System.Reflection;
using revghost.flecs.Utilities.Generator;
using static flecs_hub.flecs;

namespace revghost.flecs;

public unsafe class ProcessorUtility
{
    public static void SetupSystemManaged(ecs_world_t* world, EntityId entity, ecs_filter_desc_t filter, void* ctx, Type type, delegate*unmanaged<ecs_iter_t*, void> callback)
    {
        var targetTickSource = default(EntityId);
        var phase = EcsOnUpdate;
        if (type.GetCustomAttribute<PhaseAttribute>() is { } phaseAttribute)
        {
            phase = StaticEntity.Get(phaseAttribute.Type).Id;
            targetTickSource = phase;
        }
        
        ecs_add_id(world, entity, phase.Data != 0 ? ecs_pair(EcsDependsOn, phase) : default);
        ecs_add_id(world, entity, phase);

        var sysDesc = new ecs_system_desc_t();
        sysDesc.entity = entity;
        sysDesc.ctx = ctx;
        if (filter.terms_buffer_count > 0)
            sysDesc.query.filter = filter;
        
        // TODO: maybe we shouldn't inherit tick sources like that, instead it should be done with a custom IPhase static entity that automatically inherit it?
        while (targetTickSource != default
               && !ecs_has_id(world, targetTickSource, EcsTimer)
               && ecs_has_id(world, targetTickSource, ecs_make_pair(EcsDependsOn, EcsWildcard)))
        {
            var next = ecs_get_target(world, targetTickSource, EcsDependsOn, 0);
            if (ecs_has_id(world, next, EcsTimer))
            {
                targetTickSource = next;
                break;
            }

            targetTickSource = next;
        }

        if (targetTickSource != default && !ecs_has_id(world, targetTickSource, EcsTimer))
            targetTickSource = default;
        
        sysDesc.tick_source = targetTickSource;

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

        if (type.GetCustomAttribute<YieldExistingAttribute>() != null)
            obsDesc.yield_existing = true;

        {
            var i = 0;
            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType || iface.GetGenericTypeDefinition() != typeof(IEntityObserver<>))
                    continue;

                if (i == 8)
                    throw new IndexOutOfRangeException($"Too many subscribers on '{type.FullName}' observer (>8)");

                obsDesc.events[i] = (EntityId) typeof(StaticEntity<>)
                    .MakeGenericType(iface.GenericTypeArguments[0])
                    .GetField("Id")
                    .GetValue(null);
                foreach (var ev in obsDesc.events)
                {
                    if (ev.Data.Data == default)
                        continue;
                    
                    Console.WriteLine($"sub to {ecs_get_name(world, ev)}");
                }
                
                i += 1;
            }
        }
        
        ecs_observer_init(world, &obsDesc);
    }
}