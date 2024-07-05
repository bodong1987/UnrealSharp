using System.Diagnostics;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Core.CodeGen;

/// <summary>
/// Class CSharpBindingExporter.
/// Export binding codes, based on your BIndingContext
/// </summary>
public class CSharpBindingExporter
{
    /// <summary>
    /// The context
    /// </summary>
    public readonly BindingContext Context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpBindingExporter"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public CSharpBindingExporter(BindingContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Exports this instance.
    /// </summary>
    /// <returns><c>true</c> success, <c>false</c> otherwise.</returns>
    /// <exception cref="System.Exception">Failed Export {type}.</exception>
    public bool Export()
    {
        var totalCount = 0;
        var skipCount = 0;
        var writeCount = 0;

        foreach(var projectWithTypes in Context.ProjectTypeMapping)
        {
            var exportRootDirectory = Path.Combine(Context.UnrealProjectDirectory, $"GameScripts/Game/{projectWithTypes.Key}/Bindings/{Context.SchemaType}");

            using var exportScope = new ScopedExporter(Context.UnrealProjectDirectory, exportRootDirectory);
    
            foreach (var type in projectWithTypes.Value)
            {
                var exportDirectory = Path.Combine(exportRootDirectory, type.PackageName!);

                var exporter = CreateExporter(exportDirectory, type);

                if (exporter == null)
                {
                    Logger.LogWarning($"Skip export type: {type}");
                    continue;
                }

                ++totalCount;

                var result = exporter.Export();

                switch (result)
                {
                    case ECodeWriterSaveResult.Failure:
                        throw new Exception($"Failed Export {type} to path: {exporter.TargetFile}.");
                    case ECodeWriterSaveResult.IgnoreWhenNoChanges:
                        ++skipCount;
                        break;
                    case ECodeWriterSaveResult.Success:
                        ++writeCount;
                        break;
                }

                if(!Debugger.IsAttached && result == ECodeWriterSaveResult.Success)
                {
                    Logger.Log($"  Export: {exporter.TargetFile}");
                }

                exportScope.AddFile(exporter.TargetFile);
            }
        }

        Logger.Log($"Finish export C# binding codes, process {totalCount} files, write {writeCount} files, skip {skipCount} files[no changes].");

        return true;
    }

    /// <summary>
    /// Creates the exporter.
    /// </summary>
    /// <param name="exportDirectory">The export directory.</param>
    /// <param name="typeDefinition">The type definition.</param>
    /// <returns>System.Nullable&lt;BaseTypeExporter&gt;.</returns>
    protected virtual BaseTypeExporter? CreateExporter(string exportDirectory, BaseTypeDefinition typeDefinition)
    {
        if (typeDefinition.IsEnum)
        {
            return new EnumTypeExporter(Context, Path.Combine(exportDirectory, "Enums"), (typeDefinition as EnumTypeDefinition)!);
        }

        if (typeDefinition.IsStruct)
        {
            return new ScriptStructTypeExporter(Context, Path.Combine(exportDirectory, "Structs"), (typeDefinition as ScriptStructTypeDefinition)!);
        }

        if (typeDefinition.IsClass || typeDefinition.IsInterface)
        {
            return new ClassTypeExporter(Context, Path.Combine(exportDirectory, "Classes"), (typeDefinition as ClassTypeDefinition)!);
        }

        return null;
    }
}