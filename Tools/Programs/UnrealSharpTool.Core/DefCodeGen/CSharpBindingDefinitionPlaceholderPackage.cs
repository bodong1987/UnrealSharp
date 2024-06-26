using UnrealSharp.Utils.Extensions.IO;
using UnrealSharpTool.Core.CodeGen;
using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.DefCodeGen
{
    /// <summary>
    /// Class CSharpBindingDefinitionPlaceholderPackage.
    /// </summary>
    class CSharpBindingDefinitionPlaceholderPackage
    {
        /// <summary>
        /// The target directory
        /// </summary>
        public readonly string TargetDirectory;
        /// <summary>
        /// The name
        /// </summary>
        public readonly string ProjectName;

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>The short name.</value>
        public string ShortName
        {
            get
            {
                string ShortProjectName = ProjectName;
                int index = ShortProjectName.LastIndexOf('.');

                if (index != -1)
                {
                    ShortProjectName = ShortProjectName.Substring(index + 1);
                }

                return ShortProjectName;
            }
        }

        public readonly IEnumerable<string>? DependProjects;

        /// <summary>
        /// The context
        /// </summary>
        public readonly BindingContext Context;

        /// <summary>
        /// The enum writer
        /// </summary>
        CSharpBindingDefinitionPlaceholderCodeWriter? EnumWriter;
        /// <summary>
        /// The structure writer
        /// </summary>
        CSharpBindingDefinitionPlaceholderCodeWriter? StructWriter;
        /// <summary>
        /// The class writer
        /// </summary>
        CSharpBindingDefinitionPlaceholderCodeWriter? ClassWriter;
        /// <summary>
        /// The writers
        /// </summary>
        List<CSharpBindingDefinitionPlaceholderCodeWriter> Writers = new List<CSharpBindingDefinitionPlaceholderCodeWriter>();

        /// <summary>
        /// Gets the exported pathes.
        /// </summary>
        /// <value>The exported pathes.</value>
        public string[] ExportedPathes => Writers.Select(x => x.TargetPath).ToArray()!;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefCodeExportPackage"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="name">The name.</param>
        public CSharpBindingDefinitionPlaceholderPackage(
            BindingContext context, 
            string targetDirectory, 
            string projectName,
            IEnumerable<string>? dependProjects
            )
        {
            Context = context;
            TargetDirectory = targetDirectory;
            ProjectName = projectName;
            DependProjects = dependProjects;

            if(!TargetDirectory.IsDirectoryExists())
            {
                Directory.CreateDirectory(TargetDirectory);
            }
        }

        /// <summary>
        /// Processes the specified type definition.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        public void Process(BaseTypeDefinition typeDefinition)
        {
            if (typeDefinition.IsEnum)
            {
                if (EnumWriter == null)
                {
                    EnumWriter = new CSharpBindingDefinitionPlaceholderCodeWriter(Path.Combine(TargetDirectory, $"{ShortName}.{Context.SchemaType.ToString().ToLower()}.placeholders.enums.cs"), ProjectName);
                    Writers.Add(EnumWriter);
                }

                EnumWriter.Write("[BindingDefinition]");
                EnumWriter.Write($"public enum {typeDefinition.CppName}{{}}");
            }
            else if (typeDefinition.IsStruct)
            {
                if (StructWriter == null)
                {
                    StructWriter = new CSharpBindingDefinitionPlaceholderCodeWriter(Path.Combine(TargetDirectory, $"{ShortName}.{Context.SchemaType.ToString().ToLower()}.placeholders.structs.cs"), ProjectName, DependProjects);
                    Writers.Add(StructWriter);
                }

                StructWriter.Write("[BindingDefinition]");
                StructWriter.Write($"public struct {typeDefinition.CppName}{{}}");
            }
            else if (typeDefinition.IsClass || typeDefinition.IsInterface)
            {
                if (ClassWriter == null)
                {
                    ClassWriter = new CSharpBindingDefinitionPlaceholderCodeWriter(Path.Combine(TargetDirectory, $"{ShortName}.{Context.SchemaType.ToString().ToLower()}.placeholders.classes.cs"), ProjectName, DependProjects);
                    Writers.Add(ClassWriter);
                }

                ClassWriter.Write("[BindingDefinition]");
                if (typeDefinition.IsClass)
                {
                    ClassWriter.Write($"public abstract class {typeDefinition.CppName} : {(typeDefinition as ClassTypeDefinition)!.SuperName}");

                    {
                        using ScopedCodeWriter DefScope = new ScopedCodeWriter(ClassWriter);

                        foreach (var function in (typeDefinition as ClassTypeDefinition)!.Functions)
                        {
                            if (function.IsEvent)
                            {
                                string returnType = function.GetReturnTypeName(Context);
                                string paramList = function.GetExportParameters(Context);

                                ClassWriter.Write("[UEVENT()]");
                                ClassWriter.Write($"public virtual {returnType} {function.Name}({paramList}){{{(function.HasReturnType() ? " return default; " : "")}}}");
                            }
                        }
                    }
                }
                else
                {
                    var superName = (typeDefinition as ClassTypeDefinition)!.SuperName;
                    var superInterface = superName == "UInterface" ? "IUnrealObject" : "I" + superName.Substring(1);

                    ClassWriter.Write($"public interface {"I" + typeDefinition.CppName!.Substring(1)} : {superInterface}{{}}");
                }

                ClassWriter.WriteNewLine();
            }
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
            Writers.ForEach(x => x.Save());
        }
    }
}
