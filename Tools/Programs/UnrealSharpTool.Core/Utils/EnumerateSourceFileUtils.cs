using System.Text.RegularExpressions;
using UnrealSharp.Utils.Extensions;

namespace UnrealSharpTool.Core.Utils
{
    /// <summary>
    /// Class EnumerateSourceFileUtils.
    /// </summary>
    public static class EnumerateSourceFileUtils
    {
        /// <summary>
        /// Enumerates the source files.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns>System.String[].</returns>
        public static string[] EnumerateSourceFiles(string directory)
        {
            return EnumerateSourceFiles(directory, null);
        }

        /// <summary>
        /// Enumerates the source files.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="ignoreRegex">The ignore regex.</param>
        /// <returns>System.String[].</returns>
        public static string[] EnumerateSourceFiles(string directory, string? ignoreRegex)
        {
            var files = Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories);

            if(ignoreRegex.IsNullOrEmpty())
            {
                return files.ToArray();
            }

            List<string> Result = new List<string>();

            Regex regex = new Regex(ignoreRegex, RegexOptions.Compiled|RegexOptions.IgnoreCase);

            foreach(var file in files)
            {
                if(!regex.IsMatch(file))
                {
                    Result.Add(file);
                }
            }

            return Result.ToArray();
        }
    }
}
