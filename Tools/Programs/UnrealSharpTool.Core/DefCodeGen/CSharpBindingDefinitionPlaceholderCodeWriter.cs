using UnrealSharpTool.Core.Generation;

namespace UnrealSharpTool.Core.DefCodeGen;

internal class CSharpBindingDefinitionPlaceholderCodeWriter : CSharpCodeWriter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpBindingDefinitionPlaceholderCodeWriter" /> class.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="projectName">Name of the project.</param>
    /// <param name="dependencyProjects"></param>
    public CSharpBindingDefinitionPlaceholderCodeWriter(string path, string projectName, IEnumerable<string>? dependencyProjects = null) :
        base(path)
    {
        //   Write("#if !UNREALSHARP_SHIPPING");
        
        // provide by .editorconfig
        // write rider static check ignore comments
        //Write("// ReSharper disable InconsistentNaming");
        //Write("// ReSharper disable UnusedTypeParameter");
        Write("// ReSharper disable IdentifierTypo");
        //Write("// ReSharper disable RedundantAttributeParentheses");
        //Write("// ReSharper disable UnusedParameter.Global");
        //Write("// ReSharper disable UnassignedField.Global");
        
        Write("using UnrealSharp.Utils.UnrealEngine;");

        if(dependencyProjects != null)
        {
            foreach(var dependProject in dependencyProjects)
            {
                Write($"using {dependProject}.Bindings.Placeholders;");
            }
        }

        WriteNewLine();
        Write($"namespace {projectName}.Bindings.Placeholders;");
        WriteNewLine();

        Write("#pragma warning disable CS1591 // Missing XML annotation for publicly visible type or member");
        Write("#pragma warning disable CS0114 // Member hides inherited members; keyword override is missing");
    }

    /// <summary>
    /// Saves the specified force save.
    /// </summary>
    /// <param name="forceSave">if set to <c>true</c> [force save].</param>
    /// <returns>ECodeWriterSaveResult.</returns>
    public override ECodeWriterSaveResult Save(bool forceSave = false)
    {
        WriteNewLine();
        Write("#pragma warning restore CS0114 // Member hides inherited members; keyword override is missing");
        Write("#pragma warning restore CS1591 // Missing XML annotation for publicly visible type or member");
        //     Write("#endif");

        return base.Save(forceSave);
    }
}