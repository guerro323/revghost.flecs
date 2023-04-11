using flecs_hub;

namespace revghost.flecs.Tests;

public partial class ObserverTest
{
    public partial struct Foo : IComponent
    {
        public int Value;
    }

    public partial struct ObserveFooAdd : IObserver<OnSet>
    {
        public readonly Foo foo;
        
        public void Each()
        {
            Console.WriteLine($"{Id} -> Foo.Value={foo.Value}");
        }
    }

    [Test]
    public void Test()
    {
        using var world = Bootstrap.CreateEmpty(Array.Empty<string>());
        world.Register<Foo>();
        world.Register<ObserveFooAdd>();

        world.New().Set(new Foo {Value = 4});
        world.New().Set(new Foo {Value = 2});

        Console.WriteLine("[Progress]");
        world.Progress(1);
        
        world.New().Set(new Foo {Value = 8});
        world.New().Set(new Foo {Value = 1});
        
        Console.WriteLine("[Progress]");
        world.Progress(1);
        
        world.New().Set(new Foo {Value = 0});

        Console.WriteLine("[Progress]");
        world.Progress(1);
        
        // nothing
        
        Console.WriteLine("[Progress]");
        world.Progress(1);

        ecs_observer_desc_t desc;
    }
}