using Mono.Cecil;
using UnrealSharp.Utils.Extensions.IO;
using UnrealSharp.Utils.Misc;

namespace UnrealSharpTool.Core.TypeInfo.MonoCecil;

/// <summary>
/// Class DefaultAssemblyResolver.
/// Implements the <see cref="Mono.Cecil.IAssemblyResolver" />
/// </summary>
/// <seealso cref="Mono.Cecil.IAssemblyResolver" />
internal class AssemblyResolver :
    IAssemblyResolver
{
    /// <summary>
    /// The search directories
    /// </summary>
    public readonly List<string> SearchDirectories = [];

    /// <summary>
    /// The cached definitions
    /// </summary>
    private readonly Dictionary<string, AssemblyDefinition> _cachedDefinitions = new();

    /// <summary>
    /// Gets the loader.
    /// </summary>
    /// <value>The loader.</value>
    public AssemblyLoader Loader { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyResolver"/> class.
    /// </summary>
    /// <param name="loader">The loader.</param>
    public AssemblyResolver(AssemblyLoader loader)
    {
        Loader = loader;
    }

    /// <summary>
    /// Adds the search directory.
    /// </summary>
    /// <param name="directory">The directory.</param>
    public void AddSearchDirectory(string directory)
    {
        directory = Path.GetFullPath(directory);

        if (!SearchDirectories.Contains(directory))
        {
            SearchDirectories.Add(directory);
        }
    }

    /// <summary>
    /// Resolves the specified name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>AssemblyDefinition.</returns>
    public AssemblyDefinition? Resolve(AssemblyNameReference name)
    {
        if (_cachedDefinitions.TryGetValue(name.Name, out var definition))
        {
            return definition;
        }

        var path = SearchLibrary(name, SearchDirectories);
            
        if(path.IsFileExists())
        {
            var asm = Loader.Load(path);
            if (asm != null)
            {
                _cachedDefinitions.TryAdd(name.Name, asm);

                // Logger.LogD($"@info: locate {name.FullName} at {file}.");

                return asm;
            }
        }

        Logger.LogError($"@error: missing library {name.FullName}");
            
        return null;
    }

    private static string? SearchLibrary(AssemblyNameReference name, IEnumerable<string> directories)
    {
        var extensions = name.IsWindowsRuntime ? [".winmd", ".dll"] : new[] { ".exe", ".dll" };
        foreach (var directory in directories)
        {
            foreach (var extension in extensions)
            {
                var file = Path.Combine(directory, name.Name + extension);
                if (File.Exists(file))
                {
                    return file;
                }
            }

            // find the last one under managed child directories
            if(directory.EndsWith("Managed") && directory.IsDirectoryExists())
            {
                var latestWriteTime = DateTime.MinValue;
                string? latestFile = null;

                var childDirs = Directory.GetDirectories(directory);

                foreach (var extension in extensions)
                {
                    foreach (var subDir in childDirs)
                    {
                        var file = Path.Combine(subDir, name.Name + extension);
                        if (File.Exists(file))
                        {
                            var writeTime = File.GetLastWriteTime(file);
                            if (writeTime > latestWriteTime)
                            {
                                latestWriteTime = writeTime;
                                latestFile = file;
                            }
                        }
                    }
                }

                if(latestFile != null)
                {
                    return latestFile;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Resolves the specified name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns>AssemblyDefinition.</returns>
    public AssemblyDefinition? Resolve(AssemblyNameReference name, ReaderParameters parameters)
    {
        return Resolve(name);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _cachedDefinitions.Clear();
    }

    /// <summary>
    /// Caches the assembly.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    public void CacheAssembly(AssemblyDefinition assembly)
    {
        CacheAssembly(assembly.Name.Name, assembly);
    }

    /// <summary>
    /// Caches the assembly.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="assembly">The assembly.</param>
    /// <exception cref="Exception"></exception>
    public void CacheAssembly(string name, AssemblyDefinition assembly)
    {
        if (!_cachedDefinitions.TryAdd(name, assembly))
        {
            return;
        }

        if (assembly.MainModule.FileName != null)
        {
            SearchDirectories.Add(Path.GetDirectoryName(assembly.MainModule.FileName)!);
        }
    }
}