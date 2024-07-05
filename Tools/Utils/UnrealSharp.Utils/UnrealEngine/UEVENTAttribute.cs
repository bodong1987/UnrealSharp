namespace UnrealSharp.Utils.UnrealEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

/// <summary>
/// Implements the <see cref="UnrealSharp.Utils.UnrealEngine.UFUNCTIONAttribute" />
/// </summary>
/// <seealso cref="UnrealSharp.Utils.UnrealEngine.UFUNCTIONAttribute" />
[AttributeUsage(AttributeTargets.Method)]
public class UEVENTAttribute : UFUNCTIONAttribute
{
    /// <summary>
    /// Gets or sets a value indicating whether this instance is reliable.
    /// </summary>
    /// <value><c>true</c> if this instance is reliable; otherwise, <c>false</c>.</value>
    public bool IsReliable { get; set; }

    /// <summary>
    /// Gets or sets the replicates.
    /// </summary>
    /// <value>The replicates.</value>
    public FunctionReplicateType Replicates { get; set; } = FunctionReplicateType.NotReplicated;

    /// <summary>
    /// Initializes a new instance of the <see cref="UEVENTAttribute"/> class.
    /// </summary>
    public UEVENTAttribute() : base(EFunctionFlags.BlueprintEvent|EFunctionFlags.Event)
    {
    }
        
    /// <summary>
    /// Initializes a new instance of the <see cref="UEVENTAttribute"/> class.
    /// </summary>
    /// <param name="flags">The flags.</param>
    public UEVENTAttribute(EFunctionFlags flags) 
        : base(flags | EFunctionFlags.BlueprintEvent | EFunctionFlags.Event)
    {
    }
}