namespace UnrealSharp.Utils.UnrealEngine;

/// <summary>
/// Class UENUMAttribute.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
// ReSharper disable once InconsistentNaming
public class UENUMAttribute : UUnrealAttribute<EEnumFlags> 
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UENUMAttribute"/> class.
    /// </summary>
    public UENUMAttribute() { }
    /// <summary>
    /// Initializes a new instance of the <see cref="UENUMAttribute"/> class.
    /// </summary>
    /// <param name="flags">The flags.</param>
    public UENUMAttribute(EEnumFlags flags) : base(flags)
    {
    }
}