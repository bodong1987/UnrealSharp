using UnrealSharp.Utils.Extensions;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace UnrealSharpTool.Core.TypeInfo;

/// <summary>
/// Struct SymbolSourceInfo
/// </summary>
public struct SymbolSourceInfo
{
    /// <summary>
    /// Gets or sets the file path.
    /// </summary>
    /// <value>The file path.</value>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the line.
    /// </summary>
    /// <value>The line.</value>
    public int Line { get; set; } = 0;

    /// <summary>
    /// Returns true if ... is valid.
    /// </summary>
    /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
    public bool IsValid => FilePath.IsNotNullOrEmpty() && Line > 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="SymbolSourceInfo" /> struct.
    /// </summary>
    public SymbolSourceInfo()
    {
    }
}

/// <summary>
/// Enum SymbolIdentifierType
/// </summary>
public enum SymbolIdentifierType
{
    /// <summary>
    /// The none
    /// </summary>
    None,
    /// <summary>
    /// The class
    /// </summary>
    Class,
    /// <summary>
    /// The structure
    /// </summary>
    Struct,
    /// <summary>
    /// The enum
    /// </summary>
    Enum,
    /// <summary>
    /// The interface
    /// </summary>
    Interface,
    /// <summary>
    /// The property
    /// </summary>
    Property,
    /// <summary>
    /// The field
    /// </summary>
    Field,
    /// <summary>
    /// The method
    /// </summary>
    Method
}

/// <summary>
/// Class SymbolIdentifier.
/// </summary>
public class SymbolIdentifier
{
    /// <summary>
    /// The type
    /// </summary>
    public SymbolIdentifierType Type = SymbolIdentifierType.None;
    /// <summary>
    /// The name
    /// </summary>
    public string Name = "";
    /// <summary>
    /// The owner name
    /// </summary>
    public string OwnerName="";

    /// <summary>
    /// Initializes a new instance of the <see cref="SymbolIdentifier"/> class.
    /// </summary>
    public SymbolIdentifier()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SymbolIdentifier"/> class.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="name">The name.</param>
    /// <param name="ownerName">Name of the owner.</param>
    public SymbolIdentifier(SymbolIdentifierType type, string name, string ownerName = "")
    {
        Type = type;
        Name = name;
        OwnerName = ownerName;
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        return $"({Type}){Name}[{OwnerName}]";
    }
}

/// <summary>
/// Interface IDebugInformation
/// </summary>
public interface IDebugInformation
{
    /// <summary>
    /// Resolves the source information.
    /// </summary>
    /// <param name="symbol">The symbol.</param>
    /// <returns>SymbolSourceInfo.</returns>
    public IEnumerable<SymbolSourceInfo>? ResolveSourceInfo(SymbolIdentifier symbol);
}