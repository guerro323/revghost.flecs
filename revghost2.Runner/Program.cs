// See https://aka.ms/new-console-template for more information

using System.Numerics;
using System.Runtime.InteropServices;
using flecs_hub;
using revghost2;
using revghost2.Runner;
using static revghost2.Components.NativeFunctions;

unsafe
{
    Prout.DoMyStuff();
    return;

    [UnmanagedCallersOnly]
    static void Callback(flecs.ecs_iter_t* it)
    {
        var positions = (Position*) flecs.ecs_field_w_size(it, (ulong) ManagedTypeData<Position>.Size, 1);
        for (var i = 0; i < it->count; i++)
        {
            positions[i].Vector.Y += it->delta_time;
        }

        // Console.WriteLine($"{TimeSpan.FromSeconds(it->delta_time).TotalMilliseconds:F2}ms");
    }

    var appDesc = new flecs.ecs_app_desc_t();
    appDesc.enable_monitor = true;
    appDesc.enable_rest = true;
    appDesc.threads = 4;
    appDesc.target_fps = 30;

    var world = flecs.ecs_init();
    {
        var entDesc = new flecs.ecs_entity_desc_t();
        entDesc.name = (flecs.Runtime.CString) "this is bob, say hi to bob";
        var ent = flecs.ecs_entity_init(world, &entDesc);
    }

    {
        var view = new World(world);
        view.New();
        view.New(new World.EntityCreation
        {
            Name = "Hello!"
        });

        var parent = view.New("parent");
        view.New("AnotherChild")
            .Add(ChildOf(parent));
        
        view.New("Fromage")
            .Add((EntityId.ChildOf, parent));

        view.New(new()
        {
            Name = "Child",
            Add = stackalloc Identifier[]
            {
                ChildOf(parent)
            }
        });

        view.New();
        view.New();
        
        if (true)
        {
            view.Register<GuerroModule>();

            var jsonDesc = new flecs.ecs_world_to_json_desc_t();
            var str = new NativeStringView(flecs.ecs_world_to_json(world, &jsonDesc));
            Console.WriteLine($"{str.ToString()}");

            for (var i = 0; i < 10; i++)
                view.New()
                    .Add<Tag>()
                    .Set(new Position {Value = 42f, Vector = new Vector3(1, 2, 3)});
        }
    }
    
    {
        var parentDesc = new flecs.ecs_entity_desc_t();
        parentDesc.name = (flecs.Runtime.CString) "Container";
        
        var phase = flecs.EcsOnUpdate;
        
        var entDesc = new flecs.ecs_entity_desc_t();
        entDesc.name = (flecs.Runtime.CString) "cheese system";
        entDesc.add[0] = phase.Data != 0 ? flecs.ecs_pair(flecs.EcsDependsOn, phase) : default;
        entDesc.add[1] = phase;
        entDesc.add[2] = flecs.ecs_pair(flecs.EcsChildOf, flecs.ecs_entity_init(world, &parentDesc));
        
        var sysDesc = new flecs.ecs_system_desc_t();
        sysDesc.entity = flecs.ecs_entity_init(world, &entDesc);
        sysDesc.query.filter.expr = flecs.Runtime.CString.FromString("Modules.GuerroModule.Position");
        sysDesc.callback.Pointer = &Callback;
        flecs.ecs_system_init(world, &sysDesc);
    }

    Console.WriteLine($"{HierarchyTest.TestSystem.Filter}");
    //Console.WriteLine($"{StaticEntity<HierarchyTest.Local>.Name}");
    
    // var expr = $"[in]({StaticEntity<HierarchyTest.Local>.Name}, {StaticEntity<HierarchyTest.Position>.Name}), [inout]({StaticEntity<HierarchyTest.World>.Name}, {StaticEntity<HierarchyTest.Position>.Name}), [in]({StaticEntity<HierarchyTest.World>.Name}, {StaticEntity<HierarchyTest.Position>.Name})";
    // Console.WriteLine(expr);
    
    var code = flecs.ecs_app_run(world, &appDesc);
    Console.WriteLine(code);
}

struct GuerroModule : IModule
{
    public static void Setup(World world)
    {
        Console.WriteLine("setup");
        world.Register<Tag>();
        world.Register<Position>();
    }
}

struct Tag : IComponent<GuerroModule>
{
    
}

struct Position : IComponent<GuerroModule>
{
    public float Value;
    public Vector3 Vector;
}