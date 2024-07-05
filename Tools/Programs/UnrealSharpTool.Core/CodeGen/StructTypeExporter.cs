using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;
using UnrealSharpTool.Core.Utils;
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace UnrealSharpTool.Core.CodeGen;

/// <summary>
/// Class StructTypeExporter.
/// Base exporter for ScriptStruct and Classes
/// Implements the <see cref="UnrealSharpTool.Core.CodeGen.BaseTypeExporter" />
/// </summary>
/// <seealso cref="UnrealSharpTool.Core.CodeGen.BaseTypeExporter" />
public abstract class StructTypeExporter : BaseTypeExporter
{
    #region Properties
    /// <summary>
    /// The structure type
    /// </summary>
    // ReSharper disable once MemberCanBeProtected.Global
    public readonly StructTypeDefinition StructType;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="StructTypeExporter"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="targetDirectory">The target directory.</param>
    /// <param name="typeDefinition">The type definition.</param>
    protected StructTypeExporter(BindingContext context, string targetDirectory, StructTypeDefinition typeDefinition) :
        base(context, targetDirectory, typeDefinition)
    {
        StructType = typeDefinition;
    }
    #endregion

    #region Template Methods Override
    /// <summary>
    /// Determines whether [is meta accept in main comment] [the specified key].
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns><c>true</c> if [is meta accept in main comment] [the specified key]; otherwise, <c>false</c>.</returns>
    protected override bool IsMetaAcceptInMainComment(string key)
    {
        return StructType.Properties.Find(x => key.StartsWith(x.Name!)) == null;
    }

    /// <summary>
    /// Writes the extra namespaces.
    /// </summary>
    protected override void WriteExtraNamespaces()
    {
        foreach(var ns in StructType.DependNamespaces)
        {
            Writer.Write($"using {ns};");
        }
    }
    #endregion

    #region Delegate Support
    /// <summary>
    /// Writes the delegate definitions.
    /// </summary>
    protected virtual void WriteDelegateDefinitions()
    {
        var delegateProperties = StructType.Properties.FindAll(x => x.IsDelegateRelevanceProperty);
        if(delegateProperties.Count <= 0)
        {
            return;
        }

        Writer.WriteNewLine();
        Writer.Write("#region Delegate Definitions");

        foreach(var property in delegateProperties)
        {
            Logger.EnsureNotNull(property.SignatureFunction);
            var function = property.SignatureFunction;

            var returnType = function.GetReturnTypeName(Context);
            var delegateName = $"{property.Parent!.CppName}_{property.Name}_Signature";
            var paramList = function.GetExportParameters(Context);

            Writer.WriteComment($"Delegate definition of {delegateName}");

            Writer.Write($"public delegate {returnType} {delegateName}({paramList});");

            if(Context.SchemaType == EBindingSchemaType.CSharpBinding)
            {
                WriteDelegateDefinition(property, delegateName);
            }
        }

        Writer.Write("#endregion");
    }

    /// <summary>
    /// Writes the delegate definition.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="delegateSignatureTypeName">Name of the delegate signature type.</param>
    protected virtual void WriteDelegateDefinition(PropertyDefinition property, string delegateSignatureTypeName)
    {
        Logger.EnsureNotNull(property.SignatureFunction);

        var baseClassType = property.IsDelegateProperty ? "TDelegate" : "TMulticastDelegate";
        var functionName = property.IsDelegateProperty ? "Invoke" : "Broadcast";            

        Writer.Write($"public class F{property.Name}Delegate : {baseClassType}<{delegateSignatureTypeName}>");
        {
            using var delegateClassScope = new ScopedCodeWriter(Writer);

            WritePropertyMetaCache(
                property.SignatureFunction,
                $"F{property.Name}DelegateMetaData", 
                property.SignatureFunction.Properties, 
                ELocalUsageScenarioType.Delegate
            );

            Writer.Write("// ReSharper disable once ConvertToPrimaryConstructor");
            Writer.Write($"public F{property.Name}Delegate(UObject owner, IntPtr propertyPtr, IntPtr addressPtr) : base(owner, propertyPtr, addressPtr)");
            {
                using var constructorScope = new ScopedCodeWriter(Writer);
            }

            Writer.Write($"public void {functionName}({property.SignatureFunction.GetExportParameters(Context, true)})");
            {
                using var invokeScope = new ScopedCodeWriter(Writer);

                Writer.Write("var __invocation = Invocation;");
                Writer.Write("unsafe");
                {
                    using var unsafeScope = new ScopedCodeWriter(Writer);

                    if (property.SignatureFunction.Properties.Count > 0)
                    {
                        Writer.Write("var __paramBufferPointer = stackalloc byte[__invocation.ParamSize];");
                        Writer.WriteNewLine();

                        Writer.Write("using var __scopedInvoker = new ScopedUnrealInvocation(__invocation, (IntPtr)__paramBufferPointer);");
                    }

                    foreach (var paramProperty in property.SignatureFunction!.Properties)
                    {
                        if (property.IsReturnParam)
                        {
                            continue;
                        }

                        var processor = Context.GetProcessor(paramProperty);

                        Logger.EnsureNotNull(processor, $"Failed find property {paramProperty}");

                        var text = processor.GetInteropSetValueText(
                            paramProperty,
                            paramProperty.SafeName,
                            "(IntPtr)__paramBufferPointer",
                            $"F{property.Name}DelegateMetaData.{paramProperty.Name}_Offset",
                            ELocalUsageScenarioType.Common
                        );

                        Writer.Write($"{text};");
                    }

                    Writer.WriteNewLine();
                    var paramAddress = property.SignatureFunction.Properties.Count > 0 ? "(IntPtr)__paramBufferPointer" : "IntPtr.Zero";
                    Writer.Write($"__invocation.Invoke(Owner, {paramAddress});");
                }
            }
        }
    }
    #endregion

    #region Property Meta Caches

    /// <summary>
    /// Should write property field.
    /// </summary>
    /// <param name="typeDefinition"></param>
    /// <param name="propertyDefinition">The property definition.</param>
    /// <returns><c>true</c>need do that <c>false</c> otherwise.</returns>
    protected bool ShouldWritePropertyField(StructTypeDefinition typeDefinition, PropertyDefinition propertyDefinition)
    {
        var processor = Context.GetProcessor(propertyDefinition);
        Logger.EnsureNotNull(processor);

        return processor.ShouldExportPropertyInMetaFields(typeDefinition, propertyDefinition);
    }

    /// <summary>
    /// Writes the property meta cache.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="name">The name.</param>
    /// <param name="properties">The properties.</param>
    /// <param name="usage">The usage.</param>
    /// <param name="forceExport">if set to <c>true</c> [force export].</param>
    protected virtual void WritePropertyMetaCache(
        StructTypeDefinition typeDefinition, 
        string name,
        IEnumerable<PropertyDefinition> properties, 
        ELocalUsageScenarioType usage, 
        bool forceExport = false
    )
    {
        // ignore offset export
        var propertyDefinitions = properties.ToList();
        if(!forceExport && propertyDefinitions.Count == 0 && !typeDefinition.IsFunction)
        {
            return;
        }

        Writer.WriteNewLine();
        Writer.Write("#region Property Meta Cache");
        Writer.Write($"private static class {name}");

        {
            using var classScope = new ScopedCodeWriter(Writer);

            foreach (var property in propertyDefinitions.Where(property => property.Name!.IsValidCSharpIdentifier()))
            {
                Writer.Write($"public static short {property.Name}_Offset = -1;");

                if (typeDefinition.IsFunction || ShouldWritePropertyField(typeDefinition, property))
                {
                    Writer.Write($"public static IntPtr {property.Name}_Property = IntPtr.Zero;");
                }
            }

            PostWritePropertyMetaFields(typeDefinition, usage);

            Writer.WriteNewLine();
            Writer.Write($"static {name}()");
            {
                using var staticConstructorScope = new ScopedCodeWriter(Writer);
                {
                    PostWritePropertyMetaStaticConstructor(typeDefinition, name, propertyDefinitions, usage);
                }
            }               
        }

        Writer.Write("#endregion");
    }

    /// <summary>
    /// Posts the write property meta fields.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="usage">The usage.</param>
    protected virtual void PostWritePropertyMetaFields(StructTypeDefinition typeDefinition, ELocalUsageScenarioType usage)
    {

    }

    /// <summary>
    /// Posts the write property meta static constructor.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="name">The name.</param>
    /// <param name="properties">The properties.</param>
    /// <param name="usage">The usage.</param>
    protected virtual void PostWritePropertyMetaStaticConstructor(StructTypeDefinition typeDefinition, string name, IEnumerable<PropertyDefinition> properties, ELocalUsageScenarioType usage)
    {
        if (typeDefinition.IsFunction)
        {
            Writer.Write($"MetaInteropUtils.LoadFunctionMetaCache(typeof({name}), ref {typeDefinition.Name}Invocation, StaticClass(), \"{typeDefinition.Name}\");");
        }
    }
    #endregion

    #region Padding 
    /// <summary>
    /// Calculates the property type padding.
    /// </summary>
    /// <returns>System.Int32.</returns>
    protected virtual int CalcPropertyTypePadding()
    {
        if(StructType.Properties.Count <= 0)
        {
            return 24;
        }

        var value = StructType.Properties.Select(x => x.GetCSharpTypeName(Context)).Select(x => x.Length).Max() + 1;

        return Math.Max(value, 24);
    }
    #endregion
}