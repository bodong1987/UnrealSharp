using System.Diagnostics;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.TypeInfo;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Core.CodeGen
{
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
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="System.Exception">Failed Export {type}.</exception>
        public bool Export()
        {
            int TotalCount = 0;
            int SkipCount = 0;
            int WriteCount = 0;

            foreach(var projectWithTypes in Context.ProjectTypeMapping)
            {
                string ExportRootDirectory = Path.Combine(Context.UnrealProjectDirectory, $"GameScripts/Game/{projectWithTypes.Key}/Bindings/{Context.SchemaType}");

                using ScopedExporter ExportScope = new ScopedExporter(Context.UnrealProjectDirectory, ExportRootDirectory);
    
                foreach (var type in projectWithTypes.Value)
                {
                    string ExportDirectory = Path.Combine(ExportRootDirectory, type.PackageName!);

                    var exporter = CreateExporter(ExportDirectory, type);

                    if (exporter == null)
                    {
                        Logger.LogWarning($"Skip export type: {type}");
                        continue;
                    }

                    ++TotalCount;

                    var Result = exporter.Export();

                    if (Result == Generation.ECodeWriterSaveResult.Failure)
                    {
                        throw new Exception($"Failed Export {type} to path: {exporter.TargetFile}.");
                    }
                    else if(Result == Generation.ECodeWriterSaveResult.IgnoreWhenNoChanges)
                    {
                        ++SkipCount;
                    }
                    else if(Result == Generation.ECodeWriterSaveResult.Success)
                    {
                        ++WriteCount;
                    }

                    if(!Debugger.IsAttached && Result == Generation.ECodeWriterSaveResult.Success)
                    {
                        Logger.Log($"  Export: {exporter.TargetFile}");
                    }

                    ExportScope.AddFile(exporter.TargetFile);
                }
            }

            Logger.Log($"Finish export C# binding codes, process {TotalCount} files, write {WriteCount} files, skip {SkipCount} files[no changes].");

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
            else if (typeDefinition.IsStruct)
            {
                return new ScriptStructTypeExporter(Context, Path.Combine(exportDirectory, "Structs"), (typeDefinition as ScriptStructTypeDefinition)!);
            }
            else if (typeDefinition.IsClass || typeDefinition.IsInterface)
            {
                return new ClassTypeExporter(Context, Path.Combine(exportDirectory, "Classes"), (typeDefinition as ClassTypeDefinition)!);
            }

            return null;
        }
    }
}
