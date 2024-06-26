using Newtonsoft.Json;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharpTool.Core.TypeInfo
{
    /// <summary>
    /// Class FunctionTypeDefinition.
    /// Implements the <see cref="UnrealSharpTool.Core.TypeInfo.StructTypeDefinition" />
    /// </summary>
    /// <seealso cref="UnrealSharpTool.Core.TypeInfo.StructTypeDefinition" />
    public class FunctionTypeDefinition : StructTypeDefinition
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is override function.
        /// </summary>
        /// <value><c>true</c> if this instance is override function; otherwise, <c>false</c>.</value>
        public bool IsOverrideFunction { get; set; }

        /// <summary>
        /// Gets or sets the signature.
        /// </summary>
        /// <value>The signature.</value>
        public string Signature { get; set; } = "";

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        [JsonIgnore]
        public StructTypeDefinition? Parent { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is static.
        /// </summary>
        /// <value><c>true</c> if this instance is static; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsStatic => (Flags & (UInt32)EFunctionFlags.Static) != 0;

        /// <summary>
        /// Gets a value indicating whether this instance is public.
        /// </summary>
        /// <value><c>true</c> if this instance is public; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsPublic => (Flags & (UInt32)EFunctionFlags.Public) != 0;

        /// <summary>
        /// Gets a value indicating whether this instance is protected.
        /// </summary>
        /// <value><c>true</c> if this instance is protected; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsProtected => (Flags & (UInt32)EFunctionFlags.Protected) != 0;

        /// <summary>
        /// Gets a value indicating whether this instance is private.
        /// </summary>
        /// <value><c>true</c> if this instance is private; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsPrivate => (Flags & (UInt32)EFunctionFlags.Private) != 0;

        /// <summary>
        /// Gets a value indicating whether this instance is final.
        /// </summary>
        /// <value><c>true</c> if this instance is final; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsFinal => (Flags & (UInt32)EFunctionFlags.Final) != 0;

        /// <summary>
        /// Gets a value indicating whether this instance is event.
        /// </summary>
        /// <value><c>true</c> if this instance is event; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsEvent => (Flags & (UInt32)EFunctionFlags.BlueprintEvent) != 0;

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        /// <value>The name of the function.</value>
        [JsonIgnore]
        public string FunctionName
        {
            get
            {                
                return Name!;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionTypeDefinition"/> class.
        /// </summary>
        public FunctionTypeDefinition()
        {
            Type = EDefinitionType.Function;
        }

        /// <summary>
        /// Gets the type of the return.
        /// </summary>
        /// <returns>System.Nullable&lt;PropertyDefinition&gt;.</returns>
        public PropertyDefinition? GetReturnType()
        {
            return Properties.Find(x => (x.Flags & (ulong)EPropertyFlags.ReturnParm) != 0);
        }

        /// <summary>
        /// Determines whether [has return type].
        /// </summary>
        /// <returns><c>true</c> if [has return type]; otherwise, <c>false</c>.</returns>
        public bool HasReturnType()
        {
            return GetReturnType() != null;
        }

        /// <summary>
        /// Gets the parameter count.
        /// </summary>
        /// <value>The parameter count.</value>
        [JsonIgnore]
        public int ParameterCount => HasReturnType() ? Properties.Count-1 : Properties.Count;
    }
}
