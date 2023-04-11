namespace revghost.flecs;

using flecs = flecs_hub.flecs;

public interface IObserveAction : IComponent
{
    
}

public class OnAdd : IObserveAction, IComponentAuto, IStaticEntityReserveId
{
    public static EntityId? ReservedId()
    {
        return flecs.EcsOnAdd;
    }
}

public class OnSet : IObserveAction, IComponentAuto, IStaticEntityReserveId
{
    public static EntityId? ReservedId()
    {
        return flecs.EcsOnSet;
    }
}

public class OnRemove : IObserveAction, IComponentAuto, IStaticEntityReserveId
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