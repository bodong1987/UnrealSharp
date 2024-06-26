using Microsoft.CodeAnalysis;
using UnrealSharp.Utils.Extensions.IO;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Core.TypeInfo.MonoCecil
{
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
            var assemblySearchOptions = globalOptions as AssemblySearchOptions;

            if (assemblySearchOptions == null)
            {
                Logger.LogError("Create TypeDefinitionDocument from Assembly need an valid global options and it's type must be AssemblySearchOptions");
                return null;
            }

            Logger.Ensure<Exception>(assemblySearchOptions.UnrealProjectDirectory.IsDirectoryExists(), "Unreal project configuration must be exists.");

            TypeDefinitionDocument document = new TypeDefinitionDocument();

            List<string> SearchPathes =
            [
                .. assemblySearchOptions.GetConfigurationBasedRuntimeSearchPaths(),
                .. inputSources.Select(x => Path.GetDirectoryName(x)!).Distinct(),
                .. assemblySearchOptions.CustomSearchDirectories
            ];

            AssemblyLoader assemblyLoader = new AssemblyLoader(SearchPathes);

            AssemblyAnalyseResult? MainResult = null;

            foreach (string assemblyPath in inputSources.Select(x => x))
            {
                AssemblyAnalyst assemblyAnalyst = new AssemblyAnalyst(assemblyLoader, assemblyPath, assemblySearchOptions);

                if (MainResult == null)
                {
                    MainResult = assemblyAnalyst.Result;
                }
                else
                {
                    MainResult.Merge(assemblyAnalyst.Result);
                }
            }

            if (MainResult != null)
            {
                MainResult.DebugInformation = debugInformation;

                foreach (var typeDefinition in MainResult.Definitions.Values)
                {
                    if(assemblySearchOptions.IgnoreBindingDefinition && 
                        (typeDefinition!.Namespace.Contains(".Bindings.Def.") || typeDefinition.IsCSharpBindingDefinitionType())
                        )
                    {
                        continue;
                    }

                    var type = MonoTypeDefinitionFactory.CreateTypeDefinition(typeDefinition!, MainResult);

                    if (type != null)
                    {
                        document.Types.Add(type);

                        // export a crc code for C# defined class, based on export string
                        type.CrcCode = (Int64)TypeDefinitionDocument.CalcTypeDefinitionCrcCode(type);
                    }
                }
            }

            return document;
        }
    }
}
