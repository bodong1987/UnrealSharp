using UnrealSharp.Utils.Extensions.IO;
using UnrealSharpTool.Core.CodeGen;
using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.DefCodeGen;

/// <summary>
/// Class CSharpBindingDefinitionPlaceholderPackage.
/// </summary>
internal class CSharpBindingDefinitionPlaceholderPackage
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
            var shortProjectName = ProjectName;
            var index = shortProjectName.LastIndexOf('.');

            if (index != -1)
            {
                shortProjectName = shortProjectName[(index + 1)..];
            }

            return shortProjectName;
        }
    }

    public readonly IEnumerable<string>? DependencyProjects;

    /// <summary>
    /// The context
    /// </summary>
    public readonly BindingContext Context;

    /// <summary>
    /// The enum writer
    /// </summary>
    private CSharpBindingDefinitionPlaceholderCodeWriter? _enumWriter;
        
    /// <summary>
    /// The structure writer
    /// </summary>
    private CSharpBindingDefinitionPlaceholderCodeWriter? _structWriter;
        
    /// <summary>
    /// The class writer
    /// </summary>
    private CSharpBindingDefinitionPlaceholderCodeWriter? _classWriter;
        
    /// <summary>
    /// The writers
    /// </summary>
    private readonly List<CSharpBindingDefinitionPlaceholderCodeWriter> _writers = [];

    /// <summary>
    /// Gets the exported paths.
    /// </summary>
    /// <value>The exported paths.</value>
    public string[] ExportedPaths => _writers.Select(x => x.TargetPath).ToArray()!;

    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpBindingDefinitionPlaceholderPackage"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="targetDirectory">The target directory.</param>
    /// <param name="projectName"></param>
    /// <param name="dependencyProjects"></param>
    public CSharpBindingDefinitionPlaceholderPackage(
        BindingContext context, 
        string targetDirectory, 
        string projectName,
        IEnumerable<string>? dependencyProjects
    )
    {
        Context = context;
        TargetDirectory = targetDirectory;
        ProjectName = projectName;
        DependencyProjects = dependencyProjects;

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
            if (_enumWriter == null)
            {
                _enumWriter = new CSharpBindingDefinitionPlaceholderCodeWriter(Path.Combine(TargetDirectory, $"{ShortName}.{Context.SchemaType.ToString().ToLower()}.placeholders.enums.cs"), ProjectName);
                _writers.Add(_enumWriter);
            }

            _enumWriter.Write("[BindingDefinition]");
            _enumWriter.Write($"public enum {typeDefinition.CppName}{{}}");
        }
        else if (typeDefinition.IsStruct)
        {
            if (_structWriter == null)
            {
                _structWriter = new CSharpBindingDefinitionPlaceholderCodeWriter(Path.Combine(TargetDirectory, $"{ShortName}.{Context.SchemaType.ToString().ToLower()}.placeholders.structs.cs"), ProjectName, DependencyProjects);
                _writers.Add(_structWriter);
            }

            _structWriter.Write("[BindingDefinition]");
            _structWriter.Write($"public struct {typeDefinition.CppName}{{}}");
        }
        else if (typeDefinition.IsClass || typeDefinition.IsInterface)
        {
            if (_classWriter == null)
            {
                _classWriter = new CSharpBindingDefinitionPlaceholderCodeWriter(Path.Combine(TargetDirectory, $"{ShortName}.{Context.SchemaType.ToString().ToLower()}.placeholders.classes.cs"), ProjectName, DependencyProjects);
                _writers.Add(_classWriter);
            }

            _classWriter.Write("[BindingDefinition]");
            if (typeDefinition.IsClass)
            {
                _classWriter.Write($"public abstract class {typeDefinition.CppName} : {(typeDefinition as ClassTypeDefinition)!.SuperName}");

                {
                    using var defScope = new ScopedCodeWriter(_classWriter);

                    foreach (var function in (typeDefinition as ClassTypeDefinition)!.Functions)
                    {
                        if (function.IsEvent)
                        {
                            var returnType = function.GetReturnTypeName(Context);
                            var paramList = function.GetExportParameters(Context);

                            _classWriter.Write("[UEVENT()]");
                            _classWriter.Write($"public virtual {returnType} {function.Name}({paramList}){{{(function.HasReturnType() ? " return default; " : "")}}}");
                        }
                    }
                }
            }
            else
            {
                var superName = (typeDefinition as ClassTypeDefinition)!.SuperName;
                var superInterface = superName == "UInterface" ? "IUnrealObject" : string.Concat("I", superName.AsSpan(1));

                _classWriter.Write($"public interface {string.Concat("I", typeDefinition.CppName!.AsSpan(1))} : {superInterface}{{}}");
            }

            _classWriter.WriteNewLine();
        }
    }

    /// <summary>
    /// Saves this instance.
    /// </summary>
    public void Save()
    {
        _writers.ForEach(x => x.Save());
    }
}