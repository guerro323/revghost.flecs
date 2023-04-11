using static flecs_hub.flecs;

namespace revghost.flecs.Utilities.Generator;

public unsafe class ComponentUtility
{
    public static void SetHookCallbacks(ecs_world_t* world, EntityId entityId,
        delegate* unmanaged<void*, int, ecs_type_info_t*, void> ctor = null,
        delegate* unmanaged<void*, int, ecs_type_info_t*, void> dtor = null,
        delegate* unmanaged<void*, void*, int, ecs_type_info_t*, void> move = null,
        delegate* unmanaged<void*, void*, int, ecs_type_info_t*, void> cpy = null,
        void* context = null)
    {
        ecs_type_hooks_t hooks;
        hooks.ctor.Pointer = ctor;
        hooks.dtor.Pointer = dtor;
        hooks.move.Pointer = move;
        hooks.copy.Pointer = cpy;
        hooks.ctx = context;

        ecs_set_hooks_id(world, entityId, &hooks);
    }
}