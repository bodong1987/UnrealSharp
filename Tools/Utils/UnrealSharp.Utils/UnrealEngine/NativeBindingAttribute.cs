namespace UnrealSharp.Utils.UnrealEngine;

// ReSharper disable NotAccessedField.Global

/// <summary>
/// Class NativeBindingAttribute.
/// Implements the <see cref="Attribute" />
/// </summary>
/// <seealso cref="Attribute" />
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface)]
public class NativeBindingAttribute : Attribute
{
    /// <summary>
    /// The name
    /// </summary>
    public readonly string Name;
    /// <summary>
    /// The CPP name
    /// </summary>
    public readonly string CppName;
    /// <summary>
    /// The path
    /// </summary>
    public readonly string Path;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeBindingAttribute" /> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="cppName">Name of the CPP.</param>
    /// <param name="path">The path.</param>
    public NativeBindingAttribute(string name, string cppName, string path)
    {
        Name = name;
        CppName = cppName;
        Path = path;
    }
}