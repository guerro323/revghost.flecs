using System.Numerics;
using flecs_hub;
using revghost2.Utilities.Generator;

namespace revghost2.Runner;

public partial class SystemTest
{
    public struct Position : IComponent<Module>
    {
        public Vector3 Value;
    }

    public struct Velocity : IComponent<Module>
    {
        public Vector3 Value;
    }
    
    public partial struct MoveSystem : ISystem<Module>
    {
        public Position pos;
        public readonly Velocity vel;
        
        public void Each()
        {
            pos.Value += vel.Value * SystemDeltaTime;
        }
    }
    
    public partial struct PrintPositionSystem : ISystem<Module>
    {
        public readonly Position pos;
        
        public void Each()
        {
            Console.WriteLine(pos.Value);
        }
    }


    public struct Module : IModule
    {
        public static unsafe void Setup(World world)
        {
            world.Register<Position>();
            world.Register<Velocity>();
            
            world.Register<MoveSystem>();
            world.Register<PrintPositionSystem>();

            world.New()
                .Set(new Position())
                .Set(new Velocity {Value = Vector3.UnitX});
        }
    }
}