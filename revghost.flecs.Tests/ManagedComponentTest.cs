using revghost.flecs.Utilities.Generator;

namespace revghost.flecs.Tests;

public partial class ManagedComponentTest
{
    [ManagedField<string>("MyManagedString")]
    public partial struct ManagedComponent : IComponent
    {
        public int MyValueInt;
    }

    [Test]
    public void Test()
    {
        var world = Bootstrap.CreateEmpty(Array.Empty<string>());
        world.Register<ManagedComponent>();
        
        var ent = world.New()
            .Set(new ManagedComponent
            {
                MyManagedString = "Hello World!",
                MyValueInt = 42
            });
        
        Assert.AreEqual("Hello World!", ent.Get<ManagedComponent>().MyManagedString);
        Assert.That(ent.Get<ManagedComponent>().MyValueInt, Is.EqualTo(42));
    }
}