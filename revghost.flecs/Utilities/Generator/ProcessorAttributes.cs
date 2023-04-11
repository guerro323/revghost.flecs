namespace revghost.flecs.Utilities.Generator;

/// <summary>
/// Represent a <see cref="IProcessor"/> state value
/// </summary>
/// <remarks>
/// In a multi-threaded context it is not thread-safe to edit this value, so it is recommended to have it as read-only value.
/// If this value is set to read-only, external code can still modify it.
/// </remarks>
public class StateAttribute : Attribute {}

/// <summary>
/// Represent a <see cref="IProcessor"/> param value, given by <see cref="flecs_hub.flecs.ecs_run"/> last <see cref="Void"/> argument
/// </summary>
/// <remarks>
/// In a multi-threaded context it is not thread-safe to edit this value, so it is recommended to have it as read-only value.
/// If this value is set to read-only, external code can still modify it.
/// </remarks>
public class ParamAttribute : Attribute {}

public class PhaseAttribute : Attribute
{
    public readonly Type Type;
    
    public PhaseAttribute(Type type)
    {
        Type = type;
    }
}