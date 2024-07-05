using Mono.Cecil;
using UnrealSharp.Utils.Extensions.IO;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.ErrorReports;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Core.TypeInfo.MonoCecil;

/// <summary>
/// Interface ITypeValidator
/// Used to identify a type
/// </summary>
internal interface IMonoTypeValidator : ITypeValidator
{        
    /// <summary>
    /// Is it a valid type definition?
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    bool IsTypeDefinition(TypeDefinition typeDefinition);
             
    /// <summary>
    /// is a valid placeholder definition
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    bool IsPlaceholderType(TypeDefinition typeDefinition);

    /// <summary>
    /// Is supported type ?
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <param name="blueprintable">if set to <c>true</c> [blueprintable].</param>
    /// <returns><c>true</c> if [is supported type] [the specified type reference]; otherwise, <c>false</c>.</returns>
    bool IsSupportedType(TypeReference typeReference, bool blueprintable);

    /// <summary>
    /// Gets the debug information.
    /// </summary>
    /// <value>The debug information.</value>
    IDebugInformation? DebugInformation { get; }
}

/// <summary>
/// Class AssemblyAnalyseResult.
/// </summary>
internal class AssemblyAnalyseResult : IMonoTypeValidator
{
    /// <summary>
    /// The assembly
    /// </summary>
    public readonly AssemblyDefinition? Assembly;

    /// <summary>
    /// The definitions
    /// </summary>
    public readonly SortedList<string, TypeDefinition?> Definitions = new();

    /// <summary>
    /// The placeholder definitions
    /// </summary>
    public readonly SortedList<string, TypeDefinition?> PlaceHolderDefinitions = new();

    /// <summary>
    /// Gets the debug information.
    /// </summary>
    /// <value>The debug information.</value>
    public IDebugInformation? DebugInformation { get; internal set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyAnalyseResult"/> class.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    public AssemblyAnalyseResult(AssemblyDefinition assembly)
    {
        Assembly = assembly;
    }

    /// <summary>
    /// Merges the specified other.
    /// </summary>
    /// <param name="other">The other.</param>
    public void Merge(AssemblyAnalyseResult other)
    {
        foreach (var i in other.Definitions.Where(i => !Definitions.ContainsKey(i.Key)))
        {
            Definitions.Add(i.Key, i.Value);
        }

        foreach (var i in other.PlaceHolderDefinitions.Where(i => !PlaceHolderDefinitions.ContainsKey(i.Key)))
        {
            PlaceHolderDefinitions.Add(i.Key, i.Value);
        }
    }

    /// <summary>
    /// Is it a valid type definition?
    /// </summary>
    /// <param name="name">The name.</param>
    public bool IsTypeDefinition(string name)
    {
        return Definitions.ContainsKey(name);
    }

    /// <summary>
    /// Is it a valid type definition?
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    public bool IsTypeDefinition(TypeDefinition typeDefinition)
    {
        return Definitions.ContainsKey(typeDefinition.Name);
    }

    /// <summary>
    /// is a valid placeholder definition
    /// </summary>
    /// <param name="name">The name.</param>
    public bool IsPlaceholderType(string name)
    {
        return PlaceHolderDefinitions.ContainsKey(name);
    }

    /// <summary>
    /// is a valid placeholder definition
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    public bool IsPlaceholderType(TypeDefinition typeDefinition)
    {
        return PlaceHolderDefinitions.ContainsKey(typeDefinition.Name);
    }

    /// <summary>
    /// Is supported type ?
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <param name="blueprintable"></param>
    public bool IsSupportedType(TypeReference typeReference, bool blueprintable)
    {
        return typeReference.IsSupportedType(blueprintable);
    }

    public IEnumerable<string> GetDefinitions()
    {
        return Definitions.Keys;
    }

    public IEnumerable<string> GetPlaceholders()
    {
        return PlaceHolderDefinitions.Keys;
    }

    public void Merge(ITypeValidator otherValidator)
    {
        if(otherValidator is AssemblyAnalyseResult result)
        {
            Merge(result);
            return;
        }

        foreach (var i in otherValidator.GetDefinitions())
        {
            Definitions.TryAdd(i, null);
        }

        foreach (var i in otherValidator.GetPlaceholders())
        {
            PlaceHolderDefinitions.TryAdd(i, null);
        }
    }
}


/// <summary>
/// Class AssemblyAnalyst.
/// </summary>
internal class AssemblyAnalyst
{
    /// <summary>
    /// The loader
    /// </summary>
    public readonly AssemblyLoader Loader;
    /// <summary>
    /// The result
    /// </summary>
    public readonly AssemblyAnalyseResult Result;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyAnalyst" /> class.
    /// </summary>
    /// <param name="assemblyLoader">The assembly loader.</param>
    /// <param name="assemblyPath">The assembly path.</param>
    /// <param name="options">The options.</param>
    public AssemblyAnalyst(AssemblyLoader assemblyLoader, string assemblyPath, AssemblySearchOptions options)
    {
        Loader = assemblyLoader;

        Logger.Ensure<FileNotFoundException>(assemblyPath.IsFileExists(), $"{assemblyPath} does not exist");
        var assembly = Loader.Load(assemblyPath);

        Logger.EnsureNotNull(assembly, $"Failed load {assemblyPath}");

        Result = new AssemblyAnalyseResult(assembly);

        Analysis(options);
    }

    /// <summary>
    /// Analysis this instance.
    /// </summary>
    private void Analysis(AssemblySearchOptions options)
    {
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var module in Result.Assembly!.Modules)
        {
            foreach (var type in module.Types.Where(type => !options.IgnoreBindingDefinition ||
                                                            (!type!.Namespace.Contains(".Bindings.Defs") && !type.IsCSharpBindingDefinitionType())))
            {
                if (CheckPlaceholderType(type))
                {
                    Result.PlaceHolderDefinitions.Add(type.Name, type);
                }
                else if (CheckExportType(type))
                {
                    if(Result.Definitions.TryGetValue(type.Name, out var existsType))
                    {
                        var errorMessage = $"There is a type conflict. Exported types are not allowed to have the same name even if they are in different namespaces.{Environment.NewLine}  {existsType!} {Environment.NewLine}  {type}";

                        Logger.LogError(errorMessage);
                        throw new UnrealSharpDefinitionException(errorMessage);
                    }
                    Result.Definitions.Add(type.Name, type);
                }
            }
        }
    }

    /// <summary>
    /// Checks the type of the placeholder.
    /// </summary>
    /// <param name="typeDef">The type definition.</param>
    /// <returns><c>true</c> if target type is a placeholder type, <c>false</c> otherwise.</returns>
    private static bool CheckPlaceholderType(TypeDefinition typeDef)
    {
        return typeDef.CustomAttributes.Any(attr => 
            attr.IsCSharpBindingDefinitionTypeAttribute() || 
            attr.IsCSharpNativeBindingTypeAttribute() || 
            attr.IsCSharpBlueprintBindingTypeAttribute());
    }

    /// <summary>
    /// Checks the type of the export.
    /// </summary>
    /// <param name="typeDef">The type definition.</param>
    /// <returns><c>true</c> if target type is an export type, <c>false</c> otherwise.</returns>
    private static bool CheckExportType(TypeDefinition typeDef)
    {
        return typeDef.IsCSharpImplementType();
    }
}