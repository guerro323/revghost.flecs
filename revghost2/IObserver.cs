using flecs_hub;

namespace revghost2;

public interface IObserveAction : IComponent
{
    
}

public class OnAdd : IObserveAction, IStaticEntityReserveId
{
    public static EntityId? ReservedId()
    {
        return flecs.pinvoke_EcsOnAdd();
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
        return flecs.pinvoke_EcsOnRemove();
    }
}

public interface IObserver<TObserve> : IProcessor
    where TObserve : IObserveAction
{
}