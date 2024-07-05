namespace UnrealSharp.Utils.UnrealEngine;

// ReSharper disable NotAccessedField.Global

/// <summary>
/// Class FastAccessibleAttribute.
/// Implements the <see cref="System.Attribute" />
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Struct|AttributeTargets.Enum|AttributeTargets.Method)]
public class FastAccessibleAttribute : Attribute
{
    /// <summary>
    /// The native size
    /// </summary>
    public readonly int NativeSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="FastAccessibleAttribute"/> class.
    /// </summary>
    public FastAccessibleAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FastAccessibleAttribute"/> class.
    /// </summary>
    /// <param name="nativeSize">Size of the native.</param>
    public FastAccessibleAttribute(int nativeSize)
    {
        NativeSize = nativeSize;
    }
}