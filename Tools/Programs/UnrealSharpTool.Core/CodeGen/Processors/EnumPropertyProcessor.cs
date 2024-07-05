using UnrealSharp.Utils.Extensions;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen.Processors;

/// <summary>
/// Class EnumPropertyProcessor.
/// Implements the <see cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
/// </summary>
/// <seealso cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
[PropertyProcessPolicy]
internal class EnumPropertyProcessor : PropertyProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumPropertyProcessor"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public EnumPropertyProcessor(BindingContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets the matched type class.
    /// </summary>
    /// <returns>System.String[].</returns>
    public override string[] GetMatchedTypeClass()
    {
        return ["EnumProperty"];
    }

    /// <summary>
    /// Gets the name of the c sharp type.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>System.String.</returns>
    public override string GetCSharpTypeName(PropertyDefinition property, ELocalUsageScenarioType usage)
    {
        return property.IsByteEnum ? property.ByteEnumName : base.GetCSharpTypeName(property, usage);
    }

    /// <summary>
    /// Gets the interop get value text.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="variable">The variable.</param>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>System.String.</returns>
    public override string GetInteropGetValueText(
        PropertyDefinition property,
        string variable,
        string address,
        string offset,
        ELocalUsageScenarioType usage
    )
    {
        return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}InteropUtils.GetEnum<{property.GetCSharpTypeName(Context)}>({address}, {offset}, {property.Size})";
    }

    /// <summary>
    /// Gets the interop set value text.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="variable">The variable.</param>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>System.String.</returns>
    public override string GetInteropSetValueText(
        PropertyDefinition property,
        string variable,
        string address,
        string offset,
        ELocalUsageScenarioType usage
    )
    {
        return $"InteropUtils.SetEnum<{property.GetCSharpTypeName(Context)}>({address}, {offset}, {property.Size}, {variable})";
    }

    /// <summary>
    /// Decorates the default value text.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="value">The value.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>System.Nullable&lt;System.String&gt;.</returns>
    public override string? DecorateDefaultValueText(PropertyDefinition property, string value, ELocalUsageScenarioType usage)
    {
        if(value == "(INVALID)")
        {
            return null;
        }

        var texts = value.Split("|");

        return string.Join("|",
            property.IsByteEnum
                ? texts.Select(x => $"{property.ByteEnumName}.{x}")
                : texts.Select(x => $"{property.CppTypeName}.{x}"));
    }

    public static string GetEnumUnderlyingType(PropertyDefinition property)
    {
        return property.Size switch
        {
            sizeof(int) => "int",
            sizeof(long) => "Int64",
            sizeof(short) => "short",
            _ => "byte"
        };
    }

    public override string GetFastInvokeDelegateParameterTypeName(PropertyDefinition property, ELocalUsageScenarioType localUsageScenarioType)
    {
        var type = GetEnumUnderlyingType(property);

        return property.IsPassByReferenceInCpp ? $"{type}*" : type;
    }

    public override string GetBeforeFastInvokeText(PropertyDefinition property)
    {
        var type = GetEnumUnderlyingType(property);
        return $"var __valueOf{property.Name} = ({type}){property.Name};";
    }

    public override string GetPostFastInvokeText(PropertyDefinition property)
    {
        return property.IsPassByReferenceInCpp
            ? $"{property.Name} = InteropUtils.GetEnum<{GetCSharpTypeName(property, ELocalUsageScenarioType.GenericArgument)}>(new IntPtr(&__valueOf{property.Name}), 0, {property.Size});"
            : string.Empty;
    }

    public override string GetFastInvokeParameterName(PropertyDefinition property)
    {
        return property.IsPassByReferenceInCpp ? $"&__valueOf{property.Name}" : $"__valueOf{property.Name}";
    }

    public override string DecorateFastInvokeReturnValue(PropertyDefinition property, string returnValue)
    {
        return $"({GetCSharpTypeName(property, ELocalUsageScenarioType.GenericArgument)}){returnValue}";
    }
}