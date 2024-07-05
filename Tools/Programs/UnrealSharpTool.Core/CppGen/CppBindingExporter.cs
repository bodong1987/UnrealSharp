using System.Text;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.CodeGen;
using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Core.CppGen;

public class CppBindingExporter
{
    /// <summary>
    /// The context
    /// </summary>
    public readonly BindingContext Context;

    // ReSharper disable once CollectionNeverQueried.Global
    public readonly Dictionary<ClassTypeDefinition, List<FunctionTypeDefinition>> ClassToFunctionMapping = new();
    public readonly HashSet<FunctionTypeDefinition> ExportableFunctions = [];
    public readonly List<FunctionTypeDefinition> UnsupportedFunctions = [];
    public readonly List<List<KeyValuePair<ClassTypeDefinition, List<FunctionTypeDefinition>>>> ExportGroupList =
        [];

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
    /// <returns><c>true</c> if success, <c>false</c> otherwise.</returns>
    public bool Export()
    {
        Logger.Log("Find Fast Accessible {0} methods, unsupported {1} methods", ExportableFunctions.Count, UnsupportedFunctions.Count);

        {
            using var scopedExporter = new ScopedExporter(Context.UnrealProjectDirectory, RootDirectory);
            scopedExporter.Extensions = [".h", ".cpp", ".inl", ".cxx"];
            scopedExporter.OnDeleteFileDelegate = _ => { ++DeleteFileCount; };

            for (var index = 0; index < ExportGroupList.Count; ++index)
            {
                var target = ExportGroupList[index];

                var implementWriter = new CppBindingCodeWriter($"{RootDirectory}/Details/{FileNameTemplate}_{index}.cpp");

                WriteImplement(implementWriter, target, index);

                if (implementWriter.Save() == ECodeWriterSaveResult.Success)
                {
                    ++DirtyFileCount;
                    Logger.Log("Export C++ file:{0}", implementWriter.TargetPath!);
                }

                ++TotalFileCount;

                scopedExporter.AddFile(implementWriter.TargetPath!);
            }

            var sharedHeadWriter = new CppBindingCodeWriter($"{RootDirectory}/{FileNameTemplate}.h");
            var sharedImplementWriter = new CppBindingCodeWriter($"{RootDirectory}/{FileNameTemplate}.cpp");

            WriteSharedHeader(sharedHeadWriter);
            WriteSharedImplementation(sharedImplementWriter);

            if (sharedHeadWriter.Save() == ECodeWriterSaveResult.Success)
            {
                ++DirtyFileCount;

                Logger.Log("Export C++ file:{0}", sharedHeadWriter.TargetPath!);
            }

            ++TotalFileCount;

            if (sharedImplementWriter.Save() == ECodeWriterSaveResult.Success)
            {
                ++DirtyFileCount;

                Logger.Log("Export C++ file:{0}", sharedImplementWriter.TargetPath!);
            }

            ++TotalFileCount;

            scopedExporter.AddFile(sharedHeadWriter.TargetPath!);
            scopedExporter.AddFile(sharedImplementWriter.TargetPath!);
        }

        Logger.Log($"C++ binding code export completed, {DirtyFileCount} Changed Files, Delete {DeleteFileCount} outdated files, Skip {TotalFileCount-DirtyFileCount} Files[No Changes], Total {TotalFileCount} Files.");
        if(DirtyFileCount > 0 || DeleteFileCount > 0)
        {
            Logger.LogWarning("Please note that the C++ code has changed. Please be sure to recompile the C++ project, otherwise the binding may fail when starting the game.");
        }

        return true;
    }

    #region Base Data Utils
    private static string GetClassTypeIncludePath(ClassTypeDefinition classTypeDefinition)
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
                return includePath["Public/".Length..];
            }

            if (includePath.StartsWith("Classes/"))
            {
                return includePath["Classes/".Length..];
            }
        }

        return "";
    }

    private static List<string> GetIncludes(IEnumerable<ClassTypeDefinition> classes)
    {
        var includes = new HashSet<string>();

        foreach (var item in classes)
        {
            var inc = GetClassTypeIncludePath(item);

            if(inc.IsNotNullOrEmpty())
            {
                includes.Add(inc);
            }
        }

        var list = includes.ToList();
        list.Sort();
        return list;
    }

    private static string GetFunctionName(ClassTypeDefinition classTypeDefinition, FunctionTypeDefinition functionTypeDefinition)
    {
        return $"{classTypeDefinition.CppName}_{functionTypeDefinition.FunctionName}";
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

        var builder = new StringBuilder();

        const ELocalUsageScenarioType usage = ELocalUsageScenarioType.Class | ELocalUsageScenarioType.Method | ELocalUsageScenarioType.Parameter;

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

        return functionTypeDefinition.IsStatic ? commonParams : $"{classTypeDefinition.CppName}* __this{(commonParams.IsNullOrEmpty()?"":", ")}{commonParams}";
    }
    #endregion

    #region Implement Write
    private void WriteImplement(CppBindingCodeWriter writer, List<KeyValuePair<ClassTypeDefinition, List<FunctionTypeDefinition>>> targets, int index)
    {
        // disable resharper warnings for generated codes.
        writer.Write("// ReSharper disable all");
        writer.Write("#include \"CoreMinimal.h\"");
        writer.Write("#include \"Misc/UnrealInteropFunctions.h\"");
        writer.Write("#include \"Misc/CSharpStructures.h\"");

        var includes = GetIncludes(targets.Select(x => x.Key));
        foreach (var i in includes)
        {
            writer.Write($"#include \"{i}\"");
        }
            
        writer.WriteNewLine();

        // ignore invoke deprecated function warnings
        // because I don't know the deprecated state of UFunction precisely
        writer.Write("#if PLATFORM_WINDOWS");
        writer.Write("#pragma warning(push)");
        writer.Write("#pragma warning(disable:4996)");
        writer.Write("#else");
        writer.Write("#pragma GCC diagnostic push");
        // ReSharper disable once StringLiteralTypo
        writer.Write("#pragma GCC diagnostic ignored \"-Wdeprecated\"");
        writer.Write("#endif");

        writer.WriteNewLine();

        writer.Write("namespace UnrealSharp::Bindings");
        {
            using var scopedCodeWriter = new ScopedCodeWriter(writer);

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
                using var functionScope = new ScopedCodeWriter(writer);

                writer.Write("check(InInteropFunctions);");
                writer.WriteNewLine();

                foreach (var functionName in from pair in targets from function in pair.Value select GetFunctionName(pair.Key, function))
                {
                    writer.Write($"US_ADD_GENERATED_GLOBAL_INTEROP_FUNCTION_HELPER({functionName});");
                }
            }
        }

        writer.Write("#if PLATFORM_WINDOWS");
        writer.Write("#pragma warning(pop)");
        writer.Write("#else");
        writer.Write("#pragma GCC diagnostic pop");
        writer.Write("#endif");
        writer.Write("// ReSharper restore all");
        writer.WriteNewLine();
    }

    private void WriteFunctionImplementation(CppBindingCodeWriter writer, ClassTypeDefinition classTypeDefinition, FunctionTypeDefinition functionTypeDefinition)
    {            
        var methodNameText = $"{classTypeDefinition.CppName}_{functionTypeDefinition.FunctionName}";
        var paramDeclarationText = GetFunctionDeclarationParamList(classTypeDefinition, functionTypeDefinition);

        writer.Write($"void {methodNameText}({paramDeclarationText})");
        {
            using var scopedCodeWriter = new ScopedCodeWriter(writer);

            var returnFlag = functionTypeDefinition.HasReturnType() ? "__result = " : "";
            var invokeParamList = GetFunctionInvokeParamList(functionTypeDefinition);

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
            using var declareScope = new ScopedCodeWriter(writer);
            writer.Write("class FUnrealInteropFunctions;");
        }

        writer.WriteNewLine();

        writer.Write("namespace UnrealSharp::Bindings");
        {
            using var scopedCodeWriter = new ScopedCodeWriter(writer);

            writer.Write("extern void RegisterFastInvokeApis(FUnrealInteropFunctions* InInteropFunctions);");
        }

        // write sub game system
        writer.WriteNewLine();
        writer.Write("UCLASS()");
        writer.Write($"class U{Context.UnrealProjectName}CSharpInteropsGameInstanceSubsystem : public UGameInstanceSubsystem");
        {
            using var classScope = new ScopedCodeWriter(writer, true, true);

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
            using var scopedCodeWriter = new ScopedCodeWriter(writer);

            {
                for (var i = 0; i < ExportGroupList.Count; ++i)
                {
                    writer.Write($"extern void RegisterFastInvokeApis_{i}(FUnrealInteropFunctions* InInteropFunctions);");
                }
            }                

            writer.WriteNewLine();

            writer.Write("void RegisterFastInvokeApis(FUnrealInteropFunctions* InInteropFunctions)");
            {
                using var registerScope = new ScopedCodeWriter(writer);

                for(var i = 0; i < ExportGroupList.Count; ++i)
                {
                    writer.Write($"RegisterFastInvokeApis_{i}(InInteropFunctions);");
                }
            }
        }

        writer.WriteNewLine();

        writer.Write($"void U{Context.UnrealProjectName}CSharpInteropsGameInstanceSubsystem::Initialize(FSubsystemCollectionBase& Collection)");
        {
            using var funcScope = new ScopedCodeWriter(writer);

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

                var supportedFunctionOfClass = new List<FunctionTypeDefinition>();

                Logger.EnsureNotNull(classType, $"{type} should be ClassTypeDefinition");

                foreach(var function in classType.Functions)
                {
                    if(IsCppExportableFunction(classType, function))
                    {
                        ExportableFunctions.Add(function);
                        supportedFunctionOfClass.Add(function);
                    }
                    else
                    {
                        UnsupportedFunctions.Add(function);
                    }
                }

                if(supportedFunctionOfClass.Count > 0)
                {
                    ClassToFunctionMapping.Add(classType, supportedFunctionOfClass);

                    if (ExportGroupList.Count <= 0 || ExportGroupList.Last().Sum(x=>x.Value.Count) + supportedFunctionOfClass.Count > MaxCountInOneFile)
                    {
                        ExportGroupList.Add([]);
                    }

                    var batchedList = ExportGroupList.Last();

                    Logger.Assert(batchedList.Sum(x => x.Value.Count) < MaxCountInOneFile);

                    batchedList.Add(new KeyValuePair<ClassTypeDefinition, List<FunctionTypeDefinition>>(classType, supportedFunctionOfClass));
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
            return property is { IsReturnParam: false, IsOutParam: false, IsReference: false };
        }

        return property.IsStruct &&
               // need support fast marshaller
               Context.Document.IsFastAccessStructType(property.CppTypeName!);
    }
    #endregion
}