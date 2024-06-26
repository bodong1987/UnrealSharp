using Microsoft.CodeAnalysis.CSharp;

namespace UnrealSharpTool.Core.Utils
{
    /// <summary>
    /// Class CSharpStringExtensions.
    /// </summary>
    public static class CSharpStringExtensions
    {
        /// <summary>
        /// Determines whether [is valid c sharp identifier] [the specified identifier].
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <returns><c>true</c> if [is valid c sharp identifier] [the specified identifier]; otherwise, <c>false</c>.</returns>
        public static bool IsValidCSharpIdentifier(this string identifier)
        {
            return SyntaxFacts.IsValidIdentifier(identifier);
        }

        /// <summary>
        /// Determines whether [is c sharp keywords] [the specified identifier].
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <returns><c>true</c> if [is c sharp keywords] [the specified identifier]; otherwise, <c>false</c>.</returns>
        public static bool IsCSharpKeywords(this string identifier)
        {
            return SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None;
        }
    }
}
