using System.Runtime.InteropServices;

namespace revghost2.Runner;

public class RelationShipComponent
{
    public struct Requires : IComponent
    {
        public float Amount;
    }
    
    public struct Expires : IComponent
    {
        public float Timeout;
    }
    
    public struct Position : IComponent
    {
        public float X;
        public float Y;
    }

    public struct Gigawatts : ITag
    {
    }

    public struct MustHave : ITag
    {
    }
    
    public struct Module : IModule
    {
        public static void Setup(World world)
        {
            world.Register<Requires>();
            world.Register<Expires>();
            world.Register<Position>();
            world.Register<Gigawatts>();
            world.Register<MustHave>();
            
            world.New("e1")
                .Set<Requires, Gigawatts>(new Requires {Amount = 1.21f});
            world.New("e2")
                .Set<Gigawatts, Requires>(new Requires {Amount = 1.21f});
            
            var e3 = world.New("e3")
                .Set<Expires, Position>(new Expires {Timeout = 2.5f});
            var e4 = world.New("e4")
                .Set<Position, Expires>(new Position {X = 0.5f, Y = 1f});

            Console.WriteLine($"{e3.GetFirst<Expires, Position>().Timeout}");
            Console.WriteLine($"{e4.GetSecond<Expires, Position>().X}");
        }
    }
}