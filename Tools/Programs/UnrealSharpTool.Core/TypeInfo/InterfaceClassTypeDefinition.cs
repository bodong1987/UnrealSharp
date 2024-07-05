namespace UnrealSharpTool.Core.TypeInfo;

/// <summary>
/// Class InterfaceClassTypeDefinition.
/// Implements the <see cref="UnrealSharpTool.Core.TypeInfo.ClassTypeDefinition" />
/// </summary>
/// <seealso cref="UnrealSharpTool.Core.TypeInfo.ClassTypeDefinition" />
public class InterfaceClassTypeDefinition : ClassTypeDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InterfaceClassTypeDefinition"/> class.
    /// </summary>
    public InterfaceClassTypeDefinition()
    {
        Type = EDefinitionType.Interface;
    }
}