namespace revghost2;

public interface IModule : IStaticEntity, IStaticEntitySetup, IStaticEntityParent
{
    static EntityId IStaticEntityParent.Parent()
    {
        return StaticEntity<RootModule>.Id;
    }
}

public interface IModule<TParent> : IModule, IStaticEntityParent<TParent> 
    where TParent : IStaticEntity
{
    
}

public readonly struct RootModule : IModule, IStaticEntityCustomName
{
    public static EntityId Parent()
    {
        return default;
    }

    public static void Setup(World world)
    {
    }

    public static string Name()
    {
        return "mods";
    }
}