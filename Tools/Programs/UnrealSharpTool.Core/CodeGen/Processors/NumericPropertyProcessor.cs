using System.Globalization;
using UnrealSharp.Utils.Extensions;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen.Processors;

/// <summary>
/// Class NumericPropertyProcessor.
/// Implements the <see cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
/// </summary>
/// <seealso cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
[PropertyProcessPolicy]
internal class NumericPropertyProcessor : PropertyProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NumericPropertyProcessor"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public NumericPropertyProcessor(BindingContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets the matched type class.
    /// </summary>
    /// <returns>System.String[].</returns>
    public override string[] GetMatchedTypeClass()
    {
        return
        [
            "Int8Property",
            "ByteProperty",
            "Int16Property",
            "UInt16Property",
            "IntProperty",
            "UInt32Property",
            "Int64Property",
            "UInt64Property",
            "FloatProperty",
            "DoubleProperty"
        ];
    }

    /// <summary>
    /// Gets the name of the c sharp type.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>System.String.</returns>
    public override string GetCSharpTypeName(PropertyDefinition property, ELocalUsageScenarioType usage)
    {
        return property.TypeClass switch
        {
            "IntProperty" => "int",
            "UInt32Property" => "uint",
            "Int64Property" => "Int64",
            "UInt64Property" => "UInt64",
            "Int8Property" => "sbyte",
            "ByteProperty" when property.IsByteEnum => property.ByteEnumName,
            "ByteProperty" => "byte",
            "Int16Property" => "short",
            "UInt16Property" => "ushort",
            "FloatProperty" => "float",
            "DoubleProperty" => "double",
            _ => base.GetCSharpTypeName(property, usage)
        };
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
        return property.IsByteEnum
            ? $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}InteropUtils.GetEnum<{property.GetCSharpTypeName(Context)}>({address}, {offset}, {property.Size})"
            : $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}InteropUtils.GetNumeric<{property.GetCSharpTypeName(Context)}>({address}, {offset})";
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
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (property.IsByteEnum)
        {
            return
                $"InteropUtils.SetEnum<{property.GetCSharpTypeName(Context)}>({address}, {offset}, {property.Size}, {variable})";
        }

        return $"InteropUtils.SetNumeric({address}, {offset}, {variable})";
    }

    /// <summary>
    /// Decorates the default value text.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="value">The value.</param>
    /// <param name="usage">The usage.</param>
    /// <returns>System.Nullable&lt;System.String&gt;.</returns>
    public override string? DecorateDefaultValueText(PropertyDefinition property, string value,
        ELocalUsageScenarioType usage)
    {
        if (usage.HasFlag(ELocalUsageScenarioType.Field))
        {
            if (value.All(x => x is '0' or '.'))
            {
                return null;
            }
        }

        switch (property.TypeClass)
        {
            case "IntProperty":
            {
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                {
                    return value.Trim();
                }

                break;
            }
            case "UInt32Property":
            {
                if (uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                {
                    return value.Trim();
                }

                break;
            }
            case "Int64Property":
            {
                if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                {
                    return value.Trim();
                }

                break;
            }
            case "UInt64Property":
            {
                if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                {
                    return value.Trim();
                }

                break;
            }
            case "Int8Property":
            {
                if (sbyte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                {
                    return value.Trim();
                }

                break;
            }
            case "Int16Property":
            {
                if (short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                {
                    return value.Trim();
                }

                break;
            }
            case "UInt16Property":
            {
                if (ushort.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                {
                    return value.Trim();
                }

                break;
            }
            case "FloatProperty":
            {
                if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                {
                    return $"{value}f";
                }

                break;
            }
            case "DoubleProperty":
            {
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                {
                    return $"{value}d";
                }

                break;
            }
            default:
            {
                if (property.IsByteEnum)
                {
                    return value == "(INVALID)" ? null : $"{property.ByteEnumName}.{value}";
                }

                if (property.TypeClass == "ByteProperty")
                {
                    return property.IsByteEnum ? property.ByteEnumName : value;
                }

                break;
            }
        }

        return value;
    }
}