using flecs_hub;

namespace revghost2.Components;

public class NativeFunctions
{
    public static Pair ChildOf(Entity parent)
    {
        return Pair.From(parent.World, EntityId.ChildOf, parent);
    }
}