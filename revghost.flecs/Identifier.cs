namespace revghost.flecs;

using flecs = flecs_hub.flecs;

public struct Identifier
{
    public flecs.ecs_id_t Handle;

    // TODO: Pair version of Is/IsNot
    public bool Is<TStatic>()
        where TStatic : IStaticEntity
    {
        return Handle.Data == StaticEntity<TStatic>.Id.Handle.Data;
    }
    
    public bool IsNot<TStatic>()
        where TStatic : IStaticEntity
    {
        return Handle.Data != StaticEntity<TStatic>.Id.Handle.Data;
    }

    public static implicit operator Identifier(Entity entity)
    {
        Identifier ret;
        ret.Handle = entity.Id.Handle.Data;
        return ret;
    }
    
    public static implicit operator Identifier(EntityId entity)
    {
        Identifier ret;
        ret.Handle = entity.Handle.Data;
        return ret;
    }
    
    public static implicit operator Identifier(Pair pair)
    {
        Identifier ret;
        ret.Handle = pair.Id.Handle.Data;
        return ret;
    }
    
    public static implicit operator Identifier(PairId pairId)
    {
        Identifier ret;
        ret.Handle = pairId.Handle.Data;
        return ret;
    }
    
    public static implicit operator Identifier(flecs.ecs_id_t raw)
    {
        Identifier ret;
        ret.Handle = raw;
        return ret;
    }
    
    public static implicit operator Identifier(flecs.ecs_entity_t raw)
    {
        Identifier ret;
        ret.Handle = raw;
        return ret;
    }
}