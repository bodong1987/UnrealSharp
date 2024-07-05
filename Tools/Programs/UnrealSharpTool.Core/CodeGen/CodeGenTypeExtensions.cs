using System.Text;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen;

/// <summary>
/// Class CodeGenTypeExtensions.
/// </summary>
public static class CodeGenTypeExtensions
{
    /// <summary>
    /// Gets the name of the return type.
    /// </summary>
    /// <param name="function">The function.</param>
    /// <param name="context">The context.</param>
    /// <returns>System.String.</returns>
    public static string GetReturnTypeName(this FunctionTypeDefinition function, BindingContext context)
    {
        var rType = function.GetReturnType();

        if (rType == null)
        {
            return "void";
        }

        return rType.IsByteEnum ?
            // force convert some byte property to enum
            rType.ByteEnumName : rType.GetCSharpTypeName(context, ELocalUsageScenarioType.Method|ELocalUsageScenarioType.ReturnValue);
    }

    /// <summary>
    /// Gets the export parameters.
    /// </summary>
    /// <param name="function">The function.</param>
    /// <param name="context">The context.</param>
    /// <param name="withDefaultValue">if set to <c>true</c> [with default value].</param>
    /// <returns>System.String.</returns>
    public static string GetExportParameters(this FunctionTypeDefinition function, BindingContext context, bool withDefaultValue = false)
    {
        var stringBuilder = new StringBuilder();

        var outTag = function.IsEvent || context.SchemaType == EBindingSchemaType.NativeBinding ? "ref " : "out ";

        var stopSearchDefaultValue = !withDefaultValue;
            
        foreach (var p in function.Properties.FindAll(x => !x.IsReturnParam).Reverse<PropertyDefinition>())
        {
            var type = p.GetCSharpTypeName(context, ELocalUsageScenarioType.Parameter|ELocalUsageScenarioType.Method);
            var name = p.SafeName;

            // ReSharper disable once ArrangeRedundantParentheses
            var referenceFlag = p is { IsOutParam: true, IsConstParam: false } ? outTag : (p.IsReference ? "ref " : "");
            var defaultValueTag = "";

            if(!stopSearchDefaultValue)
            {
                var value = function.Metas.GetMeta($"CPP_Default_{p.Name}");
                if(value != null)
                {
                    var processor = context.GetProcessor(p);

                    Logger.EnsureNotNull(processor);

                    value = processor.DecorateDefaultValueText(p, value, ELocalUsageScenarioType.Method|ELocalUsageScenarioType.Parameter);

                    if(value.IsNotNullOrEmpty())
                    {
                        defaultValueTag = $" = {value}";
                    }
                    else
                    {
                        stopSearchDefaultValue = true;
                    }
                }
                else
                {
                    stopSearchDefaultValue = true;
                }
            }

            stringBuilder.Insert(0, $", {referenceFlag}{type} {name}{defaultValueTag}");
        }

        var result = stringBuilder.ToString();

        return result.StartsWith(", ") ? result[2..] : result;
    }

    /// <summary>
    /// Gets the export invoke parameters.
    /// </summary>
    /// <param name="function">The function.</param>
    /// <param name="context">The context.</param>
    /// <returns>System.String.</returns>
    public static string GetExportInvokeParameters(this FunctionTypeDefinition function, BindingContext context)
    {
        var stringBuilder = new StringBuilder();

        var outTag = function.IsEvent || context.SchemaType == EBindingSchemaType.NativeBinding ? "ref " : "out ";

        foreach (var p in function.Properties.FindAll(x => !x.IsReturnParam))
        {
            // var type = p.GetCSharpTypeName(context, ELocalUsageScenarioType.Parameter | ELocalUsageScenarioType.Method);
                
            var name = p.SafeName;

            var semicolon = p == function.Properties.First() ? "" : ", ";

            // ReSharper disable once ArrangeRedundantParentheses
            var referenceFlag = p is { IsOutParam: true, IsConstParam: false } ? outTag : (p.IsReference ? "ref " : "");

            stringBuilder.Append($"{semicolon}{referenceFlag}{name}");
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Gets the name of the c sharp type.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="context">The context.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>System.String.</returns>
    public static string GetCSharpTypeName(this PropertyDefinition property, BindingContext context, ELocalUsageScenarioType usage = ELocalUsageScenarioType.Common)
    {
        var processor = context.GetProcessor(property);

        Logger.EnsureNotNull(processor, $"Failed find processor for property:{property}");

        return processor.GetCSharpTypeName(property, usage);
    }

    /// <summary>
    /// Gets the name of the inner property c sharp type.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="context">The context.</param>
    /// <param name="index">The index.</param>
    /// <returns>System.String.</returns>
    public static string GetInnerPropertyCSharpTypeName(this PropertyDefinition property, BindingContext context, int index)
    {
        Logger.Assert(index >= 0 && index < property.InnerProperties.Count);

        if (index >= 0 && index < property.InnerProperties.Count)
        {
            return property.InnerProperties[index].GetCSharpTypeName(context);
        }

        return "";
    }
}