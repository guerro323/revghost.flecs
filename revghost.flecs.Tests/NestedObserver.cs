using revghost.flecs.Utilities.Generator;

namespace revghost.flecs.Tests;

public partial class NestedObserver
{
    public partial struct Foo : IComponent
    {
        public int Value;
    }

    public partial struct Observer : IEntityObserver<OnSet>
    {
        public Foo foo;
        
        public void Each()
        {
            Console.WriteLine($"{Entity.Name.ToString()} -> {foo.Value}");
            
            if (foo.Value < 10)
            {
                ProcessorContext.World.New("sub")
                    .Set(new Foo {Value = 42});
            }
        }
    }

    [Test]
    public void Test()
    {
        using var world = Bootstrap.CreateEmpty(Array.Empty<string>());
        world.Register<Foo>();
        world.Register<Observer>();

        world.New("e")
            .Set(new Foo {Value = 4});
    }
}