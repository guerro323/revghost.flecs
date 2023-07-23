using System.ComponentModel;
using System.Runtime.InteropServices;

namespace revghost.flecs;

/// <summary>
/// Represent an entity filter that can be used in/as a query, system or processor.
/// </summary>
public interface IEntityFilter
{
    static abstract flecs_hub.flecs.ecs_filter_desc_t GetFilter();
    
    Entity Entity { get; }
    EntityId Id { get; }
}

public interface IProcessor : IStaticEntity, IStaticEntitySetup, IEntityFilter
{
    static abstract flecs_hub.flecs.ecs_iter_action_t GetAction();
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    void Each();
}

public interface ISystem : IProcessor
{
}

public interface ISystem<TParent> : ISystem, IStaticEntityParent<TParent>
    where TParent : IStaticEntity
{
}