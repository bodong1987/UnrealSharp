using UnrealSharp.Utils.Extensions.IO;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Core.TypeInfo.MonoCecil;

/// <summary>
/// Class MonoTypeDefinitionDocumentFactory.
/// </summary>
internal static class MonoTypeDefinitionDocumentFactory
{
    /// <summary>
    /// Loads from assemblies.
    /// </summary>
    /// <param name="inputSources">The input sources.</param>
    /// <param name="globalOptions">The global options.</param>
    /// <param name="debugInformation">The debug information.</param>
    /// <returns>UnrealSharpTool.Core.TypeInfo.TypeDefinitionDocument.</returns>
    public static TypeDefinitionDocument? LoadFromAssemblies(
        IEnumerable<string> inputSources, 
        ITypeDefinitionDocumentInitializeOptions? globalOptions,
        IDebugInformation debugInformation
    )
    {
        if (globalOptions is not AssemblySearchOptions assemblySearchOptions)
        {
            Logger.LogError("Create TypeDefinitionDocument from Assembly need an valid global options and it's type must be AssemblySearchOptions");
            return null;
        }

        Logger.Ensure<Exception>(assemblySearchOptions.UnrealProjectDirectory.IsDirectoryExists(), "Unreal project configuration must be exists.");

        var document = new TypeDefinitionDocument();
            
        // avoid enumerate multiple times
        inputSources = inputSources.ToList();
            
        List<string> searchPaths =
        [
            .. assemblySearchOptions.GetConfigurationBasedRuntimeSearchPaths(),
            .. inputSources.Select(x => Path.GetDirectoryName(x)!).Distinct(),
            .. assemblySearchOptions.CustomSearchDirectories
        ];

        var assemblyLoader = new AssemblyLoader(searchPaths);

        AssemblyAnalyseResult? mainResult = null;

        foreach (var assemblyPath in inputSources.Select(x => x))
        {
            var assemblyAnalyst = new AssemblyAnalyst(assemblyLoader, assemblyPath, assemblySearchOptions);

            if (mainResult == null)
            {
                mainResult = assemblyAnalyst.Result;
            }
            else
            {
                mainResult.Merge(assemblyAnalyst.Result);
            }
        }

        if (mainResult != null)
        {
            mainResult.DebugInformation = debugInformation;

            foreach (var typeDefinition in mainResult.Definitions.Values)
            {
                if(assemblySearchOptions.IgnoreBindingDefinition && 
                   (typeDefinition!.Namespace.Contains(".Bindings.Def.") || typeDefinition.IsCSharpBindingDefinitionType())
                  )
                {
                    continue;
                }

                var type = MonoTypeDefinitionFactory.CreateTypeDefinition(typeDefinition!, mainResult);

                if (type != null)
                {
                    document.Types.Add(type);

                    // export a crc code for C# defined class, based on export string
                    type.CrcCode = (long)TypeDefinitionDocument.CalcTypeDefinitionCrcCode(type);
                }
            }
        }

        return document;
    }
}