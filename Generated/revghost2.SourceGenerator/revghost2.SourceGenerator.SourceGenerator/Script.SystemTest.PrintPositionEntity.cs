
using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using flecs_hub;
using revghost2.Utilities.Generator;

namespace revghost2.Runner;


partial class SystemTest
{
    public partial struct PrintPositionEntity
    {
        public readonly static string Expression = $"[in]{(revghost2.StaticEntity<global::revghost2.Runner.SystemTest.Position>.FullPath)}";
        
        
        public readonly global::revghost2.Runner.SystemTest.Position pos;
    }
    public static unsafe void Setup(revghost2.World world)
    {
        var entity = __FLECS__.ecs_get_scope(world.Handle);
        EcsSourceGenerator.SetupSystemManaged(world.Handle, entity, PrintPositionEntity.Expression, &EachUnmanaged);
        _ = nameof(Each); // reference it so the editor and compiler doesn't complain it's unused
    }
    private partial void Each(PrintPositionEntity entity);
}