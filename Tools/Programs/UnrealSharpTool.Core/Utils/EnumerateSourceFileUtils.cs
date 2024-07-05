using System.Text.RegularExpressions;
using UnrealSharp.Utils.Extensions;

namespace UnrealSharpTool.Core.Utils;

/// <summary>
/// Class EnumerateSourceFileUtils.
/// </summary>
public static class EnumerateSourceFileUtils
{
    /// <summary>
    /// Enumerates the source files.
    /// </summary>
    /// <param name="directory">The directory.</param>
    /// <param name="ignoreRegex">The ignore regex.</param>
    /// <returns>System.String[].</returns>
    public static string[] EnumerateSourceFiles(string directory, string? ignoreRegex = null)
    {
        var files = Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories);

        if(ignoreRegex.IsNullOrEmpty())
        {
            return files.ToArray();
        }

        List<string> result = [];

        var regex = new Regex(ignoreRegex, RegexOptions.Compiled|RegexOptions.IgnoreCase);

        result.AddRange(files.Where(file => !regex.IsMatch(file)));

        return result.ToArray();
    }
}