namespace UnrealSharp.Utils.UnrealEngine;

/// <summary>
/// Class USTRUCTAttribute.
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
// ReSharper disable once InconsistentNaming
public class USTRUCTAttribute : UUnrealAttribute<EStructFlags>
{
    /// <summary>
    /// The native size
    /// </summary>
    public int NativeSize { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="USTRUCTAttribute"/> class.
    /// </summary>
    public USTRUCTAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="USTRUCTAttribute"/> class.
    /// </summary>
    /// <param name="flags">The flags.</param>
    public USTRUCTAttribute(EStructFlags flags) : base(flags)
    {
    }
}