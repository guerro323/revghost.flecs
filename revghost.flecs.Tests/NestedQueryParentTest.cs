using System.Numerics;
using revghost.flecs.Utilities.Generator;

namespace revghost.flecs.Tests;

/*
 * Test the source generator if it can generate code from patterns with nested queries
 */

public partial class NestedQueryParentTest
{
    public partial struct Position : IComponent
    {
        public Vector2 Value;
    }

    public partial struct GlobalSystem : ISystem
    {
        [Traversal<Parent, Cascade>]
        public readonly Position? maybeParentPos;

        private NestedQuery _query;

        public void Each()
        {
            foreach (var it in _query)
            {
                if (Id <= it.Id)
                    continue;

                if (maybeParentPos is { } parentPos && it.maybeParentPos is { } itParentPos)
                {
                    if (Vector2.Distance(parentPos.Value, itParentPos.Value) < 1f)
                    {
                        Console.WriteLine($"{Id} and {it.Id} has close parents!");
                    }
                }
            }
        }

        public partial struct NestedQuery : IEntityFilter
        {
            [Traversal<Parent, Cascade>]
            public readonly Position? maybeParentPos;
        }
    }

    [Test]
    public void TestNested()
    {
        using var world = Bootstrap.CreateEmpty(Array.Empty<string>());
        world.Register<Position>();
        world.Register<GlobalSystem>();

        var rand = Random.Shared;
        for (var i = 0; i < 10; i++)
        {
            var parent = world.New()
                .Set(new Position {Value = new Vector2(rand.NextSingle() * 10, rand.NextSingle() * 10)});
            world.New().Add((EntityId.ChildOf, parent));
        }

        world.Progress(0.16f);
    }
}