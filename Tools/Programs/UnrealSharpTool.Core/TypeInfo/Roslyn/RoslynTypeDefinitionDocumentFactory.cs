using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Extensions.IO;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.ErrorReports;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Core.TypeInfo.Roslyn
{
    #region Source File
    /// <summary>
    /// Class CSharpSourceFile.
    /// </summary>
    internal class CSharpSourceFile
    {
        /// <summary>
        /// The file path
        /// </summary>
        public readonly string FilePath;
        /// <summary>
        /// The file text
        /// </summary>
        public readonly string FileText;
        /// <summary>
        /// The tree
        /// </summary>
        public readonly SyntaxTree Tree;
        /// <summary>
        /// The project name
        /// </summary>
        public readonly string ProjectName;

        /// <summary>
        /// Gets the compilation.
        /// </summary>
        /// <value>The compilation.</value>
        public CSharpCompilation? Compilation { get; private set; }
        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>
        public SemanticModel? Model { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpSourceFile"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="projectName">Name of the project.</param>
        public CSharpSourceFile(string filePath, string projectName)
        {
            FilePath = filePath;
            ProjectName = projectName;

            FileText = File.ReadAllText(FilePath);

            Tree = CSharpSyntaxTree.ParseText(FileText);            
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>string.</returns>
        public override string ToString()
        {
            return FilePath;
        }

        /// <summary>
        /// Compiles the specified compilation.
        /// </summary>
        /// <param name="compilation">The compilation.</param>
        public void Compile(CSharpCompilation compilation) 
        {
            Compilation = compilation;
            Model = Compilation.GetSemanticModel(Tree);
        }

        /// <summary>
        /// Gets the declaration members.
        /// </summary>
        /// <returns>System.Collections.Generic.IEnumerable&lt;Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax&gt;.</returns>
        public IEnumerable<MemberDeclarationSyntax> GetDeclarationMembers()
        {
            var members = Tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>();
            return members;
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>System.Collections.Generic.IEnumerable&lt;Microsoft.CodeAnalysis.AttributeData&gt;.</returns>
        public IEnumerable<AttributeData> GetAttributes(MemberDeclarationSyntax member)
        {
            var symbol = Model.GetDeclaredSymbol(member);

            Logger.EnsureNotNull(symbol);

            return symbol.GetAttributes();
        }

        /// <summary>
        /// Gets the name of the project tiny.
        /// </summary>
        /// <returns>string.</returns>
        public string GetProjectTinyName()
        {
            var index = ProjectName.LastIndexOf('.');
            if(index >= 0)
            {
                return ProjectName.Substring(index + 1);
            }

            return ProjectName;
        }
    }
    #endregion

    /// <summary>
    /// Class RoslynTypeDefinitionDocumentFactory.
    /// </summary>
    internal static class RoslynTypeDefinitionDocumentFactory
    {
        #region Source Dependencies
        /// <summary>
        /// Loads the project type definition files.
        /// </summary>
        /// <param name="unrealprojectDirectory">The unrealproject directory.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <returns>System.Collections.Generic.IEnumerable&lt;UnrealSharpTool.Core.TypeInfo.Roslyn.CSharpSourceFile&gt;.</returns>
        public static IEnumerable<CSharpSourceFile> LoadProjectTypeDefinitionFiles(string unrealprojectDirectory, string projectName)
        {
            string projectDirectory = unrealprojectDirectory.JoinPath($"GameScripts/Game/{projectName}/Bindings.Defs");

            if(projectDirectory.IsDirectoryExists())
            {
                var files = Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories);

                return files.Select(x => new CSharpSourceFile(x, projectName));
            }

            return [];
        }

        /// <summary>
        /// Loads the project native binding placeholders.
        /// </summary>
        /// <param name="unrealprojectDirectory">The unrealproject directory.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <returns>System.Collections.Generic.IEnumerable&lt;UnrealSharpTool.Core.TypeInfo.Roslyn.CSharpSourceFile&gt;.</returns>
        public static IEnumerable<CSharpSourceFile> LoadProjectNativeBindingPlaceholders(string unrealprojectDirectory, string projectName)
        {
            string projectDirectory = unrealprojectDirectory.JoinPath($"GameScripts/Game/{projectName}/Bindings.Placeholders/NativeBinding");

            if (projectDirectory.IsDirectoryExists())
            {
                var files = Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories);

                return files.Select(x => new CSharpSourceFile(x, projectName));
            }

            return [];
        }

        /// <summary>
        /// Loads the unreal engine builtin placeholders.
        /// </summary>
        /// <param name="unrealprojectDirectory">The unrealproject directory.</param>
        /// <returns>System.Collections.Generic.IEnumerable&lt;UnrealSharpTool.Core.TypeInfo.Roslyn.CSharpSourceFile&gt;.</returns>
        public static IEnumerable<CSharpSourceFile> LoadUnrealEngineBuiltinPlaceholders(string unrealprojectDirectory)
        {
            string projectDirectory = unrealprojectDirectory.JoinPath($"GameScripts/Game/UnrealSharp.UnrealEngine/Bindings.Placeholders.Builtin");

            if (projectDirectory.IsDirectoryExists())
            {
                var files = Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories);

                return files.Select(x => new CSharpSourceFile(x, "UnrealSharp.UnrealEngine"));
            }

            return [];
        }


        /// <summary>
        /// Loads the depend project related files.
        /// </summary>
        /// <param name="unrealprojectDirectory">The unrealproject directory.</param>
        /// <param name="projectNames">The project names.</param>
        /// <returns>IEnumerable&lt;CSharpSourceFile&gt;.</returns>
        public static IEnumerable<CSharpSourceFile> LoadDependProjectRelatedFiles(string unrealprojectDirectory, IEnumerable<string> projectNames)
        {
            List<CSharpSourceFile> Result = new List<CSharpSourceFile>();

            foreach(var projectName in projectNames)
            {
                string projectDirectory = unrealprojectDirectory.JoinPath($"GameScripts/Game/{projectName}/Bindings.Placeholders");

                if(projectDirectory.IsDirectoryExists())
                {
                    var files = Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories);

                    Result.AddRange(files.Select(x => new CSharpSourceFile(x, projectName)));
                }
            }

            return Result;
        }

        /// <summary>
        /// Loads the extra utility files.
        /// </summary>
        /// <param name="projectDirectory">The project directory.</param>
        /// <returns>IEnumerable&lt;CSharpSourceFile&gt;.</returns>
        public static IEnumerable<CSharpSourceFile> LoadExtraUtilFiles(string projectDirectory)
        {
            string IntermediatePath = Path.Combine(projectDirectory, "Intermediate/UnrealSharp");
            if(!Directory.Exists(IntermediatePath))
            {
                Directory.CreateDirectory(IntermediatePath);
            }

            string globalUsingFile = Path.Combine(IntermediatePath, "__globalusing_helper.cs");
            if(!File.Exists(globalUsingFile))
            {
                File.WriteAllText(globalUsingFile, $"global using global::System;{Environment.NewLine}global using global::System.Collections.Generic;{Environment.NewLine}global using global::System.IO;{Environment.NewLine}global using global::System.Linq;{Environment.NewLine}global using global::System.Net.Http;{Environment.NewLine}global using global::System.Threading;{Environment.NewLine}global using global::System.Threading.Tasks;{Environment.NewLine}");
            }

            return [
                new CSharpSourceFile(globalUsingFile, "Helper")
                ];
        }

        /// <summary>
        /// Loads the compile target files.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="csprojPath">The csproj path.</param>
        /// <returns>System.Collections.Generic.List&lt;UnrealSharpTool.Core.TypeInfo.Roslyn.CSharpSourceFile&gt;.</returns>
        public static List<CSharpSourceFile> LoadCompileTargetFiles(CSharpCodeBasedGenerateOptions options, string projectName, string csprojPath)
        {
            List<CSharpSourceFile> AllFiles = LoadProjectTypeDefinitionFiles(options.UnrealProjectDirectory!, projectName).ToList();
            AllFiles.AddRange(LoadProjectNativeBindingPlaceholders(options.UnrealProjectDirectory!, projectName));
            AllFiles.AddRange(LoadUnrealEngineBuiltinPlaceholders(options.UnrealProjectDirectory!));

            var DependencyProjectNames = CSharpProjectUtils.GetDependProjectNames(csprojPath);

            AllFiles.AddRange(LoadDependProjectRelatedFiles(options.UnrealProjectDirectory!, DependencyProjectNames));
            AllFiles.AddRange(LoadExtraUtilFiles(options.UnrealProjectDirectory!));
                        
            return AllFiles;
        }

        /// <summary>
        /// Gets all references.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>System.Collections.Generic.List&lt;Microsoft.CodeAnalysis.PortableExecutableReference&gt;.</returns>
        public static List<PortableExecutableReference> GetAllReferences(CSharpCodeBasedGenerateOptions options)
        {
            var References = GetSystemReferences(options).ToList();
            References.AddRange(GetUnrealSharpReferences(options.UnrealProjectDirectory));

            return References;
        }
        #endregion

        #region Diagnostics
        /// <summary>
        /// Logs the compilation diagnostics.
        /// </summary>
        /// <param name="compilation">The compilation.</param>
        /// <param name="sourceFiles">The source files.</param>
        /// <exception cref="UnrealSharpDefinitionException">sourceInfo.ToString(), sourceInfo</exception>
        public static void LogCompilationDiagnostics(Compilation compilation, Dictionary<SyntaxTree, CSharpSourceFile> sourceFiles)
        {
            string FileStartTag = System.Diagnostics.Debugger.IsAttached ? " >" : "";

            foreach (var diagnostic in compilation.GetDiagnostics())
            {
                var tree = diagnostic.Location.SourceTree;
                sourceFiles.TryGetValue(tree!, out var fileInfo);

                if (diagnostic.Severity == DiagnosticSeverity.Warning)
                {
                    Logger.LogWarning($"{FileStartTag}{(fileInfo?.FilePath ?? "Unknown File")}({diagnostic.Location.GetLineSpan().StartLinePosition.Line}) :{diagnostic.GetMessage()}");
                }
                else if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    Logger.LogError($"{FileStartTag}{(fileInfo?.FilePath ?? "Unknown File")}({diagnostic.Location.GetLineSpan().StartLinePosition.Line}) :{diagnostic.GetMessage()}");

                    var member = diagnostic.FindMemberDeclarationSyntax();
                    var identifierType = member != null ? member.GetSymbolIdentifierType() : SymbolIdentifierType.None;

                    var sourceInfo = new TypeDefinitionSourceInfo(
                        new SymbolIdentifier(identifierType, member?.GetIdentifierName() ?? "__UnkownName"),
                        new SymbolSourceInfo()
                        {
                            FilePath = fileInfo?.FilePath ?? "Unknown File",
                            Line = diagnostic.Location.GetLineSpan().StartLinePosition.Line
                        }
                    );

                    throw new UnrealSharpDefinitionException(sourceInfo.ToString() + " : " + diagnostic.GetMessage(), sourceInfo);
                }
            }
        }
        #endregion

        #region References
        /// <summary>
        /// Gets the system references.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>System.Collections.Generic.List&lt;Microsoft.CodeAnalysis.PortableExecutableReference&gt;.</returns>
        public static IEnumerable<PortableExecutableReference> GetSystemReferences(CSharpCodeBasedGenerateOptions options)
        {
            string[] baseReferences = [
                "System.Buffers.dll",
                "System.Collections.dll",
                "System.Collections.Concurrent.dll",
                "System.Collections.Immutable.dll",
                "System.ComponentModel.dll",
                "System.ComponentModel.Primitives.dll",
                "System.ComponentModel.TypeConverter.dll",
                "System.Console.dll",
                "System.Diagnostics.Tracing.dll",
                "System.Globalization.dll",
                "System.IO.dll",
                "System.IO.Compression.dll",
                "System.IO.Pipes.dll",
                "System.Linq.dll",
                "System.Memory.dll",
                "System.ObjectModel.dll",
                "System.Private.CoreLib.dll",
                "System.Private.Xml.dll",
                "System.Private.Xml.Linq.dll",
                "System.Reflection.dll",
                "System.Reflection.Emit.ILGeneration.dll",
                "System.Reflection.Emit.Lightweight.dll",
                "System.Reflection.Metadata.dll",
                "System.Reflection.Primitives.dll",
                "System.Runtime.dll",
                "System.Runtime.InteropServices.dll",
                "System.Runtime.Loader.dll",
                "System.Runtime.Numerics.dll",
                "System.Security.dll",
                "System.Security.AccessControl.dll",
                "System.Security.Claims.dll",
                "System.Security.Cryptography.dll",
                "System.Security.Principal.Windows.dll",
                "System.Text.Encoding.Extensions.dll",
                "System.Text.RegularExpressions.dll",
                "System.Threading.dll",
                "System.Threading.Overlapped.dll",
                "System.Threading.Tasks.Parallel.dll",
                "System.Xml.ReaderWriter.dll",
                "System.Xml.XDocument.dll",
                "System.Net.dll",
                "System.Net.Http.dll",
                "System.Drawing.dll",
                "System.Drawing.Primitives.dll",
                "System.Dynamic.Runtime.dll",
                "MicroSoft.CSharp.dll",
                "System.dll",
                "System.ComponentModel.dll",
                "System.ComponentModel.Annotations.dll",
                "System.ComponentModel.DataAnnotations.dll",
                "System.ComponentModel.TypeConverter.dll",
                "System.Configuration.dll",
                "System.Globalization.dll",
                "System.ObjectModel.dll",
                "System.Text.Json.dll",
                "System.Text.RegularExpressions.dll",
                "System.Threading.dll",
                "System.Xml.dll",
                "System.Private.Uri.dll"
            ];

            AssemblySearchOptions TempOptions = new AssemblySearchOptions();
            TempOptions.ConfigurationType = EConfigurationType.Release;
            TempOptions.ArchitectureType = EArchitectureType.X64;
            TempOptions.UnrealProjectDirectory = options.UnrealProjectDirectory;

            var searchPathes = TempOptions.GetConfigurationBasedRuntimeSearchPaths();

            List<PortableExecutableReference> references = new List<PortableExecutableReference>();

            foreach (var name in baseReferences)
            {
                bool bFound = false;

                foreach (var frameworkPath in searchPathes)
                {
                    var dllPath = Path.Combine(frameworkPath!, name);

                    if (dllPath.IsFileExists())
                    {
                        references.Add(MetadataReference.CreateFromFile(dllPath));
                        bFound = true;
                        break;
                    }
                }

                if(!bFound)
                {
                    throw new FileNotFoundException($"Failed find dependency dll:{name}");
                }                
            }

            return references;
        }

        /// <summary>
        /// Gets the unreal sharp references.
        /// </summary>
        /// <param name="unrealProjectDirectory">The unreal project directory.</param>
        /// <returns>System.Collections.Generic.IEnumerable&lt;Microsoft.CodeAnalysis.PortableExecutableReference&gt;.</returns>
        public static IEnumerable<PortableExecutableReference> GetUnrealSharpReferences(string unrealProjectDirectory)
        {
            string ManagedDirectoryPath = Path.Combine(unrealProjectDirectory, "Managed");

            if (ManagedDirectoryPath.IsDirectoryExists())
            {
                DateTime latestWriteTime = DateTime.MinValue;
                string? latestFile = null;

                foreach (var configOutputPath in Directory.GetDirectories(ManagedDirectoryPath))
                {
                    string UnrealSharpUtilsDllPath = Path.Combine(configOutputPath, "UnrealSharp.Utils.dll");

                    if (File.Exists(UnrealSharpUtilsDllPath))
                    {
                        var writeTime = File.GetLastWriteTime(UnrealSharpUtilsDllPath);
                        if (writeTime > latestWriteTime)
                        {
                            latestWriteTime = writeTime;
                            latestFile = UnrealSharpUtilsDllPath;
                        }
                    }
                }

                if (latestFile != null)
                {
                    return [MetadataReference.CreateFromFile(latestFile)];
                }
            }

            { 
                var UnrealSharpUtilsDllPath = Path.Combine(unrealProjectDirectory, "GameScripts/Tools/UnrealSharpTool/DotNET/UnrealSharp.Utils.dll");

                if (UnrealSharpUtilsDllPath.IsFileExists())
                {
                    return [MetadataReference.CreateFromFile(UnrealSharpUtilsDllPath)];
                }

                Logger.Ensure<FileNotFoundException>(UnrealSharpUtilsDllPath.IsFileExists(), $"UnrealSharp.Utils.dll is not exists. Please build UnrealSharp.sln first.");
            }            

            return [];            
        }
        #endregion

        #region Create Document
        /// <summary>
        /// Parses from directories.
        /// </summary>
        /// <param name="inputPath">The input path.</param>
        /// <param name="globalOptions">The global options.</param>
        /// <param name="debugInformation">The debug information.</param>
        /// <returns>UnrealSharpTool.Core.TypeInfo.TypeDefinitionDocument?.</returns>
        /// <exception cref="UnrealSharpDefinitionException">new TypeDefinitionSourceInfo() { Signature = diagnostic.GetMessage(), FilePath = diagnostic.Location.GetLineSpan().Path, LineNumber = diagnostic.Location.GetLineSpan().StartLinePosition.Line }, diagnostic.GetMessage()</exception>
        public static TypeDefinitionDocument? ParseFromDirectory(
            string inputPath, 
            ITypeDefinitionDocumentInitializeOptions? globalOptions,
            IDebugInformation debugInformation
            )
        {
            if (!inputPath.IsDirectoryExists())
            {
                Logger.LogError("Input directory {0} is not exists.", inputPath);
                return null;
            }

            string projectName = inputPath.TrimEnd('\\', '/').GetFileName();
            string csprojPath = Path.Combine(inputPath, projectName + ".csproj");
            Logger.Log("Target c# project:{0}", csprojPath);

            var Options = globalOptions as CSharpCodeBasedGenerateOptions;

            Logger.EnsureNotNull(Options);

            var AllFiles = LoadCompileTargetFiles(Options, projectName, csprojPath);
            Logger.LogD($"Compile {AllFiles.Count} sources:{Environment.NewLine}" + string.Join(Environment.NewLine, AllFiles.Select(x => "    " + x.FilePath.CanonicalPath())));

            var References = GetAllReferences(Options);

            Dictionary<SyntaxTree, CSharpSourceFile> TreeMaps = new Dictionary<SyntaxTree, CSharpSourceFile>();
            AllFiles.ForEach(x => TreeMaps.Add(x.Tree, x));

            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary, 
                nullableContextOptions: NullableContextOptions.Enable,
                allowUnsafe:true,
                specificDiagnosticOptions: new Dictionary<string, ReportDiagnostic>
                {
                    { "CS0649", ReportDiagnostic.Suppress },  // Suppress these warnings because, in theory, they only exist as generated code
                    { "CS0169", ReportDiagnostic.Suppress }
                }
                );

            // cache syntax tree maps            
            var compilation = CSharpCompilation.Create("Compilation", AllFiles.Select(x=>x.Tree), References, options);
            
            LogCompilationDiagnostics(compilation, TreeMaps);

            Logger.LogD("Compile definition code success, start generate document, please wait...");

            var dllpath = GenerateTempOutputFile(Options, projectName, compilation, TreeMaps);

            return GenerateDocumentFromAssembly(Options, AllFiles, dllpath);
        }

        /// <summary>
        /// Generates the document from assembly.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="allFiles">All files.</param>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns>UnrealSharpTool.Core.TypeInfo.TypeDefinitionDocument?.</returns>
        private static TypeDefinitionDocument? GenerateDocumentFromAssembly(CSharpCodeBasedGenerateOptions options, IList<CSharpSourceFile> allFiles, string assemblyPath)
        {
            Logger.Ensure<FileNotFoundException>(assemblyPath.IsFileExists(), $"Failed find temp assembly file:{assemblyPath}");

            var LocalAssemblySearchOptions = new AssemblySearchOptions() 
            { 
                IgnoreBindingDefinition = false,
                UnrealProjectDirectory = options.UnrealProjectDirectory
            };

            LocalAssemblySearchOptions.CustomSearchDirectories.Add(Path.Combine(options.UnrealProjectDirectory, "Managed"));

            var newDebugInformation = new RoslynDebugInformation(allFiles.Select(x => x.FilePath));

            var document = MonoCecil.MonoTypeDefinitionDocumentFactory.LoadFromAssemblies(
                [assemblyPath],
                LocalAssemblySearchOptions,
                newDebugInformation
                );

            return document;
        }

        /// <summary>
        /// Generates the temporary output file.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="compilation">The compilation.</param>
        /// <param name="treeMaps">The tree maps.</param>
        /// <returns>string.</returns>
        private static string GenerateTempOutputFile(CSharpCodeBasedGenerateOptions options, string projectName, CSharpCompilation compilation, Dictionary<SyntaxTree, CSharpSourceFile> treeMaps)
        {
            string IntermediatePath = Path.Combine(options.UnrealProjectDirectory!, "Intermediate/UnrealSharp");
            if (!IntermediatePath.IsDirectoryExists())
            {
                Directory.CreateDirectory(IntermediatePath);
            }

            string TempName = projectName;

            var dllPath = Path.Combine(IntermediatePath, $"{TempName}.Bindings.Def.dll");
            var pdbPath = Path.Combine(IntermediatePath, $"{TempName}.Bindings.Def.pdb");

            using (var dllStream = new FileStream(dllPath, FileMode.Create))
            {
                using (var pdbStream = new FileStream(pdbPath, FileMode.Create))
                {
                    var emitOptions = new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb);
                    var result = compilation.Emit(dllStream, pdbStream, options: emitOptions);

                    if (!result.Success)
                    {
                        LogCompilationDiagnostics(compilation, treeMaps);
                    }
                }
            }

            return dllPath;
        }
        #endregion
    }
}
