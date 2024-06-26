using UnrealSharpTool.Core.Generation;

namespace UnrealSharpTool.Core.DefCodeGen
{
    class CSharpBindingDefinitionPlaceholderCodeWriter : CSharpCodeWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpDefCodeWriter" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="projectName">Name of the project.</param>
        public CSharpBindingDefinitionPlaceholderCodeWriter(string path, string projectName, IEnumerable<string>? dependProjects = null) :
            base(path)
        {
            //   Write("#if !UNREALSHARP_SHIPPING");
            Write("using UnrealSharp.Utils.UnrealEngine;");

            if(dependProjects != null)
            {
                foreach(var dependProject in dependProjects)
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
}
