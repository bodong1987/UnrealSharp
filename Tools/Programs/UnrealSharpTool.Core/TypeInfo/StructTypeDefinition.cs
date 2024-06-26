using Newtonsoft.Json;
using UnrealSharp.Utils.Extensions;

namespace UnrealSharpTool.Core.TypeInfo
{
    /// <summary>
    /// Class StructTypeDefinition.
    /// for UStruct*
    /// Implements the <see cref="UnrealSharpTool.Core.TypeInfo.BaseTypeDefinition" />
    /// </summary>
    /// <seealso cref="UnrealSharpTool.Core.TypeInfo.BaseTypeDefinition" />
    public abstract class StructTypeDefinition : BaseTypeDefinition
    {
        /// <summary>
        /// Gets or sets the depend namespaces.
        /// </summary>
        /// <value>The depend namespaces.</value>
        public List<string> DependNamespaces { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>The properties.</value>
        public List<PropertyDefinition> Properties { get; set; } = new List<PropertyDefinition>();

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
        /// Adds the depend namespace.
        /// </summary>
        /// <param name="namespace">The namespace.</param>
        public void AddDependNamespace(string @namespace)
        {
            if(@namespace != "UnrealSharp.UnrealEngine" && !DependNamespaces.Contains(@namespace))
            {
                DependNamespaces.Add(@namespace);
            }
        }
    }
}
