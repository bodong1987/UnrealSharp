using UnrealSharp.Utils.Extensions;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen.Processors
{
    /// <summary>
    /// Class NumericPropertyProcessor.
    /// Implements the <see cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
    /// </summary>
    /// <seealso cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
    [PropertyProcessPolicy]
    class NumericPropertyProcessor : PropertyProcessor
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
            return [ 
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
            if(property.TypeClass == "IntProperty")
            {
                return "int";
            }
            else if(property.TypeClass == "UInt32Property")
            {
                return "uint";
            }
            else if(property.TypeClass == "Int64Property")
            {
                return "Int64";
            }
            else if(property.TypeClass == "UInt64Property")
            {
                return "UInt64";
            }
            else if( property.TypeClass == "Int8Property")
            {
                return "sbyte";
            }
            else if(property.TypeClass == "ByteProperty")
            {
                if(property.IsByteEnum)
                {
                    return property.ByteEnumName;
                }

                return "byte";
            }
            else if(property.TypeClass == "Int16Property")
            {
                return "short";
            }
            else if(property.TypeClass == "UInt16Property")
            {
                return "ushort";
            }
            else if(property.TypeClass == "FloatProperty")
            {
                return "float";
            }
            else if(property.TypeClass == "DoubleProperty")
            {
                return "double";
            }

            return base.GetCSharpTypeName(property);
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
            if (property.IsByteEnum)
            {
                return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}InteropUtils.GetEnum<{property.GetCSharpTypeName(Context)}>({address}, {offset}, {property.Size})";
            }

            return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}InteropUtils.GetNumeric<{property.GetCSharpTypeName(Context)}>({address}, {offset})";
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
            if (property.IsByteEnum)
            {
                return $"InteropUtils.SetEnum<{property.GetCSharpTypeName(Context)}>({address}, {offset}, {property.Size}, {variable})";
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
        public override string? DecorateDefaultValueText(PropertyDefinition property, string value, ELocalUsageScenarioType usage)
        {
            if(usage.HasFlag(ELocalUsageScenarioType.Field))
            {
                if(value.All(x=>x == '0' || x== '.'))
                {
                    return null;
                }
            }

            if (property.TypeClass == "IntProperty")
            {
                if(int.TryParse(value, out _))
                {
                    return value.Trim();
                }
            }
            else if (property.TypeClass == "UInt32Property")
            {
                if(uint.TryParse(value, out _))
                {
                    return value.Trim();
                }
            }
            else if (property.TypeClass == "Int64Property")
            {
                if(Int64.TryParse(value, out _))
                {
                    return value.Trim();
                }
            }
            else if (property.TypeClass == "UInt64Property")
            {
                if(UInt64.TryParse(value, out _))
                {
                    return value.Trim();
                }
            }
            else if (property.TypeClass == "Int8Property")
            {
                if(sbyte.TryParse(value, out _))
                {
                    return value.Trim();
                }
            }            
            else if (property.TypeClass == "Int16Property")
            {
                if(short.TryParse(value, out _))
                {
                    return value.Trim();
                }
            }
            else if (property.TypeClass == "UInt16Property")
            {
                if(ushort.TryParse(value, out _))
                {
                    return value.Trim();
                }
            }
            else if (property.TypeClass == "FloatProperty")
            {
                if (float.TryParse(value, out _))
                {
                    return $"{value}f";
                }
            }
            else if (property.TypeClass == "DoubleProperty")
            {
                if(double.TryParse(value, out _))
                {
                    return $"{value}d";
                }
            }
            else if(property.IsByteEnum)
            {
                if (value == "(INVALID)")
                {
                    return null;
                }

                return $"{property.ByteEnumName}.{value}";
            }
            else if (property.TypeClass == "ByteProperty")
            {
                if (property.IsByteEnum)
                {
                    return property.ByteEnumName;
                }

                return value.ToString();
            }

            return value;
        }
    }
}
