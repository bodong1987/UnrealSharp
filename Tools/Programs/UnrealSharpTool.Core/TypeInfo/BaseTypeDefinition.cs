using Newtonsoft.Json;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharpTool.Core.TypeInfo
{
    /// <summary>
    /// Enum EDefinitionType
    /// </summary>
    public enum EDefinitionType
    {
        /// <summary>
        /// The none
        /// </summary>
        None,
        /// <summary>
        /// The enum
        /// </summary>
        Enum,
        /// <summary>
        /// The structure
        /// </summary>
        Struct,
        /// <summary>
        /// The class
        /// </summary>
        Class,
        /// <summary>
        /// The function
        /// </summary>
        Function,
        /// <summary>
        /// The interface
        /// </summary>
        Interface
    }

    /// <summary>
    /// Class BaseTypeDefinition.
    /// Base type definition for all unreal interop types
    /// </summary>
    public abstract class BaseTypeDefinition
    {
        #region Properties
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public EDefinitionType Type { get; protected set; } = EDefinitionType.None;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the CPP.
        /// </summary>
        /// <value>The name of the CPP.</value>
        public string? CppName { get; set; }

        /// <summary>
        /// Gets or sets the name of the path.
        /// </summary>
        /// <value>The name of the path.</value>
        public string? PathName { get; set; }

        /// <summary>
        /// Gets or sets the name of the package.
        /// </summary>
        /// <value>The name of the package.</value>
        public string? PackageName { get; set; }

        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        /// <value>The name of the project.</value>
        public string? ProjectName { get; set; }

        /// <summary>
        /// Gets or sets the name of the assembly.
        /// </summary>
        /// <value>The name of the assembly.</value>
        public string? AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the namespace.
        /// </summary>
        /// <value>The namespace.</value>
        public string? Namespace { get; set; }

        /// <summary>
        /// Gets or sets the full name of the c sharp.
        /// </summary>
        /// <value>The full name of the c sharp.</value>
        public string? CSharpFullName { get; set; }

        #region Flags
        /// <summary>
        /// The flags core
        /// </summary>
        [JsonIgnore]
        private UInt64 FlagsCore;

        /// <summary>
        /// Gets or sets the flags.
        /// Because Unreal Json has precision issues when dealing with large integers, string is forced to be used to store large integers here.
        /// </summary>
        /// <value>The flags.</value>
        [JsonIgnore]
        public UInt64 Flags
        {
            get
            {
                return FlagsCore;
            }
            set
            {
                FlagsCore = value;
            }
        }

        /// <summary>
        /// Gets or sets the flags text
        /// Because Unreal Json has precision issues when dealing with large integers, string is forced to be used to store large integers here.
        /// </summary>
        /// <value>The flags t.</value>
        public string FlagsT
        {
            get => FlagsCore.ToString();
            set
            {
                FlagsCore = UInt64.Parse(value);
            }
        }

        /// <summary>
        /// The CRC code core
        /// </summary>
        [JsonIgnore]
        private Int64 CrcCodeCore;

        /// <summary>
        /// Gets or sets the CRC code.
        /// Because Unreal Json has precision issues when dealing with large integers, string is forced to be used to store large integers here.
        /// </summary>
        /// <value>The CRC code.</value>
        [JsonIgnore]
        public Int64 CrcCode 
        {
            get => CrcCodeCore;
            set
            {
                CrcCodeCore = value;
            }
        }

        /// <summary>
        /// Gets or sets the CRC code text.
        /// Because Unreal Json has precision issues when dealing with large integers, string is forced to be used to store large integers here.
        /// </summary>
        /// <value>The CRC code t.</value>
        public string CrcCodeT
        {
            get => CrcCode.ToString();
            set
            {
                CrcCodeCore = Int64.Parse(value);
            }
        }

        /// <summary>
        /// Gets or sets the export flags.
        /// </summary>
        /// <value>The export flags.</value>
        public int ExportFlags { get; set; }

        /// <summary>
        /// Gets the binding export flags.
        /// </summary>
        /// <value>The binding export flags.</value>
        [JsonIgnore]
        public EBindingExportFlags BindingExportFlags => (EBindingExportFlags)ExportFlags;
        #endregion

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public string? Guid { get; set; } = System.Guid.Empty.ToString();

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size.</value>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the metas.
        /// </summary>
        /// <value>The metas.</value>
        public MetaDefinition Metas { get; set; } = new MetaDefinition();

        /// <summary>
        /// Gets a value indicating whether this instance is enum.
        /// </summary>
        /// <value><c>true</c> if this instance is enum; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsEnum => Type == EDefinitionType.Enum;

        /// <summary>
        /// Gets a value indicating whether this instance is structure.
        /// </summary>
        /// <value><c>true</c> if this instance is structure; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsStruct => Type == EDefinitionType.Struct;

        /// <summary>
        /// Gets a value indicating whether this instance is class.
        /// </summary>
        /// <value><c>true</c> if this instance is class; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsClass => Type == EDefinitionType.Class;

        /// <summary>
        /// Gets a value indicating whether this instance is function.
        /// </summary>
        /// <value><c>true</c> if this instance is function; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsFunction => Type == EDefinitionType.Function;

        /// <summary>
        /// Gets a value indicating whether this instance is interface.
        /// </summary>
        /// <value><c>true</c> if this instance is interface; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsInterface => Type == EDefinitionType.Interface;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTypeDefinition"/> class.
        /// </summary>
        public BaseTypeDefinition()
        {
            Name = string.Empty;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string? ToString()
        {
            return PathName;
        }

        /// <summary>
        /// Converts the name of the CPP name to script.
        /// </summary>
        /// <param name="cppName">Name of the CPP.</param>
        /// <returns>System.String.</returns>
        public virtual string ConvertCppNameToScriptName(string cppName)
        {
            return cppName;
        }

        /// <summary>
        /// Gets the field comment.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        public string? GetFieldComment(string fieldName)
        {
            return Metas.GetMeta($"{fieldName}.Comment");
        }

        /// <summary>
        /// Gets the field tooltip.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        public string? GetFieldTooltip(string fieldName)
        {
            return Metas.GetMeta($"{fieldName}.ToolTip");
        }

        /// <summary>
        /// Gets the display name of the field.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        public string? GetFieldDisplayName(string fieldName)
        {
            return Metas.GetMeta($"{fieldName}.DisplayName");
        }

        /// <summary>
        /// Gets the comment.
        /// </summary>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        public string? GetComment()
        {
            return Metas.GetMeta(MetaConstants.Comment);
        }

        /// <summary>
        /// Gets the tooltip.
        /// </summary>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        public string? GetTooltip()
        {
            return Metas.GetMeta(MetaConstants.ToolTip);
        }

        /// <summary>
        /// Gets the meta.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        public string? GetMeta(string key)
        {
            return Metas.GetMeta(key);
        }
        #endregion
    }
}
