using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;
using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Core.CodeGen
{
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
        public static readonly HashSet<string> ReserveNames = new HashSet<string>()
        {
            "ToString",
            "GetClass"
        };
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
            if(ClassType.IsInterface)
            {
                return GetInterfaceName(ClassType.SuperName);
            }

            return ClassType.SuperName;
        }

        /// <summary>
        /// Gets the name of the interface.
        /// </summary>
        /// <param name="cppName">Name of the CPP.</param>
        /// <returns>System.String.</returns>
        public static string GetInterfaceName(string cppName)
        {
            return cppName == "UObject" || cppName == "UInterface" ? "IUObjectInterface" : "I" + cppName.Substring(1);
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

            else
            {
                return $"Class {Type.CppName}{Environment.NewLine}" +
                    base.QueryBaseTypeMainCommentText() +
                    $"Implements the <see cref=\"{ClassType.SuperName}\" />{Environment.NewLine}";
            }                
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
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        protected override bool ExportInternal()
        {
            bool NeedExportInterface = IsCSharpBinding;

            string ExtraNativeInterface = string.Join(", ", ClassType.Interfaces.Select(x => GetInterfaceName(x))); ;
            string InterfaceTag = NeedExportInterface ? $", I{Type.Name}" : "";

            if(ExtraNativeInterface.Length > 0)
            {
                InterfaceTag = InterfaceTag + ", " + ExtraNativeInterface;
            }

            if(IsNativeBinding)
            {
                if(ClassType.IsInterface)
                {
                    Writer.Write($"[NativeBinding(\"{Type.Name}\", \"{Type.CppName}\", \"{Type.PathName}\")]");
                }
                else
                {
                    Writer.Write($"[NativeBinding(\"{Type.Name}\", \"{Type.CppName}\", {Type.CppName}.{Type.Name}Path)]");
                }
            }
            else if(IsBlueprintBinding)
            {
                if (ClassType.IsInterface)
                {
                    Writer.Write($"[BlueprintBinding(\"{Type.PathName}\")]");
                }
                else
                {
                    Writer.Write($"[BlueprintBinding({Type.CppName}.{Type.Name}Path)]");
                }
            }

            Writer.Write("[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]");

            WriteAttributes<EClassFlags>((EClassFlags)ClassType.Flags, "UCLASS", ClassType.Metas, ()=>
            {
                List<string> results = new List<string>();
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

            if(ClassType.IsInterface)
            {
                Writer.Write($"{BaseTypeAccessPermission} partial interface {GetInterfaceName(Type.CppName!)} : {GetSuperName()}{InterfaceTag}");
            }
            else
            {
                Writer.Write($"{BaseTypeAccessPermission} partial class {Type.CppName} : {ClassType.SuperName}{InterfaceTag}");
            }
            
            {
                using ScopedCodeWriter ClassScope = new ScopedCodeWriter(Writer);

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

            if (NeedExportInterface)
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
                using ScopedCodeWriter StaticConstructorScope = new ScopedCodeWriter(Writer);
            }

            Writer.WriteNewLine();

            Writer.WriteComment($"The UClass* load path of <see cref=\"{Type.CppName}\"/>");
            Writer.Write($"public const string {ClassType.Name}Path = \"{ClassType.PathName}\";");
            Writer.Write($"private static UClass? Z_{ClassType.Name}Class;");

            Writer.WriteComment($"Get the static UClass of {ClassType.Name}");
            Writer.WriteComment("UClass", "returns");            
            Writer.Write($"public new static UClass StaticClass()");
            {
                using ScopedCodeWriter StaticClassScope = new ScopedCodeWriter(Writer);

                Writer.Write($"MetaInteropUtils.LoadClassIfNeed(ref Z_{ClassType.Name}Class, {ClassType.Name}Path);");
                Writer.WriteNewLine();

                Writer.Write($"return Z_{ClassType.Name}Class!;");
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
                using ScopedCodeWriter scopedCodeWriter = new ScopedCodeWriter(Writer);
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

            int Padding = CalcPropertyTypePadding();

            Writer.WriteNewLine();
            Writer.Write("#region Properties");
            foreach(var Property in ClassType.Properties)
            {
                if(!Property.Name!.IsValidCSharpIdentifier())
                {
                    continue;
                }

                var processor = Context.GetProcessor(Property);

                Logger.Ensure<Exception>(processor != null, $"Missing processor for {Property}");

                Writer.WriteNewLine();

                Writer.Write($"#region {Property.Name}");

                processor!.BeforeClassPropertyWrite(Writer, Property);

                if(IsNativeBinding)
                {
                    WriteAttributesComments<EPropertyFlags>((EPropertyFlags)Property.Flags, "UPROPERTY", Property.Metas);
                }
                else
                {
                    WriteAttributes<EPropertyFlags>((EPropertyFlags)Property.Flags, "UPROPERTY", Property.Metas, () =>
                    {
                        List<string> result = new List<string>();

                        foreach(var property in Property.Metas)
                        {
                            if (IsBindingToUnrealImplement && property.Key == MetaConstants.ToolTip)
                            {
                                continue;
                            }

                            var propertyInfo = typeof(UPROPERTYAttribute).GetProperty(property.Key);

                            if(propertyInfo == null)
                            {
                                continue;
                            }

                            if( propertyInfo.PropertyType == typeof(string))
                            {
                                if(ForceNameOfMetaNames.Contains(property.Key))
                                {
                                    result.Add($"{property.Key} = nameof({property.Value.ToString().Replace("\n", "\\n").Replace("\"", "\\\"")})");
                                }
                                else
                                {
                                    result.Add($"{property.Key} = \"{property.Value.ToString().Replace("\n", "\\n").Replace("\"", "\\\"")}\"");
                                }                                
                            }
                            else if(propertyInfo.PropertyType == typeof(bool))
                            {
                                result.Add($"{property.Key} = {property.Value.ToLower()}");
                            }
                            else if(propertyInfo.PropertyType.IsEnum)
                            {
                                int value = int.Parse(property.Value.ToString());
                                object enumValue = Enum.ToObject(propertyInfo.PropertyType, value);
                                result.Add($"{property.Key} = {propertyInfo.PropertyType.Name}.{enumValue.ToString()}");
                            }
                            else
                            {
                                result.Add($"{property.Key} = {property.Value}");
                            }
                        }

                        return result;
                    });
                }

                if(Property.DefaultValue.IsNotNullOrEmpty())
                {
                    var defaultValue = Property.DefaultValue;
                    
                    // replace " -> \"
                    defaultValue = defaultValue.Replace("\"", "\\\"");
                    // replace \n -> \\n
                    defaultValue = defaultValue.Replace("\n", "\\n");
                    Writer.Write($"[DefaultValueText(\"{defaultValue}\")]");
                }

                Writer.Write($"public {processor!.GetCSharpTypeName(Property, ELocalUsageScenarioType.Class|ELocalUsageScenarioType.Property)!.PadRight(Padding)} {Property.SafeName}");
                {
                    using ScopedCodeWriter PropertyScope = new ScopedCodeWriter(Writer);

                    if(!processor!.HandleClassPropertyWrite(Writer, Property))
                    {
                        Writer.Write("get");
                        {
                            using ScopedCodeWriter GetScope = new ScopedCodeWriter(Writer);

                            Writer.Write("IntPtr address = GetNativePtr();");
                            Writer.Write("Logger.Ensure<AccessViolationException>(address != IntPtr.Zero, \"Invalid native address!\");");
                            Writer.WriteNewLine();

                            Writer.Write($"{Property.GetCSharpTypeName(Context, ELocalUsageScenarioType.Class | ELocalUsageScenarioType.Property)} ReturnValue;");
                            string text = processor.GetInteropGetValueText(
                                Property, 
                                "ReturnValue", 
                                "address",
                                $"{Type.CppName}MetaData.{Property.Name}_Offset",
                                ELocalUsageScenarioType.Class | ELocalUsageScenarioType.Property | ELocalUsageScenarioType.PropertyGetter
                            );

                            Writer.Write($"{text};");
                            Writer.WriteNewLine();

                            Writer.Write("return ReturnValue;");
                        }

                        if(processor.AllowWritePropertySetter(Property))
                        {
                            string accessor = Property.IsPublicSetter ? "" : (Property.IsProtectedSetter ? "protected " : "private ");

                            Writer.Write($"{accessor}set");
                            {
                                using ScopedCodeWriter SetScope = new ScopedCodeWriter(Writer);

                                Writer.Write("IntPtr address = GetNativePtr();");
                                Writer.Write("Logger.Ensure<AccessViolationException>(address != IntPtr.Zero, \"Invalid native address!\");");
                                Writer.WriteNewLine();

                                string text = processor.GetInteropSetValueText(
                                    Property, 
                                    "value", 
                                    "address",
                                    $"{Type.CppName}MetaData.{Property.Name}_Offset",
                                    ELocalUsageScenarioType.Class | ELocalUsageScenarioType.Property | ELocalUsageScenarioType.PropertySetter
                                );

                                Writer.Write($"{text};");
                            }
                        }                        
                    }
                }

                processor!.EndClassPropertyWrite(Writer, Property);

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
            WriteAttributes<EFunctionFlags>((EFunctionFlags)function.Flags, function.IsEvent ? "UEVENT" : "UFUNCTION", function.Metas, () =>
            {
                List<string> result = new List<string>();

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
                        result.Add($"{property.Key} = \"{property.Value.ToString().Replace("\n", "\\n").Replace("\"", "\\\"")}\"");
                    }
                    else if (propertyInfo.PropertyType == typeof(bool))
                    {
                        result.Add($"{property.Key} = {property.Value.ToLower()}");
                    }
                    else if (propertyInfo.PropertyType.IsEnum)
                    {
                        int value = int.Parse(property.Value.ToString());
                        object enumValue = Enum.ToObject(propertyInfo.PropertyType, value);
                        result.Add($"{property.Key} = {propertyInfo.PropertyType.Name}.{enumValue.ToString()}");
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

                Writer.Write($"internal static UnrealInvocation? {functionName}Invocation;");
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
            else if (properties.Count() > 0)
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
                if(!function.IsStatic && function.IsPublic && (function.IsEvent || IsBindingToUnrealImplement))
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
            string returnType = function.GetReturnTypeName(Context);
            string functionName = function.FunctionName;
            string @override = ReserveNames.Contains(functionName) ? "new " : "";
            string paramList = function.GetExportParameters(Context, true);            

            Writer.Write($"{@override}{returnType} {functionName}({paramList});");
        }

        private void WriteFastInvokeFunctionInteropPointers(FunctionTypeDefinition function)
        {
            Writer.Write("#region Interop Pointers");
            string className = $"{function.FunctionName}InteropFunctionPointers";

            Writer.Write($"private static class {className}");
            
            {
                using ScopedCodeWriter classScope = new ScopedCodeWriter(Writer);

                Logger.Assert(function.Parent != null);

                string interopFunctionName = $"{function.Parent!.CppName}_{function.FunctionName}";

                Writer.Write($"public readonly static IntPtr {interopFunctionName};");
                
                // write static constructor
                Writer.WriteNewLine();
                Writer.Write($"static {className}()");
                {
                    using ScopedCodeWriter StaticConstructorScope = new ScopedCodeWriter(Writer);
                    Writer.Write($"InteropFunctions.BindInteropFunctionPointer(ref {interopFunctionName}, nameof({interopFunctionName}), nameof({className}));");
                }
            }

            Writer.Write("#endregion");
        }

        private void WriteEventFunctionPrefixContent(FunctionTypeDefinition function, string accessor, string @static, string returnType, string paramList, string endTag)
        {
            string functionName = function.FunctionName;

            if (!Context.IsFastInvokeFunction(function))
            {
                using ScopedCodeWriter FunctionScope = new ScopedCodeWriter(Writer);

                string returnFlag = function.HasReturnType() ? "return " : "";
                string InvokeParameters = function.GetExportInvokeParameters(Context);

                Writer.WriteCommonComment($"Calling this method cannot directly trigger the method of the base class. {Environment.NewLine}This method will try to find the latest event definition function again. {Environment.NewLine}To specify the background function to call the event of a certain class, you should use {functionName}<T>.");
                Writer.Write($"{returnFlag}Z_{functionName}({function.Name}MetaData.{function.Name}Invocation!{(function.ParameterCount != 0 ? ", " : "")}{InvokeParameters});");
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
                    using ScopedCodeWriter FunctionScope = new ScopedCodeWriter(Writer);

                    string returnFlag = function.HasReturnType() ? "return " : "";
                    string InvokeParameters = function.GetExportInvokeParameters(Context);

                    Writer.Write($"using UnrealInvocation __invocation = new UnrealInvocation(UClass.GetClassOf<TBaseClassType>()!, nameof({function.Name}));");
                    Writer.Write($"{returnFlag}Z_{functionName}(__invocation{(function.ParameterCount != 0 ? ", " : "")}{InvokeParameters});");
                }
            }

            Writer.WriteNewLine();
            Writer.WriteComment("internal wrapper for invoke event function");
            Writer.Write($"private {@static}{returnType} Z_{functionName}(UnrealInvocation __invocation{(function.ParameterCount != 0 ? ", " : "")}{paramList}){endTag}");
        }

        private void WriteFunctionCommonInvokeContent(FunctionTypeDefinition function, string returnType)
        {
            using ScopedCodeWriter FunctionScop = new ScopedCodeWriter(Writer);

            {
                if (!function.IsEvent)
                {
                    Writer.Write($"var __invocation = {function.Name}MetaData.{function.Name}Invocation!;");
                }

                Writer.Write("unsafe");
                using ScopedCodeWriter UnsafeScope = new ScopedCodeWriter(Writer);
                {
                    if (function.Properties.Count > 0)
                    {
                        Writer.Write($"byte* __paramBufferPointer = stackalloc byte[__invocation.ParamSize];");
                        Writer.WriteNewLine();

                        Writer.Write($"using var __scopedInvoker = new ScopedUnrealInvocation(__invocation, (IntPtr)__paramBufferPointer);");
                    }

                    bool bAnyPassParameter = false;

                    foreach (var property in function.Properties)
                    {
                        if (property.IsReturnParam)
                        {
                            continue;
                        }

                        bAnyPassParameter = true;

                        PropertyProcessor? Processor = Context.GetProcessor(property);

                        Logger.EnsureNotNull(Processor, $"Failed find property {property}");

                        string text = Processor.GetInteropSetValueText(
                            property,
                            property.SafeName,
                            "(IntPtr)__paramBufferPointer",
                            $"{function.Name}MetaData.{property.Name}_Offset"
                        );

                        Writer.Write($"{text};");
                    }

                    if (bAnyPassParameter)
                    {
                        Writer.WriteNewLine();
                    }

                    string InvokeTarget = function.IsStatic ? "null" : "this";
                    string paramAddress = function.Properties.Count > 0 ? "(IntPtr)__paramBufferPointer" : "IntPtr.Zero";
                    Writer.Write($"__invocation.Invoke({InvokeTarget}, {paramAddress});");

                    if (function.Properties.Any(x => x.IsOutParam))
                    {
                        Writer.WriteNewLine();
                        Writer.Write("// Copy out parameters back ");
                        foreach (var property in function.Properties)
                        {
                            if (property.IsOutParam)
                            {
                                // copy param back
                                PropertyProcessor? Processor = Context.GetProcessor(property);

                                Logger.EnsureNotNull(Processor, $"Failed find property {property}");

                                string text = Processor.GetInteropGetValueText(
                                    property,
                                    property.SafeName,
                                    "(IntPtr)__paramBufferPointer",
                                    $"{function.Name}MetaData.{property.Name}_Offset"
                                );

                                Writer.Write($"{text};");
                            }
                        }
                    }

                    var ReturnType = function.GetReturnType();

                    if (ReturnType != null)
                    {
                        Writer.WriteNewLine();

                        PropertyProcessor? Processor = Context.GetProcessor(ReturnType);
                        Logger.EnsureNotNull(Processor, $"Failed find property {ReturnType}");

                        // Writer.Write($"{returnType} __returnValue;");

                        string text = Processor.GetInteropGetValueText(
                            ReturnType,
                            "__returnValue",
                            "(IntPtr)__paramBufferPointer",
                            $"{function.Name}MetaData.{ReturnType.Name}_Offset"
                        );

                        Writer.Write($"var {text};");

                        Writer.Write($"return __returnValue;");
                    }
                }
            }
        }

        private string GetFastInvokeNativeDelegateText(FunctionTypeDefinition function)
        {
            List<string> ParamList = new List<string>();

            if (!function.IsStatic)
            {
                ParamList.Add("IntPtr");
            }

            ParamList.AddRange(function.Properties.Where(x => !x.IsReturnParam).Select(property =>
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

                ParamList.Add($"{type}*");
            }

            ParamList.Add("void");

            string declareTypeList = string.Join(", ", ParamList);
            
            return $"delegate* unmanaged[Cdecl]<{declareTypeList}>";
        }

        private string GetFastInvokeParamList(FunctionTypeDefinition function)
        {
            List<string> ParamList = new List<string>();

            if(!function.IsStatic)
            {
                ParamList.Add("GetNativePtrChecked()");
            }

            ParamList.AddRange(function.Properties.Where(x => !x.IsReturnParam).Select(property =>
            {
                var processor = Context.GetProcessor(property);
                Logger.EnsureNotNull(processor);

                return processor.GetFastInvokeParameterName(property);
            }));

            if(function.HasReturnType())
            {
                ParamList.Add("&__result");
            }

            return string.Join(", ", ParamList);
        }


        private void WriteFunctionFastInvokeContent(FunctionTypeDefinition function)
        {
            using ScopedCodeWriter FunctionScop = new ScopedCodeWriter(Writer);

            Writer.Write("unsafe");
            {
                using ScopedCodeWriter unsafeScope = new ScopedCodeWriter(Writer);

                var ReturnParam = function.GetReturnType();

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

                if(ReturnParam != null)
                {
                    var processor = Context.GetProcessor(ReturnParam);

                    Logger.EnsureNotNull(processor);

                    var returnTypeName = processor.GetFastInvokeDelegateParameterTypeName(ReturnParam, ELocalUsageScenarioType.Class | ELocalUsageScenarioType.Method | ELocalUsageScenarioType.Parameter | ELocalUsageScenarioType.ReturnValue);

                    Writer.Write($"{returnTypeName} __result = default;");
                }

                string DelegateTag = GetFastInvokeNativeDelegateText(function);
                string ParamInvokeList = GetFastInvokeParamList(function);

                string InvokeText = $"(({DelegateTag}){function.FunctionName}InteropFunctionPointers.{function.Parent!.CppName}_{function.FunctionName})({ParamInvokeList});";

                Writer.Write(InvokeText);

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

                if (ReturnParam != null)
                {
                    var processor = Context.GetProcessor(ReturnParam);

                    Logger.EnsureNotNull(processor);

                    Writer.Write($"return {processor.DecorateFastInvokeReturnValue(ReturnParam, "__result")};");
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
                    WriteFunctionCommonInvokeContent(function, returnType);
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
                Writer.Write("[FastAccessable]");
            }

            string returnType = function.GetReturnTypeName(Context);
            string functionName = function.FunctionName;
            string paramList = function.GetExportParameters(Context, true);
            string accessor = function.IsPublic ? "public " : (function.IsPrivate ? "private " : "protected ");
            string @static = function.IsStatic ? "static " : "";
            string @override = function.IsEvent ? (function.IsOverrideFunction?"new ":"") : (ReserveNames.Contains(functionName) && !function.IsStatic ? "new " : "");
            string @new = function.Name == "GetType" && function.HasReturnType() && function.Properties.Count == 1 ? "new " : "";
            string endTag = "";

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
            string interfaceImplementTag = ClassType.SuperName != "UObject" && ClassType.SuperName != "UClass" && !ClassType.IsNativeBindingSuperType ?
                $" : I{ClassType.ConvertCppNameToScriptName(ClassType.SuperName)}" : "";

            Writer.Write($"#region Interface I{Type.Name}");
            Writer.Write($"{BaseTypeAccessPermission} partial interface I{Type.Name}{interfaceImplementTag}");
            {
                using ScopedCodeWriter InterfaceScope = new ScopedCodeWriter(Writer);

                if(ClassType.Properties.Count > 0)
                {
                    Writer.Write("#region Properties");
                    foreach (var property in ClassType.Properties)
                    {
                        if(!property.IsDelegateRelevanceProperty)
                        {
                            Writer.Write($"{property.GetCSharpTypeName(Context, ELocalUsageScenarioType.Interface | ELocalUsageScenarioType.Class | ELocalUsageScenarioType.Property)} {property.Name}{{ get; }}");
                        }                        
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

                        if(function.IsEvent)
                        {
                            Writer.Write($"{function.GetReturnTypeName(Context)} {function.Name}_Implementation({function.GetExportParameters(Context)});");
                        }                        
                        else
                        {
                            Writer.Write($"{function.GetReturnTypeName(Context)} {function.Name}({function.GetExportParameters(Context)});");
                        }
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
                using ScopedCodeWriter EnumInteropPolicyScope = new ScopedCodeWriter(Writer);
            }

            Writer.Write("#endregion");
        }
        #endregion
    }
}
