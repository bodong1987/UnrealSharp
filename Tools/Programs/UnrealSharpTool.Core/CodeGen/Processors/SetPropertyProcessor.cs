using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen.Processors;

/// <summary>
/// Class SetPropertyProcessor.
/// Implements the <see cref="UnrealSharpTool.Core.CodeGen.Processors.CollectionPropertyProcessor" />
/// </summary>
/// <seealso cref="UnrealSharpTool.Core.CodeGen.Processors.CollectionPropertyProcessor" />
[PropertyProcessPolicy]
internal class SetPropertyProcessor : CollectionPropertyProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetPropertyProcessor"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public SetPropertyProcessor(BindingContext context) : base(context)
    {
        PropertyCacheName = "TSetPropertyCache";
    }

    /// <summary>
    /// Gets the matched type class.
    /// </summary>
    /// <returns>System.String[].</returns>
    public override string[] GetMatchedTypeClass()
    {
        return ["SetProperty"];
    }

    /// <summary>
    /// Gets the name of the c sharp type.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>System.String.</returns>
    public override string GetCSharpTypeName(PropertyDefinition property, ELocalUsageScenarioType usage)
    {
        Logger.Ensure<Exception>(property.InnerProperties.Count == 1);

        if (usage.HasFlag(ELocalUsageScenarioType.Class))
        {
            if (usage.HasFlag(ELocalUsageScenarioType.Property))
            {
                return $"TSet<{property.GetInnerPropertyCSharpTypeName(Context, 0)}>?";
            }
        }
        else if (usage.HasFlag(ELocalUsageScenarioType.StructView))
        {
            return $"TSet<{property.GetInnerPropertyCSharpTypeName(Context, 0)}>";
        }
        else if (usage.HasFlag(ELocalUsageScenarioType.Method))
        {
            return $"ISet<{property.GetInnerPropertyCSharpTypeName(Context, 0)}>?";
        }

        return $"HashSet<{property.GetInnerPropertyCSharpTypeName(Context, 0)}>?";
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

        var innerProcessor = Context.GetProcessor(property.InnerProperties[0]);
        Logger.EnsureNotNull(innerProcessor, "Failed find processor for set element.");

        var constantsName = property.IsFunctionProperty ? $"{property.Parent!.Name}MetaData" : $"{property.Parent!.CppName}MetaData";

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (usage.HasFlag(ELocalUsageScenarioType.StructView))
        {
            return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}new TSet<{property.GetInnerPropertyCSharpTypeName(Context, 0)}>({address}, {offset}, {constantsName}.{property.Name}_Property)";
        }

        return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}TSet<{property.GetInnerPropertyCSharpTypeName(Context, 0)}>.FromNative({address}, {offset}, {constantsName}.{property.Name}_Property)";
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
        ELocalUsageScenarioType usage)
    {
        if (property.IsClassProperty)
        {
            return base.GetInteropSetValueText(property, variable, address, offset, usage);
        }

        var processor = Context.GetProcessor(property.InnerProperties[0]);
        Logger.EnsureNotNull(processor, "Failed find processor for set element.");

        Logger.EnsureNotNull(property.Parent, $"{property}'s parent can't be null!");

        var constantsName = property.IsFunctionProperty ? $"{property.Parent!.Name}MetaData" : $"{property.Parent!.CppName}MetaData";

        return $"TSet<{property.GetInnerPropertyCSharpTypeName(Context, 0)}>.ToNative({address}, {offset}, {constantsName}.{property.Name}_Property, {variable})";
    }
}