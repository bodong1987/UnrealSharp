using UnrealSharp.Utils.Extensions;
using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen.Processors;

/// <summary>
/// Class SoftObjectPropertyProcessor.
/// Implements the <see cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
/// </summary>
/// <seealso cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
[PropertyProcessPolicy]
internal class SoftObjectPropertyProcessor : PropertyProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SoftObjectPropertyProcessor"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public SoftObjectPropertyProcessor(BindingContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets the matched type class.
    /// </summary>
    /// <returns>System.String[].</returns>
    public override string[] GetMatchedTypeClass()
    {
        return ["SoftObjectProperty"];
    }

    /// <summary>
    /// Gets the name of the element type.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>System.String.</returns>
    public string GetElementTypeName(PropertyDefinition property)
    {
        if(property.CppTypeName == "TSoftObjectPtr`1")
        {
            return property.InnerProperties[0].CppTypeName!;
        }

        return !property.CppTypeName!.StartsWith("TSoftObjectPtr<")
            ? property.GetCSharpTypeName(Context, ELocalUsageScenarioType.GenericArgument)
            : property.CppTypeName!.Trim()["TSoftObjectPtr<".Length..].Trim('<', '>', ' ');
    }

    /// <summary>
    /// Gets the name of the c sharp type.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>System.String.</returns>
    public override string GetCSharpTypeName(PropertyDefinition property, ELocalUsageScenarioType usage)
    {
        if(property.CppTypeName == "TSoftObjectPtr`1")
        {
            return usage.HasFlag(ELocalUsageScenarioType.GenericArgument) ? $"TSoftObjectPtr<{GetElementTypeName(property)}>" : $"TSoftObjectPtr<{GetElementTypeName(property)}>?";
        }

        if(usage.HasFlag(ELocalUsageScenarioType.GenericArgument))
        {
            return property.CppTypeName!;
        }

        return property.CppTypeName + "?";
    }

    /// <summary>
    /// Before the class property write.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="property">The property.</param>
    public override void BeforeClassPropertyWrite(CodeWriter writer, PropertyDefinition property)
    {
        base.BeforeClassPropertyWrite(writer, property);

        writer.Write("[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
        writer.Write($"private TSoftObjectPtrCache<{GetElementTypeName(property)}> Z_{property.Name}Cache;");
        writer.WriteNewLine();
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
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (usage.HasFlag(ELocalUsageScenarioType.PropertyGetter))
        {
            return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}Z_{property.Name}Cache.Get({address}, {offset})";
        }

        return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}InteropUtils.GetSoftObjectPtr<{GetElementTypeName(property)}>({address}, {offset})";
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
        return $"InteropUtils.SetSoftObjectPtr({address}, {offset}, {variable})";
    }
}