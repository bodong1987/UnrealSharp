using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen.Processors
{
    /// <summary>
    /// Class StringPropertyProcessor.
    /// Implements the <see cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
    /// </summary>
    /// <seealso cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
    [PropertyProcessPolicy]
    class StringPropertyProcessor : PropertyProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringPropertyProcessor"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public StringPropertyProcessor(BindingContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets the matched type class.
        /// </summary>
        /// <returns>System.String[].</returns>
        public override string[] GetMatchedTypeClass()
        {
            return [
                "StrProperty"
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
            if (usage.HasFlag(ELocalUsageScenarioType.StructView))
            {
                return "FStringView";
            }

            if(usage.HasFlag(ELocalUsageScenarioType.GenericArgument))
            {
                return "string";
            }

            return "string?";
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
            return $"\"{value}\"";
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
        public override string GetInteropGetValueText(PropertyDefinition property, string variable, string address, string offset, ELocalUsageScenarioType usage = ELocalUsageScenarioType.Common)
        {
            if (usage.HasFlag(ELocalUsageScenarioType.StructView))
            {
                return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}InteropUtils.GetStringView({address}, {offset})";
            }

            return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}InteropUtils.GetString({address}, {offset})";
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
        public override string GetInteropSetValueText(PropertyDefinition property, string variable, string address, string offset, ELocalUsageScenarioType usage = ELocalUsageScenarioType.Common)
        {
            if (usage.HasFlag(ELocalUsageScenarioType.StructView))
            {
                return $"InteropUtils.SetStringView({address}, {offset}, {variable})";
            }

            return $"InteropUtils.SetString({address}, {offset}, {variable})";
        }

        /// <summary>
        /// Gets the type of the CPP function declaration.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>string.</returns>
        public override string GetCppFunctionDeclarationType(PropertyDefinition property, ELocalUsageScenarioType usage)
        {
            Logger.Assert(!property.IsReference);
            Logger.Assert(!property.IsOutParam);
            Logger.Assert(!property.IsReturnParam);

            // 
            return "const char*";
        }

        /// <summary>
        /// Gets the name of the CPP function invoke parameter.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>string.</returns>
        public override string GetCppFunctionInvokeParameterName(PropertyDefinition property)
        {
            Logger.Assert(!property.IsReference);
            Logger.Assert(!property.IsOutParam);
            Logger.Assert(!property.IsReturnParam);

            return $"UNREALSHARP_STRING_TO_TCHAR({property.Name})";
        }
    }
}
