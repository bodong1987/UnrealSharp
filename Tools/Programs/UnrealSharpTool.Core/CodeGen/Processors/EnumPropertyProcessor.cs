using UnrealSharp.Utils.Extensions;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen.Processors
{
    /// <summary>
    /// Class EnumPropertyProcessor.
    /// Implements the <see cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
    /// </summary>
    /// <seealso cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
    [PropertyProcessPolicy]
    class EnumPropertyProcessor : PropertyProcessor
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
            if(property.IsByteEnum)
            {
                return property.ByteEnumName;
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

            if (property.IsByteEnum)
            {
                return string.Join("|", texts.Select(x => $"{property.ByteEnumName}.{x}"));
            }

            return string.Join("|", texts.Select(x => $"{property.CppTypeName}.{x}"));
        }

        public static string GetEnumUnderlyingType(PropertyDefinition property)
        {
            if(property.Size == sizeof(int))
            {
                return "int";
            }
            else if(property.Size == sizeof(Int64))
            {
                return "Int64";
            }
            else if (property.Size == sizeof(short))
            {
                return "short";
            }

            return "byte";
        }

        public override string GetFastInvokeDelegateParameterTypeName(PropertyDefinition property, ELocalUsageScenarioType localUsageScenarioType)
        {
            var type = GetEnumUnderlyingType(property);

            if(property.IsPassByReferenceInCpp)
            {
                return $"{type}*";
            }

            return type;
        }

        public override string GetBeforeFastInvokeText(PropertyDefinition property)
        {
            var type = GetEnumUnderlyingType(property);
            return $"{type} __valueOf{property.Name} = ({type}){property.Name};";
        }

        public override string GetPostFastInvokeText(PropertyDefinition property)
        {
            if(property.IsPassByReferenceInCpp)
            {
                return $"{property.Name} = InteropUtils.GetEnum<{GetCSharpTypeName(property, ELocalUsageScenarioType.GenericArgument)}>(new IntPtr(&__valueOf{property.Name}), 0, {property.Size});";
            }

            return string.Empty;
        }

        public override string GetFastInvokeParameterName(PropertyDefinition property)
        {
            if (property.IsPassByReferenceInCpp)
            {
                return $"&__valueOf{property.Name}";
            }

            return $"__valueOf{property.Name}";
        }

        public override string DecorateFastInvokeReturnValue(PropertyDefinition property, string returnValue)
        {
            return $"({GetCSharpTypeName(property, ELocalUsageScenarioType.GenericArgument)}){returnValue}";
        }
    }
}
