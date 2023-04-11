using revghost.flecs.Utilities.Generator;

namespace revghost.flecs.Tests;

public partial class CustomPhaseTest
{
    public struct MyCustomPhase : IStaticEntity {}

    [ManagedField<string>("Value")]
    public partial struct Message : IComponent
    {
        
    }

    [Phase(typeof(MyCustomPhase))]
    public partial struct MySystemInCustomPhase : ISystem
    {
        public readonly Message Message;
        
        public void Each()
        {
            Console.WriteLine($"Message received: {string.Format(Message.Value, Entity.Name.ToString())}");
        }
    }
    
    [Test]
    public void Test()
    {
        var world = Bootstrap.CreateEmpty(Array.Empty<string>());
        var phase = world.Register<MyCustomPhase>();
        phase.Add(EcsPhase);
        
        world.Register<Message>();
        world.Register<MySystemInCustomPhase>();
        
        world.New("George")
            .Set(new Message { Value = "Hi from {0}" });
        
        world.Progress(0.1f);
    }
}