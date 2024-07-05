namespace UnrealSharp.Utils.UnrealEngine;

// ReSharper disable NotAccessedField.Global

/// <summary>
/// Class BackendNameAttribute.
/// Implements the <see cref="System.Attribute" />
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Method|AttributeTargets.Field|AttributeTargets.Property|AttributeTargets.Class|AttributeTargets.Struct|AttributeTargets.Interface|AttributeTargets.Enum)]
public class BackendNameAttribute : Attribute
{
    /// <summary>
    /// The backend name
    /// </summary>
    public readonly string BackendName;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackendNameAttribute"/> class.
    /// </summary>
    /// <param name="backendName">The backend name.</param>
    public BackendNameAttribute(string backendName)
    {
        BackendName = backendName;
    }
}