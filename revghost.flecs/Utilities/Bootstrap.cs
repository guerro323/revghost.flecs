namespace revghost.flecs;

using flecs = flecs_hub.flecs;

public static unsafe class Bootstrap
{
    public static World CreateEmpty(string[] args)
    {
        var argsNative = args.Length == 0 ? default : flecs.Runtime.CStrings.CStringArray(args);
        var worldNative = flecs.ecs_init_w_args(args.Length, argsNative);

        return new World(worldNative);
    }

    public static World Create<T>(string[] args)
        where T : IModule
    {
        var world = CreateEmpty(args);
        world.Register<T>();
        
        return world;
    }

    public static int Run<T>(string[] args, ApplicationConfig conf)
        where T : IModule
    {
        using var world = CreateEmpty(args);
        world.Register<T>();

        var appDesc = new flecs.ecs_app_desc_t();
        appDesc.target_fps = conf.TargetFps;
        appDesc.delta_time = conf.ForcedDeltaTime;
        
        return flecs.ecs_app_run(world.Handle, &appDesc);
    }
}

public struct ApplicationConfig
{
    public int TargetFps = 0;
    public float ForcedDeltaTime = 0;

    public ApplicationConfig()
    {
    }
}