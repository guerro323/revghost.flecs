using System.Numerics;
using System.Runtime.InteropServices;
using revghost2.Utilities.Generator;

namespace revghost2.Tests;

public partial class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var world = Bootstrap.Create<EntryModule>(Array.Empty<string>());
        world.Progress(1f);
        world.Progress(1f);
        world.Progress(1f);
        Console.WriteLine("bruh");
    }

    partial struct EntryModule : IModule
    {
        public static void Setup(World world)
        {
            world.Register<Position>();
            world.Register<Velocity>();
            
            world.Register<Hello<Velocity>>();
            world.Register<Hello<Position>>();

            var e = world.New("Entity")
                //.Set(new Position {Value = new Vector3(3, 0, 1)})
                .Set(new Velocity {Value = new Vector3(1, 1, 2)});

            Console.WriteLine(e);
        }

        [Write<Position>]
        public partial struct Hello<T0> : ISystem<EntryModule>
            where T0: IComponent
        {
            public T0 Vector;
            
            public void Each()
            {
                Console.WriteLine($"{typeof(T0).Name} for {Id} is {Vector}");
                Entity.Set(new Position { Value = new Vector3(4) });
            }
        }

        public struct Position : IComponent<EntryModule>
        {
            public Vector3 Value;
            
            public override string ToString()
            {
                return Value.ToString();
            }
        }

        public struct Velocity : IComponent<EntryModule>
        {
            public Vector3 Value;
            
            public override string ToString()
            {
                return Value.ToString();
            }
        }
    }
}