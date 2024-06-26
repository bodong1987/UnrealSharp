using System.Security.Principal;
using System.Text;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;
using UnrealSharpTool.Core.CodeGen;
using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Core.CppGen
{
    public class CppBindingExporter
    {
        /// <summary>
        /// The context
        /// </summary>
        public readonly BindingContext Context;

        public readonly Dictionary<ClassTypeDefinition, List<FunctionTypeDefinition>> ClassToFunctionMappping = new Dictionary<ClassTypeDefinition, List<FunctionTypeDefinition>>();
        public readonly HashSet<FunctionTypeDefinition> ExportableFunctions = new HashSet<FunctionTypeDefinition>();
        public readonly List<FunctionTypeDefinition> UnsupportedFunctions = new List<FunctionTypeDefinition>();
        public readonly List<List<KeyValuePair<ClassTypeDefinition, List<FunctionTypeDefinition>>>> ExportGroupList = new List<List<KeyValuePair<ClassTypeDefinition, List<FunctionTypeDefinition>>>>();

        public const int MaxCountInOneFile = 800;
        public const string FolderName = "UnrealSharpBinding";
        public const string FileNameTemplate = "UnrealSharpInvokeBinding";

        public readonly string RootDirectory;

        public int DirtyFileCount { get; private set; }

        public int TotalFileCount { get; private set; }

        public int DeleteFileCount { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CppBindingExporter"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public CppBindingExporter(BindingContext context)
        {
            Logger.Ensure<Exception>(context.SchemaType == EBindingSchemaType.NativeBinding, "CppBindingExporter is only available with SchemeType == NativeBinding.");

            Context = context;

            RootDirectory = Path.Combine(Context.UnrealProjectDirectory, $"Source/{Context.UnrealProjectName}/{FolderName}");

            BuildExportableFunctions();
        }

        #region Export
        /// <summary>
        /// Exports this instance.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Export()
        {
            Logger.Log("Find Fast accessable {0} methods, unsupported {1} methods", ExportableFunctions.Count, UnsupportedFunctions.Count);

            {
                using ScopedExporter scopedExporter = new ScopedExporter(Context.UnrealProjectDirectory, RootDirectory) { Extensions = [".h", ".cpp", ".inl", ".cxx"] };
                scopedExporter.OnDeleteFileDelegate = file => { ++DeleteFileCount; };

                for (int index = 0; index < ExportGroupList.Count; ++index)
                {
                    var Target = ExportGroupList[index];

                    CppBindingCodeWriter ImplementWriter = new CppBindingCodeWriter($"{RootDirectory}/Details/{FileNameTemplate}_{index}.cpp");

                    WriteImplement(ImplementWriter, Target, index);

                    if (ImplementWriter.Save() == ECodeWriterSaveResult.Success)
                    {
                        ++DirtyFileCount;
                        Logger.Log("Export C++ file:{0}", ImplementWriter.TargetPath!);
                    }

                    ++TotalFileCount;

                    scopedExporter.AddFile(ImplementWriter.TargetPath!);
                }

                CppBindingCodeWriter SharedHeadWriter = new CppBindingCodeWriter($"{RootDirectory}/{FileNameTemplate}.h");
                CppBindingCodeWriter SharedImplementWriter = new CppBindingCodeWriter($"{RootDirectory}/{FileNameTemplate}.cpp");

                WriteSharedHeader(SharedHeadWriter);
                WriteSharedImplementation(SharedImplementWriter);

                if (SharedHeadWriter.Save() == ECodeWriterSaveResult.Success)
                {
                    ++DirtyFileCount;

                    Logger.Log("Export C++ file:{0}", SharedHeadWriter.TargetPath!);
                }

                ++TotalFileCount;

                if (SharedImplementWriter.Save() == ECodeWriterSaveResult.Success)
                {
                    ++DirtyFileCount;

                    Logger.Log("Export C++ file:{0}", SharedImplementWriter.TargetPath!);
                }

                ++TotalFileCount;

                scopedExporter.AddFile(SharedHeadWriter.TargetPath!);
                scopedExporter.AddFile(SharedImplementWriter.TargetPath!);
            }

            Logger.Log($"C++ binding code export completed, {DirtyFileCount} Changed Files, Delete {DeleteFileCount} outdated files, Skip {TotalFileCount-DirtyFileCount} Files[No Changes], Total {TotalFileCount} Files.");
            if(DirtyFileCount > 0 || DeleteFileCount > 0)
            {
                Logger.LogWarning("Please note that the C++ code has changed. Please be sure to recompile the C++ project, otherwise the binding may fail when starting the game.");
            }

            return true;
        }

        #region Base Data Utils
        private string GetClassTypeIncludePath(ClassTypeDefinition classTypeDefinition)
        {
            var includePath = classTypeDefinition.GetMeta("IncludePath");

            if(includePath.IsNotNullOrEmpty())
            {
                return includePath;
            }

            includePath = classTypeDefinition.GetMeta("ModuleRelativePath");

            if(includePath.IsNotNullOrEmpty())
            {
                if(includePath.StartsWith("Public/"))
                {
                    return includePath.Substring("Public/".Length);
                }
                else if (includePath.StartsWith("Classes/"))
                {
                    return includePath.Substring("Classes/".Length);
                }
            }

            return "";
        }

        private IEnumerable<string> GetIncludes(IEnumerable<ClassTypeDefinition> classes)
        {
            HashSet<string> Includes = new HashSet<string>();

            foreach (var item in classes)
            {
                var inc = GetClassTypeIncludePath(item);

                if(inc.IsNotNullOrEmpty())
                {
                    Includes.Add(inc);
                }
            }

            var list = Includes.ToList();
            list.Sort();
            return list;
        }

        private string GetFunctionName(ClassTypeDefinition classTypeDefinition, FunctionTypeDefinition functionTypeDefinition)
        {
            return $"{classTypeDefinition.CppName}_{functionTypeDefinition.FunctionName}";
        }

        private string GetFunctionReturnType(FunctionTypeDefinition functionTypeDefinition)
        {
            var returnType = functionTypeDefinition.GetReturnType();

            if(returnType == null)
            {
                return "void";
            }

            var cppName = returnType.CppTypeName!;

            return returnType.IsConstParam ? $"const {cppName}" : cppName;
        }

        private string GetFunctionDeclarationParamList(FunctionTypeDefinition functionTypeDefinition)
        {
            if(functionTypeDefinition.Properties.Count == 0)
            {
                return "";
            }

            var properties = functionTypeDefinition.Properties.Where(x => !x.IsReturnParam).ToList();
            var returnProperty = functionTypeDefinition.GetReturnType();

            if(returnProperty != null)
            {
                properties.Add(returnProperty);
            }

            StringBuilder builder = new StringBuilder();

            ELocalUsageScenarioType usage = ELocalUsageScenarioType.Class | ELocalUsageScenarioType.Method | ELocalUsageScenarioType.Parameter;

            foreach (var param in properties)
            {
                var processor = Context.GetProcessor(param);
                Logger.EnsureNotNull(processor);

                var typeName = processor.GetCppFunctionDeclarationType(param, param == returnProperty ? usage | ELocalUsageScenarioType.ReturnValue : usage);

                var paramName = param == returnProperty ? "__result" : param.Name;

                if(builder.Length > 0)
                {
                    builder.Append($", {typeName} {paramName}");
                }
                else
                {
                    builder.Append($"{typeName} {paramName}");
                }                
            }

            return builder.ToString();
        }

        private string GetFunctionDeclarationParamList(ClassTypeDefinition classTypeDefinition, FunctionTypeDefinition functionTypeDefinition)
        {
            var commonParams = GetFunctionDeclarationParamList(functionTypeDefinition);

            if (functionTypeDefinition.IsStatic)
            {
                return commonParams;
            }
            else
            {
                return $"{classTypeDefinition.CppName}* __this{(commonParams.IsNullOrEmpty()?"":", ")}{commonParams}";
            }
        }
        #endregion

        #region Implement Write
        private void WriteImplement(CppBindingCodeWriter writer, List<KeyValuePair<ClassTypeDefinition, List<FunctionTypeDefinition>>> targets, int index)
        {
            writer.Write("#include \"CoreMinimal.h\"");
            writer.Write($"#include \"Misc/UnrealInteropFunctions.h\"");
            writer.Write($"#include \"Misc/CSharpStructures.h\"");

            var Includes = GetIncludes(targets.Select(x => x.Key));
            foreach (var i in Includes)
            {
                writer.Write($"#include \"{i}\"");
            }
            
            writer.WriteNewLine();

            // ignore invoke deprecated function warnings
            // because i can't known the deprecated state of UFunction precisely
            writer.Write("#if PLATFORM_WINDOWS");
            writer.Write("#pragma warning(push)");
            writer.Write("#pragma warning(disable:4996)");
            writer.Write("#else");
            writer.Write("#pragma GCC diagnostic push");
            writer.Write("#pragma GCC diagnostic ignored \"-Wdeprecated\"");
            writer.Write("#endif");

            writer.WriteNewLine();

            writer.Write("namespace UnrealSharp::Bindings");
            {
                using ScopedCodeWriter scopedCodeWriter = new ScopedCodeWriter(writer);

                {
                    foreach (var pair in targets)
                    {
                        foreach (var function in pair.Value)
                        {
                            WriteFunctionImplementation(writer, pair.Key, function);
                        }
                    }
                }                

                writer.WriteNewLine();

                writer.WriteCommonComment("Export register function");
                writer.Write($"void RegisterFastInvokeApis_{index}(FUnrealInteropFunctions* InInteropFunctions)");
                {
                    using ScopedCodeWriter functionScope = new ScopedCodeWriter(writer);

                    writer.Write("check(InInteropFunctions);");
                    writer.WriteNewLine();

                    foreach (var pair in targets)
                    {
                        foreach (var function in pair.Value)
                        {
                            var functionName = GetFunctionName(pair.Key, function);

                            writer.Write($"US_ADD_GENERATED_GLOBAL_INTEROPFUNCTION_HELPER({functionName});");
                        }
                    }
                }
            }

            writer.Write("#if PLATFORM_WINDOWS");
            writer.Write("#pragma warning(pop)");
            writer.Write("#else");
            writer.Write("#pragma GCC diagnostic pop");
            writer.Write("#endif");
            writer.WriteNewLine();
        }

        private void WriteFunctionImplementation(CppBindingCodeWriter writer, ClassTypeDefinition classTypeDefinition, FunctionTypeDefinition functionTypeDefinition)
        {            
            string methodNameText = $"{classTypeDefinition.CppName}_{functionTypeDefinition.FunctionName}";
            string paramDeclarationText = GetFunctionDeclarationParamList(classTypeDefinition, functionTypeDefinition);

            writer.Write($"void {methodNameText}({paramDeclarationText})");
            {
                using ScopedCodeWriter scopedCodeWriter = new ScopedCodeWriter(writer);

                string returnFlag = functionTypeDefinition.HasReturnType() ? "__result = " : "";
                string invokeParamList = GetFunctionInvokeParamList(functionTypeDefinition);

                if(functionTypeDefinition.IsStatic)
                {
                    writer.Write($"{returnFlag}{classTypeDefinition.CppName}::{functionTypeDefinition.FunctionName}({invokeParamList});");
                }
                else
                {
                    writer.Write("checkSlow(__this != nullptr);");
                    writer.Write($"{returnFlag}__this->{functionTypeDefinition.FunctionName}({invokeParamList});");
                }                
            }

            writer.WriteNewLine();
        }

        private string GetFunctionInvokeParamList(FunctionTypeDefinition functionTypeDefinition)
        {
            if (functionTypeDefinition.Properties.Count == 0 ||
                (functionTypeDefinition.HasReturnType() && functionTypeDefinition.Properties.Count == 1))
            {
                return "";
            }

            return string.Join(", ", functionTypeDefinition.Properties.FindAll(x => !x.IsReturnParam).Select(x =>
            {
                var processor = Context.GetProcessor(x);
                Logger.EnsureNotNull(processor);

                return processor.GetCppFunctionInvokeParameterName(x);
            }
            ));
        }
        #endregion

        #region Shared Header
        private void WriteSharedHeader(CppBindingCodeWriter writer)
        {
            writer.Write("#pragma once");

            writer.WriteNewLine();
            writer.Write("#include \"CoreMinimal.h\"");
            writer.Write("#include \"Subsystems/GameInstanceSubsystem.h\"");
            writer.Write($"#include \"{FileNameTemplate}.generated.h\"");

            writer.WriteNewLine();
            writer.Write("namespace UnrealSharp");
            {
                using ScopedCodeWriter declareScope = new ScopedCodeWriter(writer);
                writer.Write("class FUnrealInteropFunctions;");
            }

            writer.WriteNewLine();

            writer.Write("namespace UnrealSharp::Bindings");
            {
                using ScopedCodeWriter scopedCodeWriter = new ScopedCodeWriter(writer);

                writer.Write("extern void RegisterFastInvokeApis(FUnrealInteropFunctions* InInteropFunctions);");
            }

            // write sub game system
            writer.WriteNewLine();
            writer.Write("UCLASS()");
            writer.Write($"class U{Context.UnrealProjectName}CSharpInteropsGameInstanceSubsystem : public UGameInstanceSubsystem");
            {
                using ScopedCodeWriter classScope = new ScopedCodeWriter(writer, true, true);

                writer.Write("GENERATED_BODY()");

                writer.WriteNewLine();

                writer.Write("virtual void Initialize(FSubsystemCollectionBase& Collection) override;");                
            }
        }
        #endregion

        #region Shared Implementation
        private void WriteSharedImplementation(CppBindingCodeWriter writer)
        {
            writer.Write($"#include \"{Context.UnrealProjectName}/{FolderName}/{FileNameTemplate}.h\"");
            writer.Write("#include \"Classes/CSharpRuntimeGameInstanceSubsystem.h\"");
            writer.Write("#include \"IUnrealSharpModule.h\"");
            writer.WriteNewLine();

            writer.Write("namespace UnrealSharp::Bindings");
            {
                using ScopedCodeWriter scopedCodeWriter = new ScopedCodeWriter(writer);

                {
                    for (int i = 0; i < ExportGroupList.Count; ++i)
                    {
                        writer.Write($"extern void RegisterFastInvokeApis_{i}(FUnrealInteropFunctions* InInteropFunctions);");
                    }
                }                

                writer.WriteNewLine();

                writer.Write("void RegisterFastInvokeApis(FUnrealInteropFunctions* InInteropFunctions)");
                {
                    using ScopedCodeWriter registerScope = new ScopedCodeWriter(writer);

                    for(int i = 0; i < ExportGroupList.Count; ++i)
                    {
                        writer.Write($"RegisterFastInvokeApis_{i}(InInteropFunctions);");
                    }
                }
            }

            writer.WriteNewLine();

            writer.Write($"void U{Context.UnrealProjectName}CSharpInteropsGameInstanceSubsystem::Initialize(FSubsystemCollectionBase& Collection)");
            {
                using ScopedCodeWriter funcScope = new ScopedCodeWriter(writer);

                writer.Write("Collection.InitializeDependency(UCSharpRuntimeGameInstanceSubsystem::StaticClass());");

                writer.WriteNewLine();
                writer.Write("UnrealSharp::Bindings::RegisterFastInvokeApis(IUnrealSharpModule::Get().GetInteropFunctions());");
            }
        }

        #endregion

        #endregion

        #region Builder
        private void BuildExportableFunctions()
        {
            foreach(var type in Context.Types)
            {
                if(type.IsClass)
                {
                    var classType = type as ClassTypeDefinition;

                    if(!Context.Document.IsModuleFastInvokeSupported(classType!.PackageName!))
                    {
                        continue;
                    }

                    List<FunctionTypeDefinition> SupportedFunctionOfClass = new List<FunctionTypeDefinition>();

                    Logger.EnsureNotNull(classType, $"{type} should be ClassTypeDefinition");

                    foreach(var function in classType!.Functions)
                    {
                        if(IsCppExportableFunction(classType, function))
                        {
                            ExportableFunctions.Add(function);
                            SupportedFunctionOfClass.Add(function);
                        }
                        else
                        {
                            UnsupportedFunctions.Add(function);
                        }
                    }

                    if(SupportedFunctionOfClass.Count > 0)
                    {
                        ClassToFunctionMappping.Add(classType, SupportedFunctionOfClass);

                        if (ExportGroupList.Count <= 0 || ExportGroupList.Last().Sum(x=>x.Value.Count) + SupportedFunctionOfClass.Count > MaxCountInOneFile)
                        {
                            ExportGroupList.Add(new ());
                        }

                        var BatchedList = ExportGroupList.Last();

                        Logger.Assert(BatchedList.Sum(x => x.Value.Count) < MaxCountInOneFile);

                        BatchedList.Add(new KeyValuePair<ClassTypeDefinition, List<FunctionTypeDefinition>>(classType, SupportedFunctionOfClass));
                    }
                }
            }
        }

        public bool IsCppExportableFunction(ClassTypeDefinition classTypeDefinition, FunctionTypeDefinition function)
        {
            return function.IsPublic &&
                !function.FunctionName.EndsWith("__DelegateSignature") &&     
                function.GetMeta("BlueprintInternalUseOnly").IsNullOrEmpty() &&
                !Context.Document.IsFastInvokeIgnore(classTypeDefinition.CppName!, function.FunctionName) &&
                function.Properties.All(IsCppExportableProperty);
        }

        public bool IsCppExportableProperty(PropertyDefinition property)
        {
            if(property.IsUObject ||
                property.IsClass ||
                property.IsBoolean ||
                property.IsEnum ||
                property.IsNativeBoolean ||
                property.IsNumeric ||
                property.IsName
                )
            {
                return true;
            }

            if(property.IsString)
            {
                // reference or return value of string is not supported.
                return !property.IsReturnParam && !property.IsOutParam && !property.IsReference;
            }

            if(property.IsStruct)
            {
                // need support fast marshaller
                return Context.Document.IsFastAccessStructType(property.CppTypeName!);
            }

            return false;
        }
        #endregion
    }
}
