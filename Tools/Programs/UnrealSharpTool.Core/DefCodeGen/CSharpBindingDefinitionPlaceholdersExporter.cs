using UnrealSharp.Utils.Extensions;
using UnrealSharpTool.Core.CodeGen;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Core.DefCodeGen;

/// <summary>
/// Class CSharpDefCodeExporter.
/// Export placeholder types, used to C# code pre-build process
/// </summary>
public class CSharpBindingDefinitionPlaceholdersExporter
{
    /// <summary>
    /// The context
    /// </summary>
    public readonly BindingContext Context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpBindingDefinitionPlaceholdersExporter"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public CSharpBindingDefinitionPlaceholdersExporter(BindingContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Exports this instance.
    /// </summary>
    /// <returns><c>true</c> success, <c>false</c> otherwise.</returns>
    public bool Export()
    {
        foreach (var projectWithTypes in Context.ProjectTypeMapping)
        {
            var exportRootDirectory = Path.Combine(Context.UnrealProjectDirectory, $"GameScripts/Game/{projectWithTypes.Key}/Bindings.Placeholders/{Context.SchemaType}");

            using var exportScope = new ScopedExporter(Context.UnrealProjectDirectory, exportRootDirectory, ShouldReserve);

            var dependencies = CSharpProjectUtils.GetUnrealSharpProjectDependProjectNames(Context.UnrealProjectDirectory, projectWithTypes.Key).ToList();

            dependencies.RemoveAll(x => !CSharpProjectUtils.IsUnrealSharpGameProject(Context.UnrealProjectDirectory, x));

            var package = 
                new CSharpBindingDefinitionPlaceholderPackage(Context, exportRootDirectory, projectWithTypes.Key, dependencies);

            projectWithTypes.Value.ForEach(package.Process);

            package.Save();

            package.ExportedPaths.ToList().ForEach(exportScope.AddFile);
        }

        return true;
    }

    private static bool ShouldReserve(string file)
    {
        // reserve for UnrealEngine project built-in file
        return file.iEndsWith(".builtins.cs");
    }
}