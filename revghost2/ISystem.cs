using System.ComponentModel;

namespace revghost2;

/// <summary>
/// Represent an entity filter that can be used in/as a query, system or processor.
/// </summary>
public interface IEntityFilter
{
}

public interface IProcessor : IStaticEntity, IStaticEntitySetup, IEntityFilter
{
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