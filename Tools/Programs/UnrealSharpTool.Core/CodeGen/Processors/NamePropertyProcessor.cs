using UnrealSharp.Utils.Extensions;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen.Processors
{
    /// <summary>
    /// Class NamePropertyProcessor.
    /// Implements the <see cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
    /// </summary>
    /// <seealso cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
    [PropertyProcessPolicy]
    class NamePropertyProcessor : PropertyProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamePropertyProcessor"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public NamePropertyProcessor(BindingContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets the matched type class.
        /// </summary>
        /// <returns>string[].</returns>
        public override string[] GetMatchedTypeClass()
        {
            return ["NameProperty"];
        }

        /// <summary>
        /// Gets the name of the c sharp type.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>string.</returns>
        public override string GetCSharpTypeName(PropertyDefinition property, ELocalUsageScenarioType usage)
        {
            return "FName";
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
        public override string GetInteropGetValueText(PropertyDefinition property, string variable, string address, string offset, ELocalUsageScenarioType usage = ELocalUsageScenarioType.Common)
        {
            return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}InteropUtils.GetName({address}, {offset})";
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
        public override string GetInteropSetValueText(PropertyDefinition property, string variable, string address, string offset, ELocalUsageScenarioType usage = ELocalUsageScenarioType.Common)
        {
            return $"InteropUtils.SetName({address}, {offset}, {variable})";
        }

        /// <summary>
        /// Gets the type of the CPP function declaration.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>string.</returns>
        public override string GetCppFunctionDeclarationType(PropertyDefinition property, ELocalUsageScenarioType usage)
        {
            string constTag = property.IsConstParam & !usage.HasFlag(ELocalUsageScenarioType.ReturnValue) ? "const " : "";

            return $"{constTag}{property.CppTypeName}&";
            //return base.GetCppFunctionDeclarationType(property);
        }

        /// <summary>
        /// Gets the name of the fast invoke delegate parameter type.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="localUsageScenarioType">Type of the local usage scenario.</param>
        /// <returns>string.</returns>
        public override string GetFastInvokeDelegateParameterTypeName(PropertyDefinition property, ELocalUsageScenarioType localUsageScenarioType)
        {
            if (localUsageScenarioType.HasFlag(ELocalUsageScenarioType.ReturnValue))
            {
                return base.GetFastInvokeDelegateParameterTypeName(property, localUsageScenarioType);
            }

            // struct force pass by pointer             
            return "FName*";
        }

        /// <summary>
        /// Gets the name of the fast invoke parameter.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>string.</returns>
        public override string GetFastInvokeParameterName(PropertyDefinition property)
        {
            if (property.IsPassByReferenceInCpp)
            {
                return $"&__{property.Name}";
            }

            return $"&{property.Name!}";
        }
    }
}
