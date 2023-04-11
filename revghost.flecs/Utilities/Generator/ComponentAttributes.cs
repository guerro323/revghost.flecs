namespace revghost.flecs.Utilities.Generator;

/// <summary>
/// Add a managed field to a <see cref="IComponent"/> struct
/// </summary>
/// <typeparam name="T">The type of the managed field</typeparam>
/// <remarks>
/// If you wish to use a generic argument from the struct, use the generic-less version <see cref="ManagedFieldAttribute"/>
/// </remarks>
public class ManagedFieldAttribute<T> : Attribute
{
    public ManagedFieldAttribute(string name) {}
}

/// <summary>
/// Add a managed field to a <see cref="IComponent"/> struct
/// </summary>
public class ManagedFieldAttribute : Attribute
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name">The name of the managed field</param>
    /// <param name="type">The type of the managed field, prepend with $ to indicate a generic argument (such as $T)</param>
    public ManagedFieldAttribute(string name, string type) {}
}