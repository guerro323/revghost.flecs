using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using flecs_hub;
using static flecs_hub.flecs;

namespace revghost2;

public struct Pair
{
    public World World;
    public PairId Id;

    public static Pair From(Entity left, Entity right)
    {
        if (left.World != right.World)
            throw new InvalidOperationException("not the same world");

        Pair ret;
        ret.World = left.World;
        ret.Id.Handle = ecs_pair(left.Id.Handle, right.Id.Handle);
        return ret;
    }
    
    public static Pair From(World world, EntityId left, EntityId right)
    {
        // Console.WriteLine($"Pair {left.Handle.Data.Data} with {right.Handle.Data.Data}");
        
        Pair ret;
        Unsafe.SkipInit(out ret);
        
        ret.World = world;

        ret.Id.Handle = ecs_pair(left.Handle, right.Handle);
        return ret;
    }
    
    public static implicit operator Pair((Entity left, Entity right) tuple)
    {
        return From(tuple.left, tuple.right);
    }
    
    public static implicit operator Pair((EntityId left, Entity right) tuple)
    {
        return From(tuple.right.World, tuple.left, tuple.right);
    }
    
    public static implicit operator Pair((Entity left, EntityId right) tuple)
    {
        return From(tuple.left.World, tuple.left, tuple.right);
    }

    public override unsafe string ToString()
    {
        return $"Pair(World: {(IntPtr) World.Handle}, Id: {Id.Handle.Data})";
    }
}

public struct PairId
{
    public ecs_id_t Handle;
    
    public static PairId From(EntityId left, EntityId right)
    {
        PairId ret;
        ret.Handle = ecs_pair(left.Handle, right.Handle);
        return ret;
    }

    public Pair WithWorld(World world)
    {
        Pair ret;
        ret.World = world;
        ret.Id = this;
        return ret;
    }
    
    public static implicit operator PairId((EntityId left, EntityId right) tuple)
    {
        return From(tuple.left, tuple.right);
    }

    public override string ToString()
    {
        var ints = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ulong, uint>(ref Handle.Data), 2);
        return $"PairId({ints[0]} | {ints[1]})";
    }
}