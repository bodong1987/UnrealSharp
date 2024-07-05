using System.Diagnostics.CodeAnalysis;
using UnrealSharp.Utils.Extensions.IO;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen;

/// <summary>
/// Enum EBindingSchemaType
/// </summary>
public enum EBindingSchemaType
{
    /// <summary>
    /// The native binding
    /// Export C# code binding for Unreal native C++ Classes
    /// </summary>
    NativeBinding,

    /// <summary>
    /// The blueprint binding
    /// Reserved, used to bind with Unreal Blueprint Classes
    /// </summary>
    BlueprintBinding,

    /// <summary>
    /// The C# binding
    /// Export C# code binding for C# generated BlueprintClasses
    /// </summary>
    CSharpBinding
}

/// <summary>
/// Enum ELocalUsageScenarioType
/// </summary>
[Flags]
public enum ELocalUsageScenarioType
{
    /// <summary>
    /// The common
    /// </summary>
    Common = 0,
    /// <summary>
    /// The class
    /// </summary>
    Class = 1 << 0,
    /// <summary>
    /// The property
    /// </summary>
    Property = 1 << 1,
    /// <summary>
    /// The structure
    /// </summary>
    Struct = 1 << 2,
    /// <summary>
    /// The structure view
    /// </summary>
    StructView = 1 << 3,
    /// <summary>
    /// The method
    /// </summary>
    Method = 1 << 4,
    /// <summary>
    /// The function
    /// </summary>
    Function = Method,
    /// <summary>
    /// The parameter
    /// </summary>
    Parameter = 1 << 5,
    /// <summary>
    /// The field
    /// </summary>
    Field = 1 << 6,
    /// <summary>
    /// The return value
    /// </summary>
    ReturnValue = 1 << 7,
    /// <summary>
    /// The interface
    /// </summary>
    Interface = 1 << 8,
    /// <summary>
    /// The delegate
    /// </summary>
    Delegate = 1 << 9,
    /// <summary>
    /// The generic argument
    /// </summary>
    GenericArgument = 1 << 10,
    /// <summary>
    /// The property getter
    /// </summary>
    PropertyGetter = 1 << 11,
    /// <summary>
    /// The property setter
    /// </summary>
    PropertySetter = 1 << 12
}


/// <summary>
/// Class BindingContext.
/// </summary>
public class BindingContext
{
    /// <summary>
    /// The document
    /// </summary>
    public readonly TypeDefinitionDocument Document;

    /// <summary>
    /// your unreal project directory
    /// </summary>
    public readonly string UnrealProjectDirectory;

    /// <summary>
    /// The project name
    /// </summary>
    public readonly string UnrealProjectName;

    /// <summary>
    /// The schema type
    /// </summary>
    public readonly EBindingSchemaType SchemaType;

    /// <summary>
    /// The importer
    /// </summary>
    private readonly PropertyProcessorImporter _importer;

    /// <summary>
    /// Gets the types.
    /// </summary>
    /// <value>The types.</value>
    public IEnumerable<BaseTypeDefinition> Types => Document.Types;

    /// <summary>
    /// The project type mapping
    /// </summary>
    public readonly Dictionary<string, List<BaseTypeDefinition>> ProjectTypeMapping = new();

    /// <summary>
    /// The fast invoke functions
    /// </summary>
    public readonly HashSet<FunctionTypeDefinition> FastInvokeFunctions = [];

    /// <summary>
    /// Gets a value indicating whether this instance is native binding.
    /// </summary>
    /// <value><c>true</c> if this instance is native binding; otherwise, <c>false</c>.</value>
    public bool IsNativeBinding => SchemaType == EBindingSchemaType.NativeBinding;

    /// <summary>
    /// Gets a value indicating whether this instance is script binding.
    /// </summary>
    /// <value><c>true</c> if this instance is script binding; otherwise, <c>false</c>.</value>
    public bool IsCSharpBinding => SchemaType == EBindingSchemaType.CSharpBinding;

    /// <summary>
    /// Gets a value indicating whether this instance is blueprint binding.
    /// </summary>
    /// <value><c>true</c> if this instance is blueprint binding; otherwise, <c>false</c>.</value>
    public bool IsBlueprintBinding => SchemaType == EBindingSchemaType.BlueprintBinding;

    /// <summary>
    /// Gets a value indicating whether this instance is binding to unreal implement.
    /// </summary>
    /// <value><c>true</c> if this instance is binding to unreal implement; otherwise, <c>false</c>.</value>
    public bool IsBindingToUnrealImplement => IsNativeBinding || IsBlueprintBinding;

    /// <summary>
    /// Gets the unreal version.
    /// </summary>
    /// <value>The unreal version.</value>
    public Version UnrealVersion { get; }

    /// <summary>
    /// Gets a value indicating whether this instance has version information.
    /// </summary>
    /// <value><c>true</c> if this instance has version information; otherwise, <c>false</c>.</value>
    public bool HasVersionInfo => UnrealVersion.MajorRevision > 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="BindingContext" /> class.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="projectDirectory">The project directory.</param>
    /// <param name="schemaType">Type of the schema.</param>
    [RequiresDynamicCode("Calls UnrealSharpTool.Core.CodeGen.PropertyProcessorImporter.PropertyProcessorImporter(BindingContext)")]
    private BindingContext(TypeDefinitionDocument document, string projectDirectory, EBindingSchemaType schemaType)
    {
        Document = document;
        UnrealProjectDirectory = Path.GetFullPath(projectDirectory);
        UnrealProjectName = UnrealProjectDirectory.Trim('\\', '/').GetFileName();
        SchemaType = schemaType;
        UnrealVersion = new Version(document.UnrealMajorVersion, document.UnrealMinorVersion, document.UnrealPatchVersion);

        foreach (var type in document.Types)
        {
            var projectName = type.ProjectName!;

            if (!ProjectTypeMapping.TryGetValue(projectName, out var definitions))
            {
                definitions = new List<BaseTypeDefinition>();

                ProjectTypeMapping.Add(projectName, definitions);
            }

            definitions.Add(type);
        }

        _importer = new PropertyProcessorImporter(this);
    }

    /// <summary>
    /// Gets the processor.
    /// </summary>
    /// <param name="propertyDefinition">The property definition.</param>
    /// <returns>System.Nullable&lt;PropertyProcessor&gt;.</returns>
    public PropertyProcessor? GetProcessor(PropertyDefinition propertyDefinition)
    {
        return _importer.GetProcessor(propertyDefinition);
    }

    /// <summary>
    /// Finds the type.
    /// </summary>
    /// <param name="cppName">Name of the CPP.</param>
    /// <returns>System.Nullable&lt;BaseTypeDefinition&gt;.</returns>
    public BaseTypeDefinition? FindType(string cppName)
    {
        return Document.GetDefinition(cppName);
    }


    /// <summary>
    /// Determines whether [is fast invoke function] [the specified function type definition].
    /// </summary>
    /// <param name="functionTypeDefinition">The function type definition.</param>
    /// <returns><c>true</c> if [is fast invoke function] [the specified function type definition]; otherwise, <c>false</c>.</returns>
    public bool IsFastInvokeFunction(FunctionTypeDefinition functionTypeDefinition)
    {
        return FastInvokeFunctions.Contains(functionTypeDefinition);
    }

    /// <summary>
    /// Creates the specified document.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="projectDirectory">The project directory.</param>
    /// <param name="schema">The schema.</param>
    /// <returns>BindingContext.</returns>
    [RequiresDynamicCode("new BindingContext")]
    public static BindingContext Create(TypeDefinitionDocument document, string projectDirectory, EBindingSchemaType schema)
    {
        return new BindingContext(document, projectDirectory, schema);
    }
}