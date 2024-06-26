using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;
using UnrealSharp.Utils.CommandLine;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Extensions.IO;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.TypeInfo.Roslyn;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Core.TypeInfo
{
    #region Input Options
    /// <summary>
    /// Interface ITypeDefinitionDocumentInitializeOptions
    /// </summary>
    public interface ITypeDefinitionDocumentInitializeOptions
    {
    }

    /// <summary>
    /// Enum BindingCodeGenerateSourceType
    /// </summary>
    public enum BindingCodeGenerateSourceType
    {
        /// <summary>
        /// The json document
        /// </summary>
        JsonDoc,

        /// <summary>
        /// The assembly
        /// </summary>
        Assembly,
       
        /// <summary>
        /// The c sharp codes
        /// </summary>
        CSharpCode
    }
    #endregion

    #region C# Codes Source Options
    /// <summary>
    /// Class CSharpCodeBasedGenerateOptions.
    /// Implements the <see cref="UnrealSharpTool.Core.TypeInfo.ITypeDefinitionDocumentInitializeOptions" />
    /// </summary>
    /// <seealso cref="UnrealSharpTool.Core.TypeInfo.ITypeDefinitionDocumentInitializeOptions" />
    public class CSharpCodeBasedGenerateOptions : ITypeDefinitionDocumentInitializeOptions
    {
        /// <summary>
        /// Gets or sets the project directory.
        /// </summary>
        /// <value>The project directory.</value>
        [Option('p', "project", Required = true, HelpText = "Your unreal project directory path.")]
        public string UnrealProjectDirectory { get; set; } = string.Empty;
    }

    #endregion

    #region Json Doc
    /// <summary>
    /// Class JsonDocBasedGenerateOptions.
    /// Implements the <see cref="UnrealSharpTool.Core.TypeInfo.ITypeDefinitionDocumentInitializeOptions" />
    /// </summary>
    /// <seealso cref="UnrealSharpTool.Core.TypeInfo.ITypeDefinitionDocumentInitializeOptions" />
    public class JsonDocBasedGenerateOptions : ITypeDefinitionDocumentInitializeOptions
    {
    }

    #endregion

    /// <summary>
    /// Enum ETypeDefinitionDocumentAttributes
    /// </summary>
    [Flags]
    public enum ETypeDefinitionDocumentAttributes
    {
        /// <summary>
        /// The none
        /// </summary>
        None,

        /// <summary>
        /// The allow fast invoke generation
        /// </summary>
        AllowFastInvokeGeneration = 1 << 0
    };

    /// <summary>
    /// Class TypeDefinitionDocument.
    /// Represents a series of type information documents
    /// </summary>
    public class TypeDefinitionDocument
    {
        #region Properties
        /// <summary>
        /// Gets or sets the unreal major version.
        /// </summary>
        /// <value>The unreal major version.</value>
        public int UnrealMajorVersion { get; set; }

        /// <summary>
        /// Gets or sets the unreal minor version.
        /// </summary>
        /// <value>The unreal minor version.</value>
        public int UnrealMinorVersion { get; set; }

        /// <summary>
        /// Gets or sets the unreal patch version.
        /// </summary>
        /// <value>The unreal patch version.</value>
        public int UnrealPatchVersion { get; set; }

        /// <summary>
        /// Gets or sets the document attributes.
        /// </summary>
        /// <value>The document attributes.</value>
        public int DocumentAttributes { get; set; }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <value>The attributes.</value>
        [JsonIgnore]
        public ETypeDefinitionDocumentAttributes Attributes => (ETypeDefinitionDocumentAttributes)DocumentAttributes;

        /// <summary>
        /// Gets or sets the types.
        /// All Types
        /// </summary>
        /// <value>The types.</value>
        public List<BaseTypeDefinition> Types { get; set; } = new List<BaseTypeDefinition>();

        /// <summary>
        /// Gets or sets the fast access structure types.
        /// </summary>
        /// <value>The fast access structure types.</value>
        public HashSet<string> FastAccessStructTypes { get; set; } = new HashSet<string>();

        /// <summary>
        /// Gets or sets the fast access export modules.
        /// </summary>
        /// <value>The fast access export modules.</value>
        public HashSet<string> FastFunctionInvokeModuleNames { get; set; } = new HashSet<string>();

        /// <summary>
        /// Gets or sets the fast access ignore classes.
        /// </summary>
        /// <value>The fast access ignore classes.</value>
        public HashSet<string> FastFunctionInvokeIgnoreClassNames { get; set; } = new HashSet<string>();

        /// <summary>
        /// Gets or sets the fast access ignore methods.
        /// </summary>
        /// <value>The fast access ignore methods.</value>
        public HashSet<string> FastFunctionInvokeIgnoreNames { get; set; } = new HashSet<string>();

        /// <summary>
        /// The name mapping
        /// For quick search, the Key is CppName. Therefore all type C++ names should be unique
        /// </summary>
        [JsonIgnore]
        public readonly Dictionary<string, BaseTypeDefinition> NameMapping = new Dictionary<string, BaseTypeDefinition>();

        /// <summary>
        /// Gets a value indicating whether this instance has engine version information.
        /// </summary>
        /// <value><c>true</c> if this instance has engine version information; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool HasEngineVersionInfo => UnrealMajorVersion > 0;
        
        /// <summary>
        /// Gets unreal engine version string
        /// </summary>
        [JsonIgnore] 
        public string EngineVersion => $"{UnrealMajorVersion}.{UnrealMinorVersion}.{UnrealPatchVersion}";

        /// <summary>
        /// Gets the unreal engine version.
        /// </summary>
        /// <value>The unreal engine version.</value>
        [JsonIgnore]
        public Version UnrealEngineVersion => new Version(UnrealMajorVersion, UnrealMinorVersion, UnrealPatchVersion);
        #endregion

        #region Base Methods
        /// <summary>
        /// Adds the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        public void Add(BaseTypeDefinition type)
        {
            Types.Add(type);
            NameMapping.Add(type.CppName!, type);
        }

        /// <summary>
        /// Determines whether [is fast access structure type] [the specified structure CPP name].
        /// </summary>
        /// <param name="structCppName">Name of the structure CPP.</param>
        /// <returns><c>true</c> if [is fast access structure type] [the specified structure CPP name]; otherwise, <c>false</c>.</returns>
        public bool IsFastAccessStructType(string structCppName)
        {
            return FastAccessStructTypes.Contains(structCppName);
        }

        /// <summary>
        /// Determines whether [is fast access module] [the specified module name].
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <returns><c>true</c> if [is fast access module] [the specified module name]; otherwise, <c>false</c>.</returns>
        public bool IsModuleFastInvokeSupported(string moduleName)
        {
            return FastFunctionInvokeModuleNames.Contains(moduleName);
        }

        /// <summary>
        /// should ignore this method
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns><c>true</c> if [is fast access ignore method] [the specified class name]; otherwise, <c>false</c>.</returns>
        public bool IsFastInvokeIgnore(string className, string methodName)
        {
            if(FastFunctionInvokeIgnoreClassNames.Contains(className))
            {
                return true;
            }

            string key = $"{className}::{methodName}";

            if(FastFunctionInvokeIgnoreNames.Contains(key))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            Types.Clear();
            NameMapping.Clear();
            FastAccessStructTypes.Clear();
            FastFunctionInvokeIgnoreNames.Clear();
            FastFunctionInvokeIgnoreClassNames.Clear();
            DocumentAttributes = 0;
        }

        /// <summary>
        /// Find type definition information by C++ name
        /// </summary>
        /// <param name="cppName">Name of the CPP.</param>
        /// <returns>System.Nullable&lt;BaseTypeDefinition&gt;.</returns>
        public BaseTypeDefinition? GetDefinition(string cppName)
        {
            NameMapping.TryGetValue(cppName, out BaseTypeDefinition? type);
            return type;
        }


        /// <summary>
        /// Calculates the type definition CRC code.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <returns>UInt64.</returns>
        public static UInt64 CalcTypeDefinitionCrcCode(BaseTypeDefinition typeDefinition)
        {
            string jsonString = JsonConvert.SerializeObject(typeDefinition, Formatting.Indented, GetDefaultJsonSerializerSettings());

            return jsonString.GetDeterministicHashCode64();
        }
        #endregion

        #region Json Serialization
        /// <summary>
        /// Class CustomOrderContractResolver.
        /// Implements the <see cref="DefaultContractResolver" />
        /// </summary>
        /// <seealso cref="DefaultContractResolver" />
        class CustomOrderContractResolver : DefaultContractResolver
        {
            /// <summary>
            /// Creates the properties.
            /// </summary>
            /// <param name="type">The type.</param>
            /// <param name="memberSerialization">The member serialization.</param>
            /// <returns>System.Collections.Generic.IList&lt;Newtonsoft.Json.Serialization.JsonProperty&gt;.</returns>
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                Dictionary<Type, int> TypeValues = new Dictionary<Type, int>();
                int CurrentValue = 0;
                TypeValues.Add(type, CurrentValue);

                for (Type? baseType = type.BaseType; baseType != null && baseType != typeof(Object); baseType = baseType.BaseType)
                {
                    TypeValues.Add(baseType, --CurrentValue);
                }

                IList<JsonProperty> defaultProperties = base.CreateProperties(type, memberSerialization);

                IList<JsonProperty> sortedProperties = defaultProperties.OrderBy(p => TypeValues[p.DeclaringType!]).ToList();

                return sortedProperties;
            }
        }

        /// <summary>
        /// Gets the default json serializer settings.
        /// </summary>
        /// <returns>Newtonsoft.Json.JsonSerializerSettings.</returns>
        private static JsonSerializerSettings GetDefaultJsonSerializerSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CustomOrderContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new[]
                {
                    new MetaDefinitionConverter()
                }
            };

            return settings;
        }
        #endregion

        #region Json Support
        /// <summary>
        /// Saves to file.
        /// </summary>
        /// <param name="path">The path.</param>
        public void SaveToFile(string path)
        {
            string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented, GetDefaultJsonSerializerSettings());

            string dir = path.GetDirectoryPath();

            if(!dir.IsDirectoryExists())
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllBytes(path, Encoding.UTF8.GetBytes(jsonString));

            Logger.Log("Save file success :{0}", path.CanonicalPath());
        }

        /// <summary>
        /// Loads from file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>bool.</returns>
        public bool LoadFromFile(string path)
        {
            Reset();

            if(!File.Exists(path))
            {
                Logger.LogError("{0} is not exists.", path);
                return false;
            }

            string jsonString = File.ReadAllText(path, Encoding.UTF8);
            JsonConvert.PopulateObject(jsonString, this, GetDefaultJsonSerializerSettings());

            foreach(var type in Types)
            {
                // Logger.Log($"read {type.Type} {type.CppName} {type.PathName} ");

                if(type is StructTypeDefinition structType)
                {
                    foreach(var property in structType.Properties)
                    {
                        property.Parent = structType;
                    }
                }

                if(type is ClassTypeDefinition classType)
                {
                    foreach(var function in classType.Functions)
                    {
                        function.Parent = classType;

                        foreach(var property in function.Properties)
                        {
                            property.Parent = function;
                        }
                    }
                }

                NameMapping.Add(type.CppName!, type);
            }

            return true;
        }
        #endregion

        #region Factory        
        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <param name="bindingCodeGenerateSourceType">Type of the binding code generate source.</param>
        /// <param name="inputSource">The input source.</param>
        /// <param name="globalOptions">The global options.</param>
        /// <param name="sourceFiles">The source files.</param>
        /// <returns>UnrealSharpTool.Core.TypeInfo.TypeDefinitionDocument?.</returns>
        public static TypeDefinitionDocument? CreateDocument(
            BindingCodeGenerateSourceType bindingCodeGenerateSourceType, 
            string inputSource, 
            ITypeDefinitionDocumentInitializeOptions? globalOptions,
            IEnumerable<string> sourceFiles
            )
        {
            RoslynDebugInformation roslynDebugInformation = new RoslynDebugInformation(sourceFiles);

            TypeDefinitionDocument? document = null;

            if(bindingCodeGenerateSourceType == BindingCodeGenerateSourceType.CSharpCode)
            {
                return TypeInfo.Roslyn.RoslynTypeDefinitionDocumentFactory.ParseFromDirectory(inputSource, globalOptions, roslynDebugInformation);
            }
            else if(bindingCodeGenerateSourceType == BindingCodeGenerateSourceType.Assembly)
            {
                return MonoCecil.MonoTypeDefinitionDocumentFactory.LoadFromAssemblies(

                    [inputSource], 
                    globalOptions, 
                    roslynDebugInformation
                    );
            }

            document = new TypeDefinitionDocument();

            if (!document.LoadFromFile(inputSource))
            {
                return null;
            }

            return document;
        }
#endregion
    }
}
