using Mono.Cecil;
using UnrealSharp.Utils.Misc;

namespace UnrealSharpTool.Core.TypeInfo.MonoCecil;

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
    private AssemblyResolver? _resolver;

    /// <summary>
    /// The DLL reader parameters
    /// </summary>
    internal readonly ReaderParameters DllReaderParameters;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyLoader"/> class.
    /// </summary>
    /// <param name="searchPaths">The search paths.</param>
    /// <param name="applyWindowsRuntimeProjections">if set to <c>true</c> [apply windows runtime projections].</param>
    public AssemblyLoader(IEnumerable<string> searchPaths, bool applyWindowsRuntimeProjections = false)
    {
        _resolver = new AssemblyResolver(this);
        foreach (var searchDirectory in searchPaths)
        {
            if (Directory.Exists(searchDirectory))
            {
                _resolver.AddSearchDirectory(searchDirectory);
            }
            else
            {
                Logger.LogWarning("Assembly search directory is not exists:{0}", searchDirectory);
            }
        }

        DllReaderParameters = new ReaderParameters
        {
            AssemblyResolver = _resolver,
            ReadSymbols = false,
            SymbolReaderProvider = null,
            ApplyWindowsRuntimeProjections = applyWindowsRuntimeProjections,
            ReadingMode = ReadingMode.Deferred
        };
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
            var assembly = AssemblyDefinition.ReadAssembly(path, DllReaderParameters);
                
            _resolver!.CacheAssembly(assembly);

            return assembly;
        }

        return Resolve(new AssemblyNameReference(path, null));
    }

    /// <summary>
    /// Gets the reference.
    /// </summary>
    /// <param name="scope">The scope.</param>
    /// <returns>AssemblyNameReference.</returns>
    private static AssemblyNameReference GetReference(IMetadataScope scope)
    {
        if (scope is ModuleDefinition moduleDefinition)
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
            return _resolver!.Resolve(reference, DllReaderParameters);
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
        if (_resolver != null)
        {
            _resolver.Dispose();
            _resolver = null;
        }
    }
}