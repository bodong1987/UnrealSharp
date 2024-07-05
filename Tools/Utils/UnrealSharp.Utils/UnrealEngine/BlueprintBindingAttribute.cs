namespace UnrealSharp.Utils.UnrealEngine;

/// <summary>
/// Class BlueprintBindingAttribute.
/// Implements the <see cref="Attribute" />
/// </summary>
/// <seealso cref="Attribute" />
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface)]
public class BlueprintBindingAttribute : Attribute
{
    /// <summary>
    /// The path
    /// </summary>
    public readonly string Path;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlueprintBindingAttribute" /> class.
    /// </summary>
    /// <param name="path">The path.</param>
    public BlueprintBindingAttribute(string path)
    {
        Path = path;
    }
}