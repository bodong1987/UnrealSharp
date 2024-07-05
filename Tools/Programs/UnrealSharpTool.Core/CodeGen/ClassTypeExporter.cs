using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;
using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;
using UnrealSharpTool.Core.Utils;
// ReSharper disable ArrangeRedundantParentheses

namespace UnrealSharpTool.Core.CodeGen;

/// <summary>
/// Class ClassTypeExporter.
/// Implements the <see cref="UnrealSharpTool.Core.CodeGen.StructTypeExporter" />
/// </summary>
/// <seealso cref="UnrealSharpTool.Core.CodeGen.StructTypeExporter" />
public class ClassTypeExporter : StructTypeExporter
{
    #region Properties
    /// <summary>
    /// The class type
    /// </summary>
    public readonly ClassTypeDefinition ClassType;

    /// <summary>
    /// The reserve names
    /// </summary>
    public static readonly HashSet<string> ReserveNames =
    [
        "ToString",
        "GetClass"
    ];
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassTypeExporter"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="targetDirectory">The target directory.</param>
    /// <param name="typeDefinition">The type definition.</param>
    public ClassTypeExporter(BindingContext context, string targetDirectory, ClassTypeDefinition typeDefinition) : 
        base(context, targetDirectory, typeDefinition)
    {
        ClassType = typeDefinition;

        if(ClassType.IsInterface)
        {
            TargetFile = Path.Combine(targetDirectory, $"{GetInterfaceName(typeDefinition.CppName!)}.gen.cs");
        }            
    }
    #endregion

    #region Misc
    /// <summary>
    /// Gets the name of the super.
    /// </summary>
    /// <returns>System.String.</returns>
    public string GetSuperName()
    {
        return ClassType.IsInterface ? GetInterfaceName(ClassType.SuperName) : ClassType.SuperName;
    }

    /// <summary>
    /// Gets the name of the interface.
    /// </summary>
    /// <param name="cppName">Name of the CPP.</param>
    /// <returns>System.String.</returns>
    public static string GetInterfaceName(string cppName)
    {
        // ReSharper disable once MergeIntoLogicalPattern
        return cppName == "UObject" || cppName == "UInterface" ? "IUObjectInterface" : "I" + cppName[1..];
    }
    #endregion

    #region Template Methods Override
    /// <summary>
    /// Queries the base type main comment text.
    /// </summary>
    /// <returns>System.String.</returns>
    protected override string QueryBaseTypeMainCommentText()
    {
        if(ClassType.IsInterface)
        {
            return $"Interface {Type.CppName}{Environment.NewLine}" +
                   base.QueryBaseTypeMainCommentText() +
                   $"Implements the <see cref=\"{GetSuperName()}\" />{Environment.NewLine}";
        }

        return $"Class {Type.CppName}{Environment.NewLine}" +
               base.QueryBaseTypeMainCommentText() +
               $"Implements the <see cref=\"{ClassType.SuperName}\" />{Environment.NewLine}";
    }

    /// <summary>
    /// Writes the main comment text.
    /// </summary>
    protected override void WriteMainCommentText()
    {
        base.WriteMainCommentText();

        Writer.WriteComment("", "seealso", $"cref=\"{GetSuperName()}\"");
        foreach(var i in ClassType.Interfaces)
        {
            Writer.WriteComment("", "seealso", $"cref=\"{GetInterfaceName(i)}\"");
        }
    }

    /// <summary>
    /// Exports the internal.
    /// </summary>
    /// <returns><c>true</c> if export success, <c>false</c> otherwise.</returns>
    protected override bool ExportInternal()
    {
        var needExportInterface = IsCSharpBinding;

        var extraNativeInterface = string.Join(", ", ClassType.Interfaces.Select(GetInterfaceName)); 
        var interfaceTag = needExportInterface ? $", I{Type.Name}" : "";

        if(extraNativeInterface.Length > 0)
        {
            interfaceTag = interfaceTag + ", " + extraNativeInterface;
        }

        if(IsNativeBinding)
        {
            Writer.Write(ClassType.IsInterface
                ? $"[NativeBinding(\"{Type.Name}\", \"{Type.CppName}\", \"{Type.PathName}\")]"
                : $"[NativeBinding(\"{Type.Name}\", \"{Type.CppName}\", {Type.Name}Path)]");
        }
        else if(IsBlueprintBinding)
        {
            Writer.Write(ClassType.IsInterface
                ? $"[BlueprintBinding(\"{Type.PathName}\")]"
                : $"[BlueprintBinding({Type.CppName}.{Type.Name}Path)]");
        }

        Writer.Write("[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]");

        WriteAttributes((EClassFlags)ClassType.Flags, "UCLASS", ClassType.Metas, ()=>
        {
            List<string> results = [];
            if (ClassType.ConfigName.IsNotNullOrEmpty())
            {
                results.Add($"Config = \"{ClassType.ConfigName}\"");
            }

            if(ClassType.Guid.IsNotNullOrEmpty() && ClassType.Guid != "00000000-0000-0000-0000-000000000000")
            {
                results.Add($"Guid = \"{ClassType.Guid}\"");
            }

            return results;
        });

        Writer.Write(ClassType.IsInterface
            ? $"{BaseTypeAccessPermission} partial interface {GetInterfaceName(Type.CppName!)} : {GetSuperName()}{interfaceTag}"
            : $"{BaseTypeAccessPermission} partial class {Type.CppName} : {ClassType.SuperName}{interfaceTag}");

        {
            using var classScope = new ScopedCodeWriter(Writer);

            if(ClassType.IsClass)
            {
                WriteStaticClass();

                WriteDefaultConstructor();

                WriteDelegateDefinitions();

                WritePropertyMetaCache(ClassType, $"{Type.CppName}MetaData", StructType.Properties, ELocalUsageScenarioType.Class);

                WriteProperties();

                WriteFunctions();
            }
            else
            {
                WriteInterfaceFunctions();
            }
        }

        if(!ClassType.IsInterface)
        {
            WriteInteropService();
        }            

        if (needExportInterface)
        {
            Writer.WriteNewLine();
            WriteInterface();
        }            

        return true;
    }
    #endregion

    #region Base Components
    /// <summary>
    /// Writes the static class.
    /// </summary>
    private void WriteStaticClass()
    {
        Writer.Write("#region Static Class");

        Writer.WriteComment($"Initializes static members of the <see cref=\"{ClassType.CppName}\"/> class.");

        Writer.Write("[DynamicDependency(nameof(StaticClass))]");
        Writer.Write($"static {ClassType.CppName}()");
        {
            using var staticConstructorScope = new ScopedCodeWriter(Writer);
        }

        Writer.WriteNewLine();

        Writer.WriteComment($"The UClass* load path of <see cref=\"{Type.CppName}\"/>");
        Writer.Write($"public const string {ClassType.Name}Path = \"{ClassType.PathName}\";");
        Writer.Write($"private static UClass? Z_{ClassType.Name}Class;");

        Writer.WriteComment($"Get the static UClass of {ClassType.Name}");
        Writer.WriteComment("UClass", "returns");            
        Writer.Write("public new static UClass StaticClass()");
        {
            using var staticClassScope = new ScopedCodeWriter(Writer);

            Writer.Write($"MetaInteropUtils.LoadClassIfNeed(ref Z_{ClassType.Name}Class, {ClassType.Name}Path);");
            Writer.WriteNewLine();

            Writer.Write($"return Z_{ClassType.Name}Class;");
        }
        Writer.Write("#endregion");
    }

    /// <summary>
    /// Writes the default constructor.
    /// </summary>
    private void WriteDefaultConstructor()
    {
        Writer.WriteNewLine();
        Writer.Write("#region Constructor");

        Writer.WriteComment($"Initializes a new instance of the <see cref=\"{Type.CppName}\"/>class.{Environment.NewLine}Do not use this function directly unless you know exactly what you are doing.");
        Writer.WriteComment("The native pointer value.", "param", "name=\"nativePtr\"");            
        Writer.Write($"public {Type.CppName}(IntPtr nativePtr) : base(nativePtr)");
        {
            using var scopedCodeWriter = new ScopedCodeWriter(Writer);
        }
        Writer.Write("#endregion");
    }
    #endregion

    #region Properties
    /// <summary>
    /// The force name of meta names
    /// </summary>
    public readonly HashSet<string> ForceNameOfMetaNames = [
        "ReplicatedUsing"
    ];

    /// <summary>
    /// Writes the properties.
    /// </summary>
    private void WriteProperties()
    {
        if(ClassType.Properties.Count <=0)
        {
            return;
        }

        var padding = CalcPropertyTypePadding();

        Writer.WriteNewLine();
        Writer.Write("#region Properties");
        foreach(var classProperty in ClassType.Properties)
        {
            if(!classProperty.Name!.IsValidCSharpIdentifier())
            {
                continue;
            }

            var processor = Context.GetProcessor(classProperty);

            Logger.Ensure<Exception>(processor != null, $"Missing processor for {classProperty}");

            Writer.WriteNewLine();

            Writer.Write($"#region {classProperty.Name}");

            processor!.BeforeClassPropertyWrite(Writer, classProperty);

            if(IsNativeBinding)
            {
                WriteAttributesComments((EPropertyFlags)classProperty.Flags, "UPROPERTY", classProperty.Metas);
            }
            else
            {
                WriteAttributes((EPropertyFlags)classProperty.Flags, "UPROPERTY", classProperty.Metas, () =>
                {
                    List<string> result = [];

                    foreach(var metaProperty in classProperty.Metas)
                    {
                        if (IsBindingToUnrealImplement && metaProperty.Key == MetaConstants.ToolTip)
                        {
                            continue;
                        }

                        var propertyInfo = typeof(UPROPERTYAttribute).GetProperty(metaProperty.Key);

                        if(propertyInfo == null)
                        {
                            continue;
                        }

                        if( propertyInfo.PropertyType == typeof(string))
                        {
                            result.Add(ForceNameOfMetaNames.Contains(metaProperty.Key)
                                ? $"{metaProperty.Key} = nameof({metaProperty.Value.Replace("\n", "\\n").Replace("\"", "\\\"")})"
                                : $"{metaProperty.Key} = \"{metaProperty.Value.Replace("\n", "\\n").Replace("\"", "\\\"")}\"");
                        }
                        else if(propertyInfo.PropertyType == typeof(bool))
                        {
                            result.Add($"{metaProperty.Key} = {metaProperty.Value.ToLower()}");
                        }
                        else if(propertyInfo.PropertyType.IsEnum)
                        {
                            var value = int.Parse(metaProperty.Value);
                            var enumValue = Enum.ToObject(propertyInfo.PropertyType, value);
                            result.Add($"{metaProperty.Key} = {propertyInfo.PropertyType.Name}.{enumValue}");
                        }
                        else
                        {
                            result.Add($"{metaProperty.Key} = {metaProperty.Value}");
                        }
                    }

                    return result;
                });
            }

            if(classProperty.DefaultValue.IsNotNullOrEmpty())
            {
                var defaultValue = classProperty.DefaultValue;
                    
                // replace " -> \"
                defaultValue = defaultValue.Replace("\"", "\\\"");
                // replace \n -> \\n
                defaultValue = defaultValue.Replace("\n", "\\n");
                Writer.Write($"[DefaultValueText(\"{defaultValue}\")]");
            }

            Writer.Write($"public {processor.GetCSharpTypeName(classProperty, ELocalUsageScenarioType.Class|ELocalUsageScenarioType.Property).PadRight(padding)} {classProperty.SafeName}");
            {
                using var propertyScope = new ScopedCodeWriter(Writer);

                if(!processor.HandleClassPropertyWrite(Writer, classProperty))
                {
                    Writer.Write("get");
                    {
                        using var getScope = new ScopedCodeWriter(Writer);

                        Writer.Write("var address = GetNativePtr();");
                        Writer.Write("Logger.Ensure<AccessViolationException>(address != IntPtr.Zero, \"Invalid native address!\");");
                        Writer.WriteNewLine();

                       // var returnTypeString = classProperty.GetCSharpTypeName(Context,
                       //     ELocalUsageScenarioType.Class | ELocalUsageScenarioType.Property);
                        
                        var text = processor.GetInteropGetValueText(
                            classProperty, 
                            "__returnValue", 
                            "address",
                            $"{Type.CppName}MetaData.{classProperty.Name}_Offset",
                            ELocalUsageScenarioType.Class | ELocalUsageScenarioType.Property | ELocalUsageScenarioType.PropertyGetter
                        );

                        Writer.Write($"var {text};");
                        Writer.WriteNewLine();

                        Writer.Write("return __returnValue;");
                    }

                    if(processor.AllowWritePropertySetter(classProperty))
                    {
                        var accessor = classProperty.IsPublicSetter ? "" : (classProperty.IsProtectedSetter ? "protected " : "private ");

                        Writer.Write($"{accessor}set");
                        {
                            using var setScope = new ScopedCodeWriter(Writer);

                            Writer.Write("var address = GetNativePtr();");
                            Writer.Write("Logger.Ensure<AccessViolationException>(address != IntPtr.Zero, \"Invalid native address!\");");
                            Writer.WriteNewLine();

                            var text = processor.GetInteropSetValueText(
                                classProperty, 
                                "value", 
                                "address",
                                $"{Type.CppName}MetaData.{classProperty.Name}_Offset",
                                ELocalUsageScenarioType.Class | ELocalUsageScenarioType.Property | ELocalUsageScenarioType.PropertySetter
                            );

                            Writer.Write($"{text};");
                        }
                    }                        
                }
            }

            processor.EndClassPropertyWrite(Writer, classProperty);

            Writer.Write("#endregion");
        }

        Writer.Write("#endregion");
    }
    #endregion

    #region Functions
    /// <summary>
    /// Writes the functions.
    /// </summary>
    private void WriteFunctions()
    {
        if(ClassType.Functions.Count<=0)
        {
            return;
        }

        var exportFunctions = ClassType.Functions.FindAll(function => !function.IsEvent && IsBindingToUnrealImplement);

        if(exportFunctions.Count > 0)
        {
            Writer.WriteNewLine();

            Writer.Write("#region Methods");

            foreach (var function in exportFunctions)
            {
                WriteFunction(function);
            }

            Writer.Write("#endregion");
        }

        var exportEventFunctions = ClassType.Functions.FindAll(function => function.IsEvent);

        if(exportEventFunctions.Count > 0)
        {
            Writer.WriteNewLine();
            Writer.Write("#region Event Methods");
            foreach (var function in exportEventFunctions)
            {
                WriteFunction(function);
            }
            Writer.Write("#endregion");
        }
    }

    /// <summary>
    /// Writes the function attributes.
    /// </summary>
    /// <param name="function">The function.</param>
    private void WriteFunctionAttributes(FunctionTypeDefinition function)
    {
        WriteAttributes((EFunctionFlags)function.Flags, function.IsEvent ? "UEVENT" : "UFUNCTION", function.Metas, () =>
        {
            List<string> result = [];

            foreach (var property in function.Metas)
            {
                if(IsBindingToUnrealImplement && property.Key == MetaConstants.ToolTip)
                {
                    continue;
                }

                var propertyInfo = (function.IsEvent ? typeof(UEVENTAttribute) : typeof(UFUNCTIONAttribute)).GetProperty(property.Key);

                if (propertyInfo == null)
                {
                    continue;
                }

                if (propertyInfo.PropertyType == typeof(string))
                {
                    result.Add($"{property.Key} = \"{property.Value.Replace("\n", "\\n").Replace("\"", "\\\"")}\"");
                }
                else if (propertyInfo.PropertyType == typeof(bool))
                {
                    result.Add($"{property.Key} = {property.Value.ToLower()}");
                }
                else if (propertyInfo.PropertyType.IsEnum)
                {
                    var value = int.Parse(property.Value);
                    var enumValue = Enum.ToObject(propertyInfo.PropertyType, value);
                    result.Add($"{property.Key} = {propertyInfo.PropertyType.Name}.{enumValue}");
                }
                else
                {
                    result.Add($"{property.Key} = {property.Value}");
                }
            }

            return result;
        });            
    }

    /// <summary>
    /// Posts the write property meta fields.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="usage">The usage.</param>
    protected override void PostWritePropertyMetaFields(StructTypeDefinition typeDefinition, ELocalUsageScenarioType usage)
    {
        base.PostWritePropertyMetaFields(typeDefinition, usage);

        if (typeDefinition.IsFunction && usage.HasFlag(ELocalUsageScenarioType.Class))
        {
            var functionName = typeDefinition.Name;

            Writer.Write($"internal static readonly UnrealInvocation? {functionName}Invocation;");
        }            
    }

    /// <summary>
    /// Posts the write property meta static constructor.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="name">The name.</param>
    /// <param name="properties">The properties.</param>
    /// <param name="usage">The usage.</param>
    protected override void PostWritePropertyMetaStaticConstructor(StructTypeDefinition typeDefinition, string name, IEnumerable<PropertyDefinition> properties, ELocalUsageScenarioType usage)
    {
        if (typeDefinition.IsFunction)
        {
            if(usage.HasFlag(ELocalUsageScenarioType.Delegate))
            {
                Writer.Write($"MetaInteropUtils.BindFunctionPropertyMetaCache(typeof({name}), StaticClass(), \"{typeDefinition.Name}\");");
            }
            else
            {
                base.PostWritePropertyMetaStaticConstructor(typeDefinition, name, properties, usage);
            }                
        }
        else if (properties.Any())
        {
            Writer.Write($"MetaInteropUtils.BindPropertyMetaCache(typeof({name}), StaticClass().GetNativePtr());");
        }
    }

    /// <summary>
    /// Writes the interface functions.
    /// </summary>
    private void WriteInterfaceFunctions()
    {
        foreach(var function in ClassType.Functions)
        {
            if(function is { IsStatic: false, IsPublic: true } && (function.IsEvent || IsBindingToUnrealImplement))
            {
                if(!WriteMetaComments(function.Metas))
                {
                    Writer.WriteComment($"Interface method of {function.Name}");
                }
                    
                // WriteFunctionAttributes(function);
                WriteFunctionDeclaration(function);
                Writer.WriteNewLine();
            }
        }
    }

    /// <summary>
    /// Writes the function declaration.
    /// </summary>
    /// <param name="function">The function.</param>
    private void WriteFunctionDeclaration(FunctionTypeDefinition function)
    {
        var returnType = function.GetReturnTypeName(Context);
        var functionName = function.FunctionName;
        var @override = ReserveNames.Contains(functionName) ? "new " : "";
        var paramList = function.GetExportParameters(Context, true);            

        Writer.Write($"{@override}{returnType} {functionName}({paramList});");
    }

    private void WriteFastInvokeFunctionInteropPointers(FunctionTypeDefinition function)
    {
        Writer.Write("#region Interop Pointers");
        var className = $"{function.FunctionName}InteropFunctionPointers";

        Writer.Write($"private static class {className}");
            
        {
            using var classScope = new ScopedCodeWriter(Writer);

            Logger.Assert(function.Parent != null);

            var interopFunctionName = $"{function.Parent!.CppName}_{function.FunctionName}";

            Writer.Write($"public static readonly IntPtr {interopFunctionName};");
                
            // write static constructor
            Writer.WriteNewLine();
            Writer.Write($"static {className}()");
            {
                using var staticConstructorScope = new ScopedCodeWriter(Writer);
                Writer.Write($"InteropFunctions.BindInteropFunctionPointer(ref {interopFunctionName}, nameof({interopFunctionName}), nameof({className}));");
            }
        }

        Writer.Write("#endregion");
    }

    private void WriteEventFunctionPrefixContent(FunctionTypeDefinition function, string accessor, string @static, string returnType, string paramList, string endTag)
    {
        var functionName = function.FunctionName;

        if (!Context.IsFastInvokeFunction(function))
        {
            using var functionScope = new ScopedCodeWriter(Writer);

            var returnFlag = function.HasReturnType() ? "return " : "";
            var invokeParameters = function.GetExportInvokeParameters(Context);

            Writer.WriteCommonComment($"Calling this method cannot directly trigger the method of the base class. {Environment.NewLine}This method will try to find the latest event definition function again. {Environment.NewLine}To specify the background function to call the event of a certain class, you should use {functionName}<T>.");
            Writer.Write($"{returnFlag}Z_{functionName}({function.Name}MetaData.{function.Name}Invocation!{(function.ParameterCount != 0 ? ", " : "")}{invokeParameters});");
        }
        else
        {
            WriteFunctionFastInvokeContent(function);
        }

        if (!function.IsOverrideFunction)
        {
            Writer.WriteNewLine();
            Writer.WriteComment("Use this version of the function to call the implementation function behind an event of a specified type, {Environment.NewLine}which mimics the behavior of calling a function of the base class.");
            Writer.Write($"{accessor}{@static}{returnType} {functionName}<TBaseClassType>({paramList}) where TBaseClassType : {ClassType.CppName}{endTag}");
            {
                using var functionScope = new ScopedCodeWriter(Writer);

                var returnFlag = function.HasReturnType() ? "return " : "";
                var invokeParameters = function.GetExportInvokeParameters(Context);

                Writer.Write($"using var __invocation = new UnrealInvocation(UClass.GetClassOf<TBaseClassType>(), nameof({function.Name}));");
                Writer.Write($"{returnFlag}Z_{functionName}(__invocation{(function.ParameterCount != 0 ? ", " : "")}{invokeParameters});");
            }
        }

        Writer.WriteNewLine();
        Writer.WriteComment("internal wrapper for invoke event function");
        Writer.Write($"private {@static}{returnType} Z_{functionName}(UnrealInvocation __invocation{(function.ParameterCount != 0 ? ", " : "")}{paramList}){endTag}");
    }

    private void WriteFunctionCommonInvokeContent(FunctionTypeDefinition function)
    {
        using var functionScope = new ScopedCodeWriter(Writer);

        {
            if (!function.IsEvent)
            {
                Writer.Write($"var __invocation = {function.Name}MetaData.{function.Name}Invocation!;");
            }

            Writer.Write("unsafe");
            using var unsafeScope = new ScopedCodeWriter(Writer);
            {
                if (function.Properties.Count > 0)
                {
                    Writer.Write("var __paramBufferPointer = stackalloc byte[__invocation.ParamSize];");
                    Writer.WriteNewLine();

                    Writer.Write("using var __scopedInvoker = new ScopedUnrealInvocation(__invocation, (IntPtr)__paramBufferPointer);");
                }

                var bAnyPassParameter = false;

                foreach (var property in function.Properties.Where(property => !property.IsReturnParam))
                {
                    bAnyPassParameter = true;

                    var processor = Context.GetProcessor(property);

                    Logger.EnsureNotNull(processor, $"Failed find property {property}");

                    var text = processor.GetInteropSetValueText(
                        property,
                        property.SafeName,
                        "(IntPtr)__paramBufferPointer",
                        $"{function.Name}MetaData.{property.Name}_Offset",
                        ELocalUsageScenarioType.Common
                    );

                    Writer.Write($"{text};");
                }

                if (bAnyPassParameter)
                {
                    Writer.WriteNewLine();
                }

                var invokeTarget = function.IsStatic ? "null" : "this";
                var paramAddress = function.Properties.Count > 0 ? "(IntPtr)__paramBufferPointer" : "IntPtr.Zero";
                Writer.Write($"__invocation.Invoke({invokeTarget}, {paramAddress});");

                if (function.Properties.Any(x => x.IsOutParam))
                {
                    Writer.WriteNewLine();
                    Writer.Write("// Copy out parameters back ");
                    foreach (var property in function.Properties)
                    {
                        if (property.IsOutParam)
                        {
                            // copy param back
                            var processor = Context.GetProcessor(property);

                            Logger.EnsureNotNull(processor, $"Failed find property {property}");

                            var text = processor.GetInteropGetValueText(
                                property,
                                property.SafeName,
                                "(IntPtr)__paramBufferPointer",
                                $"{function.Name}MetaData.{property.Name}_Offset",
                                ELocalUsageScenarioType.Common
                            );

                            Writer.Write($"{text};");
                        }
                    }
                }

                var functionReturnType = function.GetReturnType();

                if (functionReturnType != null)
                {
                    Writer.WriteNewLine();

                    var processor = Context.GetProcessor(functionReturnType);
                    Logger.EnsureNotNull(processor, $"Failed find property {functionReturnType}");

                    // Writer.Write($"{returnType} __returnValue;");

                    var text = processor.GetInteropGetValueText(
                        functionReturnType,
                        "__returnValue",
                        "(IntPtr)__paramBufferPointer",
                        $"{function.Name}MetaData.{functionReturnType.Name}_Offset",
                        ELocalUsageScenarioType.Common
                    );

                    Writer.Write($"var {text};");

                    Writer.Write("return __returnValue;");
                }
            }
        }
    }

    private string GetFastInvokeNativeDelegateText(FunctionTypeDefinition function)
    {
        List<string> paramList = [];

        if (!function.IsStatic)
        {
            paramList.Add("IntPtr");
        }

        paramList.AddRange(function.Properties.Where(x => !x.IsReturnParam).Select(property =>
        {
            var processor = Context.GetProcessor(property);
            Logger.EnsureNotNull(processor);

            return processor.GetFastInvokeDelegateParameterTypeName(property, ELocalUsageScenarioType.Class|ELocalUsageScenarioType.Method|ELocalUsageScenarioType.Parameter);
        }));

        var returnParam = function.GetReturnType();

        if (returnParam != null)
        {
            var processor = Context.GetProcessor(returnParam);
            Logger.EnsureNotNull(processor);

            var type = processor.GetFastInvokeDelegateParameterTypeName(returnParam, ELocalUsageScenarioType.Class | ELocalUsageScenarioType.Method | ELocalUsageScenarioType.ReturnValue);

            paramList.Add($"{type}*");
        }

        paramList.Add("void");

        var declareTypeList = string.Join(", ", paramList);
            
        return $"delegate* unmanaged[Cdecl]<{declareTypeList}>";
    }

    private string GetFastInvokeParamList(FunctionTypeDefinition function)
    {
        List<string> paramList = [];

        if(!function.IsStatic)
        {
            paramList.Add("GetNativePtrChecked()");
        }

        paramList.AddRange(function.Properties.Where(x => !x.IsReturnParam).Select(property =>
        {
            var processor = Context.GetProcessor(property);
            Logger.EnsureNotNull(processor);

            return processor.GetFastInvokeParameterName(property);
        }));

        if(function.HasReturnType())
        {
            paramList.Add("&__result");
        }

        return string.Join(", ", paramList);
    }


    private void WriteFunctionFastInvokeContent(FunctionTypeDefinition function)
    {
        using var functionScope = new ScopedCodeWriter(Writer);

        Writer.Write("unsafe");
        {
            using var unsafeScope = new ScopedCodeWriter(Writer);

            var returnParam = function.GetReturnType();

            foreach(var param in function.Properties.Where(x=>!x.IsReturnParam))
            {
                var processor = Context.GetProcessor(param);

                Logger.EnsureNotNull(processor);

                var text = processor.GetBeforeFastInvokeText(param);
                if(text.IsNotNullOrEmpty())
                {
                    Writer.Write(text);
                }
            }

            if(returnParam != null)
            {
                var processor = Context.GetProcessor(returnParam);

                Logger.EnsureNotNull(processor);

                var returnTypeName = processor.GetFastInvokeDelegateParameterTypeName(returnParam, ELocalUsageScenarioType.Class | ELocalUsageScenarioType.Method | ELocalUsageScenarioType.Parameter | ELocalUsageScenarioType.ReturnValue);

                Writer.Write($"{returnTypeName} __result = default;");
            }

            var delegateTag = GetFastInvokeNativeDelegateText(function);
            var paramInvokeList = GetFastInvokeParamList(function);

            var invokeText = $"(({delegateTag}){function.FunctionName}InteropFunctionPointers.{function.Parent!.CppName}_{function.FunctionName})({paramInvokeList});";

            Writer.Write(invokeText);

            foreach (var param in function.Properties.Where(x => !x.IsReturnParam))
            {
                var processor = Context.GetProcessor(param);

                Logger.EnsureNotNull(processor);

                var text = processor.GetPostFastInvokeText(param);
                if (text.IsNotNullOrEmpty())
                {
                    Writer.Write(text);
                }
            }

            if (returnParam != null)
            {
                var processor = Context.GetProcessor(returnParam);

                Logger.EnsureNotNull(processor);

                Writer.Write($"return {processor.DecorateFastInvokeReturnValue(returnParam, "__result")};");
            }
        }
    }

    private void WriteFunctionContent(FunctionTypeDefinition function, string accessor, string @static, string returnType, string paramList, string endTag)
    {            
        if (function.IsEvent)
        {
            WriteEventFunctionPrefixContent(function, accessor, @static, returnType, paramList, endTag);
        }

        if (ClassType.IsClass)
        {
            if(!Context.IsFastInvokeFunction(function))
            {
                WriteFunctionCommonInvokeContent(function);
            }
            else
            {
                WriteFunctionFastInvokeContent(function);
            }                
        }
    }

    /// <summary>
    /// Writes the function.
    /// </summary>
    /// <param name="function">The function.</param>
    private void WriteFunction(FunctionTypeDefinition function)
    {
        if(!function.Name!.IsValidCSharpIdentifier())
        {
            return;
        }

        if(ClassType.IsClass)
        {
            Writer.WriteNewLine();
            Writer.Write($"#region {function.FunctionName}");

            if(!Context.IsFastInvokeFunction(function))
            {
                WritePropertyMetaCache(
                    function,
                    $"{function.Name}MetaData",
                    function.Properties,
                    ELocalUsageScenarioType.Class | ELocalUsageScenarioType.Method,
                    !function.IsEvent
                );
            }
            else
            {
                WriteFastInvokeFunctionInteropPointers(function);
            }                
                
            Writer.WriteNewLine();
        }

        if(!WriteMetaComments(function.Metas))
        {
            Writer.WriteComment($"Invoke Unreal UFunction {function.Name}");
        }

        WriteFunctionAttributes(function);

        if(Context.IsFastInvokeFunction(function))
        {
            Writer.Write("[FastAccessible]");
        }

        var returnType = function.GetReturnTypeName(Context);
        var functionName = function.FunctionName;
        var paramList = function.GetExportParameters(Context, true);
        var accessor = function.IsPublic ? "public " : (function.IsPrivate ? "private " : "protected ");
        var @static = function.IsStatic ? "static " : "";
        var @override = function.IsEvent ? (function.IsOverrideFunction?"new ":"") : (ReserveNames.Contains(functionName) && !function.IsStatic ? "new " : "");
        var @new = function.Name == "GetType" && function.HasReturnType() && function.Properties.Count == 1 ? "new " : "";
        var endTag = "";

        if(ClassType.IsInterface)
        {
            accessor = "";
            @new = "";
            @override = "";
            @static = "";
            endTag = ";";
        }                       

        Writer.Write($"{accessor}{@new}{@override}{@static}{returnType} {functionName}({paramList}){endTag}");

        WriteFunctionContent(function, accessor, @static, returnType, paramList, endTag);

        if (ClassType.IsClass)
        {
            Writer.Write("#endregion");
        }
    }
    #endregion

    #region Interfaces
    /// <summary>
    /// Writes the interface.
    /// </summary>
    private void WriteInterface()
    {
        var interfaceImplementTag = ClassType.SuperName != "UObject" && ClassType.SuperName != "UClass" && !ClassType.IsNativeBindingSuperType ?
            $" : I{ClassType.ConvertCppNameToScriptName(ClassType.SuperName)}" : "";

        Writer.Write($"#region Interface I{Type.Name}");
        Writer.Write($"{BaseTypeAccessPermission} partial interface I{Type.Name}{interfaceImplementTag}");
        {
            using var interfaceScope = new ScopedCodeWriter(Writer);

            if(ClassType.Properties.Count > 0)
            {
                Writer.Write("#region Properties");
                
                foreach (var property in ClassType.Properties.Where(property => !property.IsDelegateRelevanceProperty))
                {
                    Writer.Write($"{property.GetCSharpTypeName(Context, ELocalUsageScenarioType.Interface | ELocalUsageScenarioType.Class | ELocalUsageScenarioType.Property)} {property.Name}{{ get; }}");
                }
                
                Writer.Write("#endregion");

                Writer.WriteNewLine();
            }

            if(ClassType.Functions.Count > 0)
            {
                Writer.Write("#region Methods");
                foreach (var function in ClassType.Functions)
                {
                    Writer.WriteComment($"Interface method of {function.Name}");

                    Writer.Write(function.IsEvent
                        ? $"{function.GetReturnTypeName(Context)} {function.Name}_Implementation({function.GetExportParameters(Context)});"
                        : $"{function.GetReturnTypeName(Context)} {function.Name}({function.GetExportParameters(Context)});");
                }
                Writer.Write("#endregion");
            }                
        }
        Writer.Write("#endregion");
    }
    #endregion

    #region Interop Service
    /// <summary>
    /// Writes the interop service.
    /// </summary>
    private void WriteInteropService()
    {
        Writer.WriteNewLine();
        Writer.Write("#region Interop Policy");

        Writer.WriteComment($"Class {Type.CppName}InteropPolicy");
        Writer.Write("[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]");
        Writer.Write($"{BaseTypeAccessPermission} class {Type.CppName}InteropPolicy : TObjectInteropPolicy<{Type.CppName}>");
        {
            using var enumInteropPolicyScope = new ScopedCodeWriter(Writer);
        }

        Writer.Write("#endregion");
    }
    #endregion
}