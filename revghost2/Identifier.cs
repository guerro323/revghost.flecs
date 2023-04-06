using flecs_hub;

namespace revghost2;

public struct Identifier
{
    public flecs.ecs_id_t Handle;

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
}