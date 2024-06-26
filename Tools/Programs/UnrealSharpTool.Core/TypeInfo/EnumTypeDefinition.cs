using Newtonsoft.Json;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharpTool.Core.TypeInfo
{
    /// <summary>
    /// Class EnumFieldDefinition.
    /// </summary>
    public class EnumFieldDefinition
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string? Name { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public long Value { get; set; }
    }

    /// <summary>
    /// Class EnumTypeDefinition.
    /// Implements the <see cref="UnrealSharpTool.Core.TypeInfo.BaseTypeDefinition" />
    /// </summary>
    /// <seealso cref="UnrealSharpTool.Core.TypeInfo.BaseTypeDefinition" />
    public class EnumTypeDefinition : BaseTypeDefinition
    {
        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        /// <value>The fields.</value>
        public List<EnumFieldDefinition> Fields { get; set; } = new List<EnumFieldDefinition>();

        /// <summary>
        /// Gets or sets the size of the underlying type.
        /// </summary>
        /// <value>The size of the underlying type.</value>
        [JsonIgnore]
        public int UnderlyingTypeSize => Size;

        /// <summary>
        /// Gets a value indicating whether this instance is flags.
        /// </summary>
        /// <value><c>true</c> if this instance is flags; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsFlags => Metas.GetMeta(MetaConstants.Bitflags) != null;

        /// <summary>
        /// Gets the name of the underlying type.
        /// </summary>
        /// <value>The name of the underlying type.</value>
        [JsonIgnore]
        public string UnderlyingTypeName
        {
            get
            {
                switch(UnderlyingTypeSize)
                {
                    case sizeof(byte):
                        return "byte";
                    case sizeof(short):
                        return "ushort";
                    case sizeof(int):
                        return "uint";
                    case sizeof(System.UInt64):
                        return typeof(System.UInt64).FullName!;
                    default:
                        return "uint";
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumTypeDefinition"/> class.
        /// </summary>
        public EnumTypeDefinition()
        {
            Type = EDefinitionType.Enum;
        }
    }
}
