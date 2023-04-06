
using __FLECS__ = flecs_hub.flecs;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using flecs_hub;
using revghost2.Utilities.Generator;

namespace revghost2.Runner;


partial class SystemTest
{
    partial struct Velocity
    {
        public readonly static string Expression = $"[inout]{(revghost2.StaticEntity<global::System.Numerics.Vector3>.FullPath)}";
        
    }
}