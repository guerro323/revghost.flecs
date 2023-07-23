using System.Runtime.CompilerServices;

namespace flecs_hub;

public unsafe partial class flecs
{
    public static ulong ecs_pair(ecs_entity_t pred, ecs_entity_t obj)
    {
        return ECS_PAIR.Data | ecs_entity_t_comb(obj.Data.Data, pred.Data.Data);
    }
    
    public static ulong ecs_entity_t_comb(ulong lo, ulong hi)
    {
        return (hi << 32) + (uint)lo;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ecs_entity_t ecs_pair_first(ecs_world_t* world, ecs_entity_t entity)
    {
        var value = ECS_PAIR_FIRST(entity.Data.Data);
        return ecs_get_alive(world, *(ecs_entity_t*)&value);
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ecs_entity_t ecs_pair_second(ecs_world_t* world, ecs_entity_t entity)
    {
        var value = ECS_PAIR_SECOND(entity.Data.Data);
        return ecs_get_alive(world, *(ecs_entity_t*)&value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong ECS_PAIR_FIRST(ulong entity)
    {
        var value = entity & ECS_COMPONENT_MASK;
        return ecs_entity_t_hi(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong ECS_PAIR_SECOND(ulong entity)
    {
        var value = entity;
        return ecs_entity_t_lo(value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint ecs_entity_t_hi(ulong value)
    {
        return (uint)(value >> 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint ecs_entity_t_lo(ulong value)
    {
        return (uint)value;
    }
}