using Newtonsoft.Json;

namespace UnrealSharpTool.Core.TypeInfo
{
    /// <summary>
    /// Class ClassTypeDefinition.
    /// Implements the <see cref="UnrealSharpTool.Core.TypeInfo.StructTypeDefinition" />
    /// </summary>
    /// <seealso cref="UnrealSharpTool.Core.TypeInfo.StructTypeDefinition" />
    public class ClassTypeDefinition : StructTypeDefinition
    {
        /// <summary>
        /// Super class name, should be CPPName
        /// </summary>
        /// <value>The name of the super.</value>
        public string SuperName { get; set; } = "UObject";

        /// <summary>
        /// Gets or sets the name of the configuration.
        /// </summary>
        /// <value>The name of the configuration.</value>
        public string? ConfigName { get; set; } = "";

        /// <summary>
        /// Gets a value indicating whether this instance is native binding super type.
        /// </summary>
        /// <value><c>true</c> if this instance is native binding super type; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsNativeBindingSuperType { get; internal set; }

        /// <summary>
        /// Gets or sets the functions.
        /// </summary>
        /// <value>The functions.</value>
        public List<FunctionTypeDefinition> Functions { get; set; } = new List<FunctionTypeDefinition>();

        /// <summary>
        /// Gets or sets the interfaces.
        /// </summary>
        /// <value>The interfaces.</value>
        public List<string> Interfaces { get; set; } = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassTypeDefinition"/> class.
        /// </summary>
        public ClassTypeDefinition()
        {
            Type = EDefinitionType.Class;
        }

        /// <summary>
        /// Converts the name of the CPP name to script.
        /// </summary>
        /// <param name="cppName">Name of the CPP.</param>
        /// <returns>System.String.</returns>
        public override string ConvertCppNameToScriptName(string cppName)
        {
            if(cppName.StartsWith('A') ||
                cppName.StartsWith('U'))
            {
                return cppName.Substring(1);
            }

            return cppName;
        }
    }
}
