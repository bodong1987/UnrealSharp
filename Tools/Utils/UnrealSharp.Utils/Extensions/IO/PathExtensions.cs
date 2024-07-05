using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace UnrealSharp.Utils.Extensions.IO;

/// <summary>
/// Class PathExtensions.
/// </summary>
public static class PathExtensions
{
    /// <summary>
    /// Ends the with path separator.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns><c>true</c> if this path end with the path separator , <c>false</c> otherwise.</returns>
    private static bool EndsWithPathSeparator([NotNullWhen(true)]this string? path)
    {
        if(path.IsNullOrEmpty())
        {
            return false;
        }

        return path[^1] == '\\' || path[^1] == '/';    
    }

    /// <summary>
    /// Makes the relative.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="referencePath">The reference path.</param>
    /// <returns>System.String.</returns>
    public static string MakeRelative(this string filePath, string referencePath)
    {
        var fileUri = new Uri(filePath);
        var refPath = referencePath;

        if (!referencePath.EndsWithPathSeparator())
        {
            refPath = referencePath + Path.DirectorySeparatorChar;
        }

        var referenceUri = new Uri(refPath);

        return Uri.UnescapeDataString(referenceUri.MakeRelativeUri(fileUri).ToString());
    }

    /// <summary>
    /// Returns true if <paramref name="path" /> starts with the path <paramref name="baseDirPath" />.
    /// The comparison is case-insensitive, handles / and \ slashes as folder separators and
    /// only matches if the base dir folder name is matched exactly ("c:\foobar\file.txt" is not a sub path of "c:\foo").
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="baseDirPath">The base dir path.</param>
    /// <returns><c>true</c> if [is sub path of] [the specified base dir path]; otherwise, <c>false</c>.</returns>
    public static bool IsSubPathOf(this string path, string baseDirPath)
    {
        var normalizedPath = Path.GetFullPath(path);

        var normalizedBaseDirPath = Path.GetFullPath(baseDirPath);

        return normalizedPath.iStartsWith(normalizedBaseDirPath);
    }

    /// <summary>
    /// Determines whether [is file exists] [the specified path].
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns><c>true</c> if [is file exists] [the specified path]; otherwise, <c>false</c>.</returns>
    public static bool IsFileExists([NotNullWhen(true)] this string? path)
    {
        return path.IsNotNullOrEmpty() && File.Exists(path);
    }

    /// <summary>
    /// Determines whether [is directory exists] [the specified path].
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns><c>true</c> if [is directory exists] [the specified path]; otherwise, <c>false</c>.</returns>
    public static bool IsDirectoryExists([NotNullWhen(true)] this string? path)
    {
        return path.IsNotNullOrEmpty() && Directory.Exists(path);
    }

    /// <summary>
    /// Determines whether [is absolute path] [the specified path].
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns><c>true</c> if [is absolute path] [the specified path]; otherwise, <c>false</c>.</returns>
    public static bool IsAbsolutePath(this string? path)
    {
        return path.IsNotNullOrEmpty() && Path.IsPathRooted(path);
    }

    /// <summary>
    /// Determines whether [is relative path] [the specified path].
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns><c>true</c> if [is relative path] [the specified path]; otherwise, <c>false</c>.</returns>
    public static bool IsRelativePath(this string? path)
    {
        return !IsAbsolutePath(path);
    }

    /// <summary>
    /// Determines whether [is read only file] [the specified path].
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns><c>true</c> if [is read only file] [the specified path]; otherwise, <c>false</c>.</returns>
    public static bool IsReadOnlyFile([NotNullWhen(true)] this string? path)
    {
        if(path.IsNullOrEmpty())
        {
            return false;
        }

        // 不存在的文件就是为只读如何？
        if (!File.Exists(path))
        {
            return true;
        }

        var attr = File.GetAttributes(path);
        return attr.HasFlag(FileAttributes.ReadOnly);
    }

    /// <summary>
    /// Determines whether the specified path is file.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns><c>true</c> if the specified path is file; otherwise, <c>false</c>.</returns>
    public static bool IsFile(this string path)
    {
        try
        {
            var attr = File.GetAttributes(path);
            return !attr.HasFlag(FileAttributes.Directory);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the specified path is directory.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns><c>true</c> if the specified path is directory; otherwise, <c>false</c>.</returns>
    public static bool IsDirectory(this string path)
    {
        try
        {
            var attr = File.GetAttributes(path);
            return attr.HasFlag(FileAttributes.Directory);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Joins the path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="elements">The elements.</param>
    /// <returns>System.String.</returns>
    public static string JoinPath(this string path, params string[] elements)
    {
        var v = new[] { path };
        return Path.Combine(v.Concat(elements).ToArray()).CanonicalPath();
    }

    /// <summary>
    /// Determines whether the specified extension is extension.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="extension">The extension.</param>
    /// <returns><c>true</c> if the specified extension is extension; otherwise, <c>false</c>.</returns>
    public static bool IsExtension(this string path, string extension)
    {
        return Path.GetExtension(path).iEquals(extension);
    }

    /// <summary>
    /// Gets the extension.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>System.String.</returns>
    public static string GetExtension(this string path)
    {
        return Path.GetExtension(path);
    }

    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>System.String.</returns>
    public static string GetFileName(this string path)
    {
        // During actual operation, the interface of the Path class under Mac cannot correctly return the corresponding string,
        // so we will temporarily handle it here.
        var index = path.LastIndexOf(ch => ch is '\\' or '/');

        return index != -1 ? path[(index + 1)..] : Path.GetFileName(path);
    }

    /// <summary>
    /// Gets the file name without extension.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>System.String.</returns>
    public static string GetFileNameWithoutExtension(this string path)
    {
        // During actual operation, the interface of the Path class under Mac cannot correctly return the corresponding string,
        // so we will temporarily handle it here.
        var slashIndex = path.LastIndexOf(ch => ch is '\\' or '/');

        if (slashIndex != -1)
        {
            var index = path.LastIndexOf('.');

            if(index != -1 && index > slashIndex)
            {
                return path.Substring(slashIndex + 1, index - slashIndex-1);
            }                
        }

        return Path.GetFileNameWithoutExtension(path);
    }

    /// <summary>
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>System.String.</returns>
    public static string GetDirectoryPath(this string path)
    {
        return Path.GetDirectoryName(path)!;
    }

    /// <summary>
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>System.String.</returns>
    public static string GetDirectoryName(this string path)
    {
        return Path.GetFileName(Path.GetDirectoryName(path))!;
    }

    /// <summary>
    /// Canonicals the path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>System.String.</returns>
    public static string CanonicalPath(this string path)
    {
        if(path.IsNullOrEmpty())
        {
            return path;
        }

        var p = Path.GetFullPath(path);

        p = Path.DirectorySeparatorChar == '\\' ? p.Replace('/', '\\') : p.Replace('\\', '/');

        return p;
    }

    /// <summary>
    /// Determines whether [is path equal] [the specified other path].
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="otherPath">The other path.</param>
    /// <returns><c>true</c> if [is path equal] [the specified other path]; otherwise, <c>false</c>.</returns>
    public static bool IsPathEqual(this string path, string otherPath)
    {
        return path.CanonicalPath().iEquals(otherPath.CanonicalPath());
    }

    /// <summary>
    /// Ensures the extension.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="extension">The extension.</param>
    /// <returns>System.String.</returns>
    public static string EnsureExtension(this string path, string extension)
    {
        var ext = path.GetExtension();

        if (ext.IsNullOrEmpty())
        {
            return path + extension;
        }

        return path.GetFileNameWithoutExtension() + extension;
    }

    /// <summary>
    /// Determines whether [is valid file name] [the specified filename].
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <returns><c>true</c> if [is valid file name] [the specified filename]; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidFileName(this string filename)
    {
        return !(filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidPath(this string path)
    {
        return !(path.IndexOfAny(Path.GetInvalidPathChars()) >= 0);
    }
}