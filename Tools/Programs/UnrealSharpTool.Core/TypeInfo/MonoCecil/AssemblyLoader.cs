using Mono.Cecil;
using UnrealSharp.Utils.Misc;

namespace UnrealSharpTool.Core.TypeInfo.MonoCecil
{
    /// <summary>
    /// Class AssemblyLoader.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class AssemblyLoader : IDisposable
    {
        /// <summary>
        /// The resolver
        /// </summary>
        private AssemblyResolver? Resolver;

        /// <summary>
        /// The DLL reader parameters
        /// </summary>
        internal ReaderParameters DllReaderParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyLoader"/> class.
        /// </summary>
        /// <param name="searchPathes">The search pathes.</param>
        /// <param name="applyWindowsRuntimeProjections">if set to <c>true</c> [apply windows runtime projections].</param>
        public AssemblyLoader(IEnumerable<string> searchPathes, bool applyWindowsRuntimeProjections = false)
        {
            Resolver = new AssemblyResolver(this);
            foreach (var searchDirectory in searchPathes)
            {
                if (Directory.Exists(searchDirectory))
                {
                    Resolver.AddSearchDirectory(searchDirectory);
                }
                else
                {
                    Logger.LogWarning("Assembly search directory is not exists:{0}", searchDirectory);
                }
            }

            DllReaderParameters = new ReaderParameters
            {
                AssemblyResolver = Resolver,
                ReadSymbols = false,
                SymbolReaderProvider = null,
                ApplyWindowsRuntimeProjections = applyWindowsRuntimeProjections,
                ReadingMode = ReadingMode.Deferred
            };
        }

        /// <summary>
        /// Chooses the reader parameters.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="loadSymbols">if set to <c>true</c> [load symbols].</param>
        /// <returns>ReaderParameters.</returns>
        private ReaderParameters ChooseReaderParameters(string path, bool loadSymbols)
        {
            return DllReaderParameters;
        }

        /// <summary>
        /// Loads the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>AssemblyDefinition.</returns>
        public AssemblyDefinition? Load(string path)
        {
            if (File.Exists(path))
            {
                AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(path, DllReaderParameters);
                
                Resolver!.CacheAssembly(assembly);

                return assembly;
            }

            return Resolve(new AssemblyNameReference(path, null));
        }

        /// <summary>
        /// Gets the reference.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <returns>AssemblyNameReference.</returns>
        static AssemblyNameReference GetReference(IMetadataScope scope)
        {
            var moduleDefinition = scope as ModuleDefinition;
            if (moduleDefinition != null)
            {
                return moduleDefinition.Assembly.Name;
            }
            return (AssemblyNameReference)scope;
        }

        /// <summary>
        /// Resolves the specified scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <returns>AssemblyDefinition.</returns>
        /// <exception cref="AssemblyResolutionException"></exception>
        public AssemblyDefinition? Resolve(IMetadataScope scope)
        {
            var reference = GetReference(scope);
            try
            {
                return Resolver!.Resolve(reference, DllReaderParameters);
            }
            catch
            {
                throw new AssemblyResolutionException(reference);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Resolver != null)
            {
                Resolver.Dispose();
                Resolver = null;
            }
        }
    }

}
