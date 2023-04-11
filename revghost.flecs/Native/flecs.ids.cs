using System.Reflection;
using System.Runtime.InteropServices;

namespace flecs_hub;

public unsafe partial class flecs
{
    static flecs()
    {
        var lib = NativeLibrary.Load("Native/libs/libflecs.so");
        
        ecs_entity_t GetEntity(string name)
        {
            return *(ecs_entity_t*) NativeLibrary.GetExport(lib, name);
        }

        foreach (var field in typeof(flecs).GetFields(BindingFlags.Static | BindingFlags.Public))
        {
            if (!field.IsInitOnly)
                continue;
            
            field.SetValue(null, GetEntity(field.Name));
        }
    }
    
    // Roles
    public static readonly ecs_entity_t ECS_PAIR;
    public static readonly ecs_entity_t ECS_OVERRIDE;
    
    // Relationships
    public static readonly ecs_entity_t EcsIsA;
    public static readonly ecs_entity_t EcsDependsOn;
    public static readonly ecs_entity_t EcsChildOf;
    public static readonly ecs_entity_t EcsSlotOf;
    
    // Entity tags
    public static readonly ecs_entity_t EcsPrefab;
    
    // System tags
    public static readonly ecs_entity_t EcsPreFrame;
    public static readonly ecs_entity_t EcsOnLoad;
    public static readonly ecs_entity_t EcsPostLoad;
    public static readonly ecs_entity_t EcsPreUpdate;
    public static readonly ecs_entity_t EcsOnUpdate;
    public static readonly ecs_entity_t EcsOnValidate;
    public static readonly ecs_entity_t EcsPostUpdate;
    public static readonly ecs_entity_t EcsPreStore;
    public static readonly ecs_entity_t EcsOnStore;
    public static readonly ecs_entity_t EcsPostFrame;
    public static readonly ecs_entity_t EcsPhase;
    
    public static readonly ecs_entity_t EcsOnAdd;
    public static readonly ecs_entity_t EcsOnSet;
    public static readonly ecs_entity_t EcsOnRemove;
    
    public static readonly ecs_entity_t FLECS__Eecs_f32_t;
    public static readonly ecs_entity_t FLECS__Eecs_i32_t;
}