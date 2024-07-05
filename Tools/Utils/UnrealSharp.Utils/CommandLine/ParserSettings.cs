// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
namespace UnrealSharp.Utils.CommandLine;

/// <summary>
/// Class ParserSettings.
/// </summary>
public class ParserSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether [case-sensitive].
    /// </summary>
    /// <value><c>true</c> if [case-sensitive]; otherwise, <c>false</c>.</value>
    public bool CaseSensitive { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether [ignore unknown arguments].
    /// </summary>
    /// <value><c>true</c> if [ignore unknown arguments]; otherwise, <c>false</c>.</value>
    public bool IgnoreUnknownArguments { get; set; } = true;
}