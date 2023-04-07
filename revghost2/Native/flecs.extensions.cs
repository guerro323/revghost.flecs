namespace flecs_hub;

public partial class flecs
{
    public static ulong ecs_pair(ecs_entity_t pred, ecs_entity_t obj)
    {
        return ECS_PAIR.Data | ecs_entity_t_comb(obj.Data.Data, pred.Data.Data);
    }
    
    public static ulong ecs_entity_t_comb(ulong lo, ulong hi)
    {
        return (hi << 32) + (uint)lo;
    }
}