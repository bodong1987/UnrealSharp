namespace UnrealSharp.Utils.UnrealEngine;

/// <summary>
/// Enum FunctionReplicateType
/// </summary>
public enum FunctionReplicateType
{
    /// <summary>
    /// The not replicated
    /// </summary>
    NotReplicated = 0,

    /// <summary>
    /// The multicast
    /// </summary>
    Multicast = (int)EFunctionFlags.NetMulticast|(int)EFunctionFlags.Net,

    /// <summary>
    /// The client
    /// </summary>
    Client = (int)EFunctionFlags.NetClient | (int)EFunctionFlags.Net,

    /// <summary>
    /// The server
    /// </summary>
    Server = (int)EFunctionFlags.NetServer | (int)EFunctionFlags.Net
}

/// <summary>
/// Class UFUNCTIONAttribute.
/// https://docs.unrealengine.com/5.3/en-US/ufunctions-in-unreal-engine/
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
// ReSharper disable once InconsistentNaming
public class UFUNCTIONAttribute : UUnrealAttribute<EFunctionFlags>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UFUNCTIONAttribute"/> class.
    /// </summary>
    public UFUNCTIONAttribute() { }
    /// <summary>
    /// Initializes a new instance of the <see cref="UFUNCTIONAttribute"/> class.
    /// </summary>
    /// <param name="flags">The flags.</param>
    // ReSharper disable once MemberCanBeProtected.Global
    public UFUNCTIONAttribute(EFunctionFlags flags) : base(flags)
    {
    }
}