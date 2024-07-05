namespace UnrealSharpTool.Core.TypeInfo;

/// <summary>
/// Class ScriptStructTypeDefinition.
/// Implements the <see cref="UnrealSharpTool.Core.TypeInfo.StructTypeDefinition" />
/// </summary>
/// <seealso cref="UnrealSharpTool.Core.TypeInfo.StructTypeDefinition" />
public class ScriptStructTypeDefinition : StructTypeDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptStructTypeDefinition"/> class.
    /// </summary>
    public ScriptStructTypeDefinition()
    {
        Type = EDefinitionType.Struct;
    }

    /// <summary>
    /// Converts the name of the CPP name to script.
    /// </summary>
    /// <param name="cppName">Name of the CPP.</param>
    /// <returns>System.String.</returns>
    public override string ConvertCppNameToScriptName(string cppName)
    {
        return cppName.StartsWith('F') ? cppName[1..] : base.ConvertCppNameToScriptName(cppName);
    }
}