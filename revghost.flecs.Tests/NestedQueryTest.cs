using System.Numerics;
using System.Runtime.CompilerServices;

namespace revghost.flecs.Tests;

public partial class NestedQueryTest
{
    public partial struct Position : IComponent
    {
        public Vector2 Value;
    }

    public partial struct HitSize : IComponent
    {
        public float Value;
    }

    public partial struct GlobalSystem : ISystem
    {
        public readonly Position pos;
        public readonly HitSize size;

        private NestedQuery _query;

        public void Each()
        {
            Console.WriteLine($"{Id} will start colliding...");
            foreach (var it in _query)
            {
                if (Id <= it.Id)
                    continue;

                if (Vector2.Distance(pos.Value, it.pos.Value) > size.Value + it.size.Value)
                    continue;

                Console.WriteLine($"  {Id} and {it.Id} collided!");
            }
        }

        public partial struct NestedQuery : IEntityFilter
        {
            public readonly Position pos;
            public readonly HitSize size;
        }
    }

    [Test]
    public void TestNested()
    {
        using var world = Bootstrap.CreateEmpty(Array.Empty<string>());
        world.Register<Position>();
        world.Register<HitSize>();
        world.Register<GlobalSystem>();

        var rand = Random.Shared;
        for (var i = 0; i < 10; i++)
        {
            world.New()
                .Set(new Position {Value = new Vector2(rand.NextSingle() * 100, rand.NextSingle() * 100)})
                .Set(new HitSize {Value = rand.NextSingle() * 20 + 1});
        }
        
        world.Progress(0.16f);
    }
}