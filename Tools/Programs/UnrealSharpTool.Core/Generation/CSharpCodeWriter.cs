using UnrealSharpTool.Core.CodeGen;

namespace UnrealSharpTool.Core.Generation;

/// <summary>
/// Class CSharpCodeWriter.
/// Write copy right as file header
/// Implements the <see cref="UnrealSharpTool.Core.Generation.CodeWriter" />
/// </summary>
/// <seealso cref="UnrealSharpTool.Core.Generation.CodeWriter" />
public class CSharpCodeWriter : CodeWriter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpCodeWriter"/> class.
    /// </summary>
    public CSharpCodeWriter() 
    {
        Write(CodegenCopyright.GetUnrealSharpCopyright());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpCodeWriter"/> class.
    /// </summary>
    /// <param name="path">The path.</param>
    // ReSharper disable once MemberCanBeProtected.Global
    public CSharpCodeWriter(string path) : base(path) 
    {
        Write(CodegenCopyright.GetUnrealSharpCopyright());
        Write(CodegenCopyright.GetCopyright(path));
    }
}