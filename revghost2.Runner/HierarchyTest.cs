using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using flecs_hub;
using revghost2.Components;
using revghost2.Utilities.Generator;

namespace revghost2.Runner;

public partial class HierarchyTest
{
    public struct Local : IComponent<Module>
    {
    }

    public struct World : IComponent<Module>
    {
    }

    public struct Position : IComponent<Module>
    {
        public Vector3 Value;
    }

    public partial struct TestSystem : ISystem<Module>
    {
        [Pair<Local>] public readonly Position localPos;
        [Pair<World>] public Position worldPos;

        [Pair<World>, Traversal<Parent, Cascade>]
        public readonly Position? maybeParentPos;

        public void Each()
        {
            worldPos = localPos;
            if (maybeParentPos is { } parentPos)
            {
                worldPos.Value += parentPos.Value;
            }
        }
    }

    [Write<Position>]
    public partial struct WriteSystem : ISystem<Module>
    {
        public void Each()
        {
            _ = Entity;
            World.New();
        }
    }

    public struct Module : IModule
    {
        public static void Setup(revghost2.World world)
        {
            world.Register<Position>();
            world.Register<Local>();
            world.Register<World>();
            world.Register<TestSystem>();

            var parent = world.New("Parent")
                .Set<Local, Position>(new Position {Value = new Vector3(2, 1, 0)})
                .Add<World, Position>();
            world.New("LonesomeFellow")
                .Set<Local, Position>(new Position {Value = new Vector3(0.5f, 0, 0)})
                .Add<World, Position>();
            world.New("Child")
                .Set<Local, Position>(new Position {Value = new Vector3(0, 2, 4)})
                .Add<World, Position>()
                .Add(NativeFunctions.ChildOf(parent));
        }
    }
}