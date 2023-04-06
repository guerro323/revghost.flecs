
using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Runtime.InteropServices;
using flecs_hub;
using revghost2;
using revghost2.Runner;
using static revghost2.Components.NativeFunctions;

namespace <global namespace>;


partial struct Position
{
    public readonly static string Expression = $"[inout]{(revghost2.StaticEntity<float>.FullPath)},\n[inout]{(revghost2.StaticEntity<global::System.Numerics.Vector3>.FullPath)}";
    
}