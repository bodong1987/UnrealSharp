using UnrealSharp.Utils.Extensions.IO;
using UnrealSharp.Utils.Misc;

namespace UnrealSharpTool.Core.Utils;

/// <summary>
/// Class ScopedExporter.
/// Save the output information so that we can actively clean up some expired files after the output is completed.
/// Implements the <see cref="IDisposable" />
/// </summary>
/// <seealso cref="IDisposable" />
internal class ScopedExporter : IDisposable
{
    /// <summary>
    /// The unreal project directory
    /// </summary>
    public readonly string UnrealProjectDirectory;

    /// <summary>
    /// The export root directory
    /// </summary>
    public readonly string ExportRootDirectory;

    /// <summary>
    /// The exported files
    /// </summary>
    public readonly HashSet<string> ExportedFiles = [];

    /// <summary>
    /// The reserve filter function
    /// </summary>
    public readonly Func<string, bool>? ReserveFilterFunc;

    public List<string> Extensions = [".cs"];

    /// <summary>
    /// The on delete file delegate
    /// </summary>
    public Action<string>? OnDeleteFileDelegate;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScopedExporter"/> class.
    /// </summary>
    /// <param name="unrealProjectDirectory">The unreal project directory.</param>
    /// <param name="exportRootDirectory">export root directory</param>
    public ScopedExporter(string unrealProjectDirectory, string exportRootDirectory)
    {
        UnrealProjectDirectory = unrealProjectDirectory;
        ExportRootDirectory = exportRootDirectory;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScopedExporter"/> class.
    /// </summary>
    /// <param name="unrealProjectDirectory">The unreal project directory.</param>
    /// <param name="exportRootDirectory">The export root directory.</param>
    /// <param name="reserveFilterFunc">The reserve filter function.</param>
    public ScopedExporter(string unrealProjectDirectory, string exportRootDirectory, Func<string, bool> reserveFilterFunc) :
        this(unrealProjectDirectory, exportRootDirectory)
    {
        ReserveFilterFunc = reserveFilterFunc;
    }

    /// <summary>
    /// Adds the file.
    /// </summary>
    /// <param name="path">The path.</param>
    public void AddFile(string path)
    {
        ExportedFiles.Add(path.CanonicalPath().ToLower());
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        if (ExportRootDirectory.IsDirectoryExists())
        {
            foreach (var file in Directory.EnumerateFiles(ExportRootDirectory, "*.*", SearchOption.AllDirectories))
            {
                if(!Extensions.Any(x=>file.IsExtension(x)))
                {
                    continue;
                }

                var p = file.CanonicalPath().ToLower();

                if (!ExportedFiles.Contains(p))
                {
                    if(ReserveFilterFunc != null && ReserveFilterFunc(p))
                    {
                        continue;
                    }

                    OnDeleteFileDelegate?.Invoke(file);

                    File.Delete(file);
                    Logger.LogWarning($"  Delete Expired File: {file}");
                }
            }

            DeleteEmptyDirectories(ExportRootDirectory);
        }
    }

    private static void DeleteEmptyDirectories(string rootDirectory)
    {
        foreach (var directory in Directory.GetDirectories(rootDirectory))
        {
            DeleteEmptyDirectories(directory);
        }

        if (Directory.GetFiles(rootDirectory).Length == 0 &&
            Directory.GetDirectories(rootDirectory).Length == 0)
        {
            Directory.Delete(rootDirectory, true);
        }
    }
}