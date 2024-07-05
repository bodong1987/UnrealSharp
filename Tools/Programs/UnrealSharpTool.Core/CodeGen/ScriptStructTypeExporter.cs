using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;
using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Core.CodeGen;

/// <summary>
/// Class ScriptStructTypeExporter.
/// Export UStruct Binding
/// Implements the <see cref="UnrealSharpTool.Core.CodeGen.StructTypeExporter" />
/// </summary>
/// <seealso cref="UnrealSharpTool.Core.CodeGen.StructTypeExporter" />
public class ScriptStructTypeExporter : StructTypeExporter
{
    #region Properties
    /// <summary>
    /// The script structure type
    /// </summary>
    public readonly ScriptStructTypeDefinition ScriptStructType;

    /// <summary>
    /// The is fast access type
    /// </summary>
    public readonly bool IsFastAccessType;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptStructTypeExporter"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="targetDirectory">The target directory.</param>
    /// <param name="typeDefinition">The type definition.</param>
    public ScriptStructTypeExporter(BindingContext context, string targetDirectory, ScriptStructTypeDefinition typeDefinition) :
        base(context, targetDirectory, typeDefinition)
    {
        ScriptStructType = typeDefinition;
        IsFastAccessType = Context.Document.IsFastAccessStructType(Type.CppName!);
    }
    #endregion

    #region Template Methods Override
    /// <summary>
    /// Queries the base type main comment text.
    /// </summary>
    /// <returns>System.String.</returns>
    protected override string QueryBaseTypeMainCommentText()
    {
        return $"Struct {Type.CppName}{Environment.NewLine}" + base.QueryBaseTypeMainCommentText();
    }

    /// <summary>
    /// Exports the internal.
    /// </summary>
    /// <returns><c>true</c> if export success, <c>false</c> otherwise.</returns>
    protected override bool ExportInternal()
    {
        if (IsNativeBinding)
        {
            if(IsFastAccessType)
            {
                Writer.Write($"[FastAccessible({StructType.Size})]");
                Writer.Write("[StructLayout(LayoutKind.Sequential)]");
            }

            Writer.Write($"[NativeBinding(\"{Type.Name}\", \"{Type.CppName}\", {Type.Name}Path)]");                
        }
        else if(IsBlueprintBinding)
        {
            Writer.Write($"[BlueprintBinding({Type.CppName}.{Type.Name}Path)]");                
        }
            
        Writer.Write("[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]");

        WriteAttributes((EStructFlags)Type.Flags, "USTRUCT", Type.Metas, ()=>
        {
            if(IsNativeBinding)
            {
                return [$"NativeSize = {StructType.Size}"];
            }

            return [];
        });

        Writer.Write($"{BaseTypeAccessPermission} partial struct {Type.CppName}");

        {
            using var structScope = new ScopedCodeWriter(Writer);

            ExportConstTypeLoadPath();

            if (!Type.BindingExportFlags.HasFlag(EBindingExportFlags.NoConstructor))
            {
                ExportEmptyConstructor();
            }                    

            WriteDelegateDefinitions();

            ExportStructFields();

            if(!Type.BindingExportFlags.HasFlag(EBindingExportFlags.NoMetaData) && !IsFastAccessType)
            {
                WritePropertyMetaCache(
                    ScriptStructType,
                    $"{Type.CppName}MetaData",
                    StructType.Properties,
                    ELocalUsageScenarioType.Struct
                );
            }                

            if(!Type.BindingExportFlags.HasFlag(EBindingExportFlags.NoFromNative))
            {
                ExportFromNative();
            }

            if (!Type.BindingExportFlags.HasFlag(EBindingExportFlags.NoToNative))
            {
                ExportToNative();
            }                    

            if(Type.BindingExportFlags.HasFlag(EBindingExportFlags.WithStructView))
            {
                ExportStructView();
            }                
        }

        ExportInteropPolicy();

        return true;
    }
    #endregion

    #region Struct C# Proxy
    /// <summary>
    /// Exports the constant type load path.
    /// </summary>
    private void ExportConstTypeLoadPath()
    {
        Writer.Write("#region Struct Path");
        Writer.WriteComment($"The UStruct* load path of <see cref=\"{Type.CppName}\"/>");
        Writer.Write($"public const string {StructType.Name}Path = \"{Type.PathName}\";");
        Writer.Write("#endregion");
        Writer.WriteNewLine();
    }

    /// <summary>
    /// Exports the empty constructor.
    /// </summary>
    private void ExportEmptyConstructor()
    {
        Writer.Write("#region Constructor");
        Writer.WriteComment($"Initializes a new instance of the <see cref=\"{Type.CppName}\"/> struct.");
        Writer.Write($"public {StructType.CppName}()");
        {
            using var scopedCodeWriter = new ScopedCodeWriter(Writer);
        }
        Writer.Write("#endregion");
    }

    /// <summary>
    /// Exports the structure fields.
    /// </summary>
    private void ExportStructFields()
    {
        var allValidProperties = StructType.Properties.FindAll(
            structProperty =>
                !structProperty.IsDelegateRelevanceProperty &&
                structProperty.Name!.IsValidCSharpIdentifier());

        if (allValidProperties.Count <= 0)
        {
            return;
        }
        
        Writer.WriteNewLine();
        Writer.Write("#region Fields");
        var padding = CalcPropertyTypePadding();
            
        foreach (var structProperty in allValidProperties)
        {
            // WriteMetaComments(Property.Metas);
            if (IsBindingToUnrealImplement)
            {
                WriteAttributesComments((EPropertyFlags)structProperty.Flags, "UPROPERTY", structProperty.Metas);
            }
            else
            {
                WriteAttributes((EPropertyFlags)structProperty.Flags, "UPROPERTY", structProperty.Metas, () =>
                {
                    var result = new List<string>();

                    foreach (var property in structProperty.Metas)
                    {
                        if (IsBindingToUnrealImplement && property.Key == MetaConstants.ToolTip)
                        {
                            continue;
                        }

                        var propertyInfo = typeof(UPROPERTYAttribute).GetProperty(property.Key);

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

            var defaultValueTag = "";

            if(structProperty.DefaultValue.IsNotNullOrEmpty())
            {
                var processor = Context.GetProcessor(structProperty);

                Logger.EnsureNotNull(processor);

                var defaultValue = processor.DecorateDefaultValueText(structProperty, structProperty.DefaultValue, ELocalUsageScenarioType.Struct | ELocalUsageScenarioType.Field);

                if(defaultValue.IsNotNullOrEmpty())
                {
                    defaultValueTag = $" = {defaultValue}";
                }
            }

            Writer.Write($"public {structProperty.GetCSharpTypeName(Context, ELocalUsageScenarioType.Struct | ELocalUsageScenarioType.Field).PadRight(padding)} {structProperty.SafeName}{defaultValueTag};");
            Writer.WriteNewLine();
        }

        Writer.Write("#endregion");
    }

    /// <summary>
    /// Exports from native.
    /// </summary>
    /// <exception cref="System.Exception">Missing processor for {property}</exception>
    private void ExportFromNative()
    {
        Writer.WriteNewLine();
        Writer.Write("#region From Native");
        Writer.WriteComment($"Create a new instance of the <see cref=\"{Type.CppName}\"/> struct from the Native pointer");
        Writer.Write("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        Writer.Write($"public static {Type.CppName} FromNative(IntPtr address, int offset)");

        {
            using var fromNativeScope = new ScopedCodeWriter(Writer);                

            Writer.Write("Logger.Ensure<AccessViolationException>(address != IntPtr.Zero, \"Invalid native address!\");");
            Writer.WriteNewLine();

            if (!IsFastAccessType)
            {   
                var allValidProperties = StructType.Properties.FindAll(property =>
                    !property.IsDelegateProperty && property.Name!.IsValidCSharpIdentifier()
                ).ToList();

                if (allValidProperties.Count > 0)
                {
                    Writer.Write("var NativeAddress = IntPtr.Add(address, offset);");
                    Writer.WriteNewLine();    
                }

                Writer.Write($"var value = new {Type.CppName}();");

                foreach (var property in allValidProperties)
                {
                    var processor = Context.GetProcessor(property);

                    if (processor == null)
                    {
                        throw new Exception($"Missing processor for {property}");
                    }

                    var text = processor.GetInteropGetValueText(
                        property,
                        $"value.{property.SafeName}",
                        "NativeAddress",
                        $"{Type.CppName}MetaData.{property.Name}_Offset",
                        ELocalUsageScenarioType.Common
                    );

                    if (text.IsNotNullOrEmpty())
                    {
                        Writer.Write($"{text};");
                    }
                }

                Writer.Write("return value;");
            }
            else
            {
                Writer.Write($"return InteropUtils.GetStructUnsafe<{Type.CppName!}>(address, offset);");
            }
        }

        Writer.Write("#endregion");
    }

    /// <summary>
    /// Exports to native.
    /// </summary>
    private void ExportToNative()
    {
        Writer.WriteNewLine();
        Writer.Write("#region To Native");
        Writer.WriteComment("Write the struct value to the Native pointer");
        Writer.Write("[MethodImpl(MethodImplOptions.AggressiveInlining)]");            
        Writer.Write($"public static void ToNative(IntPtr address, int offset, ref {Type.CppName} value)");

        {
            using var toNativeScope = new ScopedCodeWriter(Writer);

            Writer.Write("Logger.Ensure<AccessViolationException>(address != IntPtr.Zero, \"Invalid native address!\");");

            Writer.WriteNewLine();

            if (!IsFastAccessType)
            {
                var allValidProperties = StructType.Properties.FindAll(property =>
                    !property.IsDelegateProperty && property.Name!.IsValidCSharpIdentifier()
                ).ToList();

                if (allValidProperties.Count > 0)
                {
                    Writer.Write("var NativeAddress = IntPtr.Add(address, offset);");

                    Writer.WriteNewLine();

                    foreach (var property in allValidProperties)
                    {
                        var processor = Context.GetProcessor(property);

                        Logger.EnsureNotNull(processor, $"Missing processor for {property}");

                        var text = processor.GetInteropSetValueText(
                            property,
                            $"value.{property.SafeName}",
                            "NativeAddress",
                            $"{Type.CppName}MetaData.{property.Name}_Offset",
                            ELocalUsageScenarioType.Common
                        );

                        Writer.Write($"{text};");
                    }
                }
            }
            else
            {
                Writer.Write($"InteropUtils.SetStructUnsafe<{Type.CppName}>(address, offset, ref value);");
            }
        }

        Writer.Write("#endregion");
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
            base.PostWritePropertyMetaStaticConstructor(typeDefinition, name, properties, usage);
        }
        else if(properties.Any())
        {
            Writer.Write($"MetaInteropUtils.LoadStructPropertyMetaCache(typeof({name}), {typeDefinition.Name}Path);");
        }
    }
    #endregion

    #region Struct View
    /// <summary>
    /// Exports the structure view.
    /// </summary>
    protected virtual void ExportStructView()
    {
        Writer.WriteNewLine();
        Writer.Write("#region Struct View");
        Writer.Write("public partial struct StructView");
        {
            using var structScope = new ScopedCodeWriter(Writer);

            Writer.Write("public readonly IntPtr NativePtr;");

            ExportViewConstructor();

            ExportOffsetBasedProperties();

            ExportInteropMethods();
        }
        Writer.Write("#endregion");
    }

    /// <summary>
    /// Exports the view constructor.
    /// </summary>
    private void ExportViewConstructor()
    {
        Writer.Write("#region Constructor");

        Writer.WriteComment("Initializes a new instance of the <see cref=\"StructView\"/> struct.\nDo not use this function directly unless you know exactly what you are doing.");
        Writer.WriteComment("The native pointer value.", "param", "name=\"nativePtr\"");
        Writer.Write("public StructView(IntPtr nativePtr)");
        {
            using var scopedCodeWriter = new ScopedCodeWriter(Writer);

            Writer.Write("Logger.Ensure<AccessViolationException>(nativePtr != IntPtr.Zero, \"Invalid native address!\");");

            Writer.Write("NativePtr = nativePtr;");                
        }
        Writer.Write("#endregion");
    }

    /// <summary>
    /// Exports the offset based properties.
    /// </summary>
    private void ExportOffsetBasedProperties()
    {
        if (StructType.Properties.Count <= 0)
        {
            return;
        }

        var padding = CalcPropertyTypePadding();

        Writer.WriteNewLine();
        Writer.Write("#region Properties");
        foreach (var property in StructType.Properties)
        {
            var processor = Context.GetProcessor(property);

            Logger.Ensure<Exception>(processor != null, $"Missing processor for {property}");

            Writer.WriteNewLine();

            Writer.Write($"#region {property.Name}");

            WriteAttributesComments((EPropertyFlags)property.Flags, "UPROPERTY", property.Metas);

            Writer.Write($"public {processor!.GetCSharpTypeName(property, ELocalUsageScenarioType.StructView | ELocalUsageScenarioType.Property).PadRight(padding)} {property.SafeName}");
            {
                using var propertyScope = new ScopedCodeWriter(Writer);

                Writer.Write("get");
                {
                    using var getScope = new ScopedCodeWriter(Writer);

                    // Writer.Write($"{property.GetCSharpTypeName(Context, ELocalUsageScenarioType.StructView|ELocalUsageScenarioType.Property)} ReturnValue;");
                    var text = processor.GetInteropGetValueText(
                        property, 
                        "__returnValue",
                        "NativePtr",
                        $"{Type.CppName}MetaData.{property.Name}_Offset",
                        ELocalUsageScenarioType.StructView | ELocalUsageScenarioType.Property
                    );

                    if(text.IsNotNullOrEmpty())
                    {
                        Writer.Write($"var {text};");
                    }
                    else if(property.IsDelegateProperty)
                    {
                        Writer.Write($"var __returnValue = new ({Type.CppName}MetaData.{property.Name}_Property, IntPtr.Add(NativePtr, MetaData.{property.Name}_Offset));");
                    }
                        
                    Writer.WriteNewLine();

                    Writer.Write("return __returnValue;");
                }

                if(processor.AllowWritePropertySetter(property))
                {
                    Writer.Write("set");
                    {
                        using var setScope = new ScopedCodeWriter(Writer);

                        var text = processor.GetInteropSetValueText(
                            property, 
                            "value", 
                            "NativePtr",
                            $"{Type.CppName}MetaData.{property.Name}_Offset",
                            ELocalUsageScenarioType.StructView | ELocalUsageScenarioType.Property
                        );

                        Writer.Write($"{text};");
                    }
                }                    
            }

            Writer.Write("#endregion");
        }

        Writer.Write("#endregion");
    }

    /// <summary>
    /// Exports the interop methods.
    /// </summary>
    private void ExportInteropMethods()
    {
        Writer.WriteNewLine();

        Writer.Write("#region Interop");

        Writer.WriteNewLine();

        Writer.Write($"public static implicit operator {Type.CppName}(StructView view)");
        {
            using var funcScope = new ScopedCodeWriter(Writer);

            Writer.Write("return FromNative(view.NativePtr, 0);");
        }

        Writer.WriteNewLine();
        Writer.Write($"public {Type.CppName} To{Type.Name}()");
        {
            using var funcScope = new ScopedCodeWriter(Writer);

            Writer.Write("return FromNative(NativePtr, 0);");
        }

        Writer.WriteNewLine();
        Writer.Write($"public void From{Type.Name}(ref {Type.CppName} value)");
        {
            using var funcScope = new ScopedCodeWriter(Writer);

            Writer.Write("ToNative(NativePtr, 0, ref value);");
        }

        Writer.Write("#endregion");
    }
    #endregion

    #region Interop Policy
    /// <summary>
    /// Exports the interop policy.
    /// </summary>
    private void ExportInteropPolicy()
    {
        Writer.WriteNewLine();
        Writer.Write("#region Interop Policy");

        Writer.WriteComment($"Class {Type.CppName}InteropPolicy");
        Writer.Write("[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]");
        Writer.Write($"{BaseTypeAccessPermission} class {Type.CppName}InteropPolicy : IInteropPolicy<{Type.CppName}>");
        {
            using var enumInteropPolicyScope = new ScopedCodeWriter(Writer);

            Writer.WriteComment($"Create a new instance of the <see cref=\"{Type.CppName}\"/> struct from the Native pointer");
            Writer.Write($"public {Type.CppName} Read(IntPtr address)");
            {
                using var scope = new ScopedCodeWriter(Writer);

                Writer.Write($"return {Type.CppName}.FromNative(address, 0);");
            }

            Writer.WriteNewLine();

            Writer.WriteComment("Write the struct value to the Native pointer");
            Writer.Write($"public void Write(IntPtr address, {Type.CppName} value)");
            {
                using var scope = new ScopedCodeWriter(Writer);

                Writer.Write($"{Type.CppName}.ToNative(address, 0, ref value);");
            }
        }

        Writer.Write("#endregion");
    }
    #endregion
}