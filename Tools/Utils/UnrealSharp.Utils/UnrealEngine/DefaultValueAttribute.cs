namespace UnrealSharp.Utils.UnrealEngine;

// ReSharper disable NotAccessedField.Global

/// <summary>
/// Class DefaultValueTextAttribute.
/// Implements the <see cref="System.Attribute" />
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Parameter)]
public class DefaultValueTextAttribute : Attribute
{
    /// <summary>
    /// The text
    /// </summary>
    public readonly string Text;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultValueTextAttribute"/> class.
    /// </summary>
    /// <param name="text">The text.</param>
    public DefaultValueTextAttribute(string text)
    {
        Text = text;
    }
}