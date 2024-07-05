using Newtonsoft.Json;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace UnrealSharpTool.Core.TypeInfo;

/// <summary>
/// Class StructTypeDefinition.
/// for UStruct*
/// Implements the <see cref="UnrealSharpTool.Core.TypeInfo.BaseTypeDefinition" />
/// </summary>
/// <seealso cref="UnrealSharpTool.Core.TypeInfo.BaseTypeDefinition" />
public abstract class StructTypeDefinition : BaseTypeDefinition
{
    /// <summary>
    /// Gets or sets the dependency namespaces.
    /// </summary>
    /// <value>The depend namespaces.</value>
    public List<string> DependNamespaces { get; set; } = [];

    /// <summary>
    /// Gets or sets the properties.
    /// </summary>
    /// <value>The properties.</value>
    public List<PropertyDefinition> Properties { get; set; } = [];

    /// <summary>
    /// Gets a value indicating whether this instance has properties.
    /// </summary>
    /// <value><c>true</c> if this instance has properties; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool HasProperties => Properties.Count > 0;

    /// <summary>
    /// Gets the count.
    /// </summary>
    /// <value>The count.</value>
    [JsonIgnore]
    public int Count => Properties.Count;

    /// <summary>
    /// Adds the dependency namespace.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    public void AddDependencyNamespace(string @namespace)
    {
        if(@namespace != "UnrealSharp.UnrealEngine" && !DependNamespaces.Contains(@namespace))
        {
            DependNamespaces.Add(@namespace);
        }
    }
}