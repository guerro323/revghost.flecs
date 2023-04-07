using flecs_hub;

namespace revghost2;

public interface IObserveAction : IComponent
{
    
}

public class OnAdd : IObserveAction, IStaticEntityReserveId
{
    public static EntityId? ReservedId()
    {
        return flecs.EcsOnAdd;
    }
}

public class OnSet : IObserveAction, IStaticEntityReserveId
{
    public static EntityId? ReservedId()
    {
        return flecs.EcsOnSet;
    }
}

public class OnRemove : IObserveAction, IStaticEntityReserveId
{
    public static EntityId? ReservedId()
    {
        return flecs.EcsOnRemove;
    }
}

public interface IObserver<TObserve> : IProcessor
    where TObserve : IObserveAction
{
}