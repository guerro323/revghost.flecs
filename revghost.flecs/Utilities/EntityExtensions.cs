using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using flecs_hub;
using static flecs_hub.flecs;

namespace revghost.flecs;

public static unsafe class EntityExtensions
{
    public static void Run<T>(this Entity entity, float deltaTime, ref T arguments)
    {
        fixed (T* ptrArg = &arguments)
        {
            ecs_run(entity.World.Handle, entity.Id, deltaTime, ptrArg);
        }
    }

    public static void Run(this Entity entity, float deltaTime)
    {
        ecs_run(entity.World.Handle, entity.Id, deltaTime, null);
    }

    public static Entity AddChild(this Entity parent, Entity child)
    {
        parent.Add((EcsChildOf, child));
        return parent;
    }

    public static Entity SetParent(this Entity child, Entity parent)
    {
        parent.Add((EcsChildOf, child));
        return child;
    }

    public static Entity DependsOn(this Entity entity, Entity dependency)
    {
        entity.Add((EcsDependsOn, dependency));
        return entity;
    }
}