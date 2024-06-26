using UnrealSharp.Utils.Extensions;
using UnrealSharpTool.Core.CodeGen;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Core.DefCodeGen
{
    /// <summary>
    /// Class CSharpDefCodeExporter.
    /// Export place holder types, used to C# code pre build process
    /// </summary>
    public class CSharpBindingDefinitionPlaceholdersExporter
    {
        /// <summary>
        /// The context
        /// </summary>
        public readonly BindingContext Context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpDefCodeExporter"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public CSharpBindingDefinitionPlaceholdersExporter(BindingContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Exports this instance.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Export()
        {
            foreach (var projectWithTypes in Context.ProjectTypeMapping)
            {
                string ExportRootDirectory = Path.Combine(Context.UnrealProjectDirectory, $"GameScripts/Game/{projectWithTypes.Key}/Bindings.Placeholders/{Context.SchemaType}");

                using ScopedExporter ExportScope = new ScopedExporter(Context.UnrealProjectDirectory, ExportRootDirectory, ShouldReserve);

                var dependencies = CSharpProjectUtils.GetUnrealSharpProjectDependProjectNames(Context.UnrealProjectDirectory, projectWithTypes.Key).ToList();

                dependencies.RemoveAll(x => !CSharpProjectUtils.IsUnrealSharpGameProject(Context.UnrealProjectDirectory, x));

                CSharpBindingDefinitionPlaceholderPackage package = 
                    new CSharpBindingDefinitionPlaceholderPackage(Context, ExportRootDirectory, projectWithTypes.Key, dependencies);

                projectWithTypes.Value.ForEach(package.Process);

                package.Save();

                package.ExportedPathes.ToList().ForEach(ExportScope.AddFile);
            }

            return true;
        }

        private bool ShouldReserve(string file)
        {
            // reserve for UnrealEngine project built-in file
            return file.iEndsWith(".builtins.cs");
        }
    }
}
