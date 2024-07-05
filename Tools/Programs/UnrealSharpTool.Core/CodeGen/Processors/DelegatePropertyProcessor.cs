using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen.Processors;

/// <summary>
/// Class DelegatePropertyProcessor.
/// Implements the <see cref="UnrealSharpTool.Core.CodeGen.Processors.CollectionPropertyProcessor" />
/// </summary>
/// <seealso cref="UnrealSharpTool.Core.CodeGen.Processors.CollectionPropertyProcessor" />
[PropertyProcessPolicy]
internal class DelegatePropertyProcessor : CollectionPropertyProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DelegatePropertyProcessor"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public DelegatePropertyProcessor(BindingContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets the matched type class.
    /// </summary>
    /// <returns>string[].</returns>
    public override string[] GetMatchedTypeClass()
    {
        return [
            "DelegateProperty",
            "MulticastInlineDelegateProperty",
            "MulticastSparseDelegateProperty"
        ];
    }

    /// <summary>
    /// Gets the name of the c sharp type.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>string.</returns>
    public override string GetCSharpTypeName(PropertyDefinition property, ELocalUsageScenarioType usage)
    {
        if(Context.SchemaType == EBindingSchemaType.CSharpBinding)
        {
            return $"F{property.Name}Delegate?";
        }

        var parentCppName = property.Parent != null ? property.Parent.CppName! : "Z";
        var classScope = usage.HasFlag(ELocalUsageScenarioType.Interface) ? $"{parentCppName}." : "";
        
        return property.IsDelegateProperty
            ? $"TDelegate<{classScope}{parentCppName}_{property.Name}_Signature>?"
            : $"TMulticastDelegate<{classScope}{parentCppName}_{property.Name}_Signature>?";
    }

    /// <summary>
    /// Should export property in meta fields.
    /// </summary>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="propertyDefinition">The property definition.</param>
    /// <returns><c>true</c> success, <c>false</c> otherwise.</returns>
    public override bool ShouldExportPropertyInMetaFields(StructTypeDefinition typeDefinition, PropertyDefinition propertyDefinition)
    {
        return true;
    }

    /// <summary>
    /// Before the class property write.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="property">The property.</param>
    public override void BeforeClassPropertyWrite(CodeWriter writer, PropertyDefinition property)
    {
        var parentCppName = property.Parent != null ? property.Parent.CppName! : "Z";
        const string classScope = "";
        var defaultValue = "";

        if(Context.SchemaType == EBindingSchemaType.CSharpBinding)
        {
            defaultValue = $" = new((owner, propertyPtr, addressPtr)=> new F{property.Name}Delegate(owner, propertyPtr, addressPtr))";
        }

        writer.Write("[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
        writer.Write($"private {(property.IsDelegateProperty? "TDelegatePropertyCache": "TMulticastDelegatePropertyCache")}<{classScope}{parentCppName}_{property.Name}_Signature> Z_{property.Name}Private{defaultValue};");
    }

    /// <summary>
    /// Gets the interop get value text.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="variable">The variable.</param>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>string.</returns>
    public override string GetInteropGetValueText(
        PropertyDefinition property,
        string variable,
        string address,
        string offset,
        ELocalUsageScenarioType usage)
    {
        return "";
    }

    /// <summary>
    /// Gets the interop set value text.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="variable">The variable.</param>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>string.</returns>
    public override string GetInteropSetValueText(
        PropertyDefinition property,
        string variable,
        string address,
        string offset,
        ELocalUsageScenarioType usage
    )
    {
        return "";
    }

    /// <summary>
    /// Allows the write property setter.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns><c>true</c> success, <c>false</c> otherwise.</returns>
    public override bool AllowWritePropertySetter(PropertyDefinition property)
    {
        return false;
    }

    /// <summary>
    /// Handles the class property write.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="property">The property.</param>
    /// <returns><c>true</c> success, <c>false</c> otherwise.</returns>
    public override bool HandleClassPropertyWrite(CodeWriter writer, PropertyDefinition property)
    {
        writer.Write("get");
        {
            using var getScope = new ScopedCodeWriter(writer);

            var ownerTag = Context.SchemaType == EBindingSchemaType.CSharpBinding ? "this, " : "";
            var asTag = Context.SchemaType == EBindingSchemaType.CSharpBinding ? $" as F{property.Name}Delegate" : "";
            writer.Write($"return Z_{property.Name}Private.Get({ownerTag}GetNativePtr(), {property.Parent!.CppName}MetaData.{property.Name}_Offset, {property.Parent!.CppName}MetaData.{property.Name}_Property){asTag};");
        }

        return true;
    }
}