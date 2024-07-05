using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen.Processors;

/// <summary>
/// Class MapPropertyProcessor.
/// Implements the <see cref="UnrealSharpTool.Core.CodeGen.Processors.CollectionPropertyProcessor" />
/// </summary>
/// <seealso cref="UnrealSharpTool.Core.CodeGen.Processors.CollectionPropertyProcessor" />
[PropertyProcessPolicy]
internal class MapPropertyProcessor : CollectionPropertyProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapPropertyProcessor"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public MapPropertyProcessor(BindingContext context) : base(context)
    {
        PropertyCacheName = "TMapPropertyCache";
    }

    /// <summary>
    /// Gets the matched type class.
    /// </summary>
    /// <returns>System.String[].</returns>
    public override string[] GetMatchedTypeClass()
    {
        return ["MapProperty"];
    }

    /// <summary>
    /// Gets the name of the c sharp type.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>System.String.</returns>
    public override string GetCSharpTypeName(PropertyDefinition property, ELocalUsageScenarioType usage)
    {
        Logger.Ensure<Exception>(property.InnerProperties.Count == 2);

        if (usage.HasFlag(ELocalUsageScenarioType.Class))
        {
            if (usage.HasFlag(ELocalUsageScenarioType.Property))
            {
                return $"TMap<{GetNotNullCSharpTypeName(property.GetInnerPropertyCSharpTypeName(Context, 0))}, {property.GetInnerPropertyCSharpTypeName(Context, 1)}>?";
            }
        }
        else if (usage.HasFlag(ELocalUsageScenarioType.StructView))
        {
            return $"TMap<{GetNotNullCSharpTypeName(property.GetInnerPropertyCSharpTypeName(Context, 0))}, {property.GetInnerPropertyCSharpTypeName(Context, 1)}>";
        }
        else if (usage.HasFlag(ELocalUsageScenarioType.Method))
        {
            return $"IDictionary<{GetNotNullCSharpTypeName(property.GetInnerPropertyCSharpTypeName(Context, 0))}, {property.GetInnerPropertyCSharpTypeName(Context, 1)}>?";
        }

        return $"Dictionary<{GetNotNullCSharpTypeName(property.GetInnerPropertyCSharpTypeName(Context, 0))}, {property.GetInnerPropertyCSharpTypeName(Context, 1)}>?";
    }

    /// <summary>
    /// Befores the class property write.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="property">The property.</param>
    public override void BeforeClassPropertyWrite(CodeWriter writer, PropertyDefinition property)
    {
        Logger.Ensure<Exception>(property.InnerProperties.Count == 2);

        writer.Write("[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
        writer.Write($"private {PropertyCacheName}<{GetNotNullCSharpTypeName(property.GetInnerPropertyCSharpTypeName(Context, 0))}, {property.GetInnerPropertyCSharpTypeName(Context, 1)}> Z_{property.Name}Private;");
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
        ELocalUsageScenarioType usage)
    {
        if (property.IsClassProperty)
        {
            return base.GetInteropGetValueText(property, variable, address, offset, usage);
        }

        var keyProcessor = Context.GetProcessor(property.InnerProperties[0]);
        Logger.EnsureNotNull(keyProcessor, "Failed find processor for map key element.");

        var valueProcessor = Context.GetProcessor(property.InnerProperties[1]);
        Logger.EnsureNotNull(valueProcessor, "Failed find processor for map value element.");

        var constantsName = property.IsFunctionProperty ? $"{property.Parent!.Name}MetaData" : $"{property.Parent!.CppName}MetaData";

        return usage.HasFlag(ELocalUsageScenarioType.StructView)
            ? $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}new TMap<{GetNotNullCSharpTypeName(property.GetInnerPropertyCSharpTypeName(Context, 0))}, {property.GetInnerPropertyCSharpTypeName(Context, 1)}>({address}, {offset}, {constantsName}.{property.Name}_Property)"
            : $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}TMap<{GetNotNullCSharpTypeName(property.GetInnerPropertyCSharpTypeName(Context, 0))}, {property.GetInnerPropertyCSharpTypeName(Context, 1)}>.FromNative({address}, {offset}, {constantsName}.{property.Name}_Property)"
            ;
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
        if (property.IsClassProperty)
        {
            return base.GetInteropSetValueText(property, variable, address, offset, usage);
        }

        var keyProcessor = Context.GetProcessor(property.InnerProperties[0]);
        Logger.EnsureNotNull(keyProcessor, "Failed find processor for map key element.");

        var valueProcessor = Context.GetProcessor(property.InnerProperties[1]);
        Logger.EnsureNotNull(valueProcessor, "Failed find processor for map value element.");

        Logger.EnsureNotNull(property.Parent, $"{property}'s parent can't be null!");

        var constantsName = property.IsFunctionProperty ? $"{property.Parent!.Name}MetaData" : $"{property.Parent!.CppName}MetaData";

        return $"TMap<{GetNotNullCSharpTypeName(property.GetInnerPropertyCSharpTypeName(Context, 0))}, {property.GetInnerPropertyCSharpTypeName(Context, 1)}>.ToNative({address}, {offset}, {constantsName}.{property.Name}_Property, {variable})";
    }
}