using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen.Processors
{
    /// <summary>
    /// Class ArrayPropertyProcessor.
    /// Implements the <see cref="UnrealSharpTool.Core.CodeGen.Processors.CollectionPropertyProcessor" />
    /// </summary>
    /// <seealso cref="UnrealSharpTool.Core.CodeGen.Processors.CollectionPropertyProcessor" />
    [PropertyProcessPolicy]
    class ArrayPropertyProcessor : CollectionPropertyProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayPropertyProcessor"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ArrayPropertyProcessor(BindingContext context) : base(context)
        {
            PropertyCacheName = "TArrayPropertyCache";
        }

        /// <summary>
        /// Gets the matched type class.
        /// </summary>
        /// <returns>System.String[].</returns>
        public override string[] GetMatchedTypeClass()
        {
            return
            [
                "ArrayProperty"
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
            Logger.Ensure<Exception>(property.InnerProperties.Count == 1);
            
            if(usage.HasFlag(ELocalUsageScenarioType.Class))
            {
                if(usage.HasFlag(ELocalUsageScenarioType.Property))
                {
                    return $"TArray<{property.GetInnerPropertyCSharpTypeName(Context, 0)}>?";
                }
            }
            else if(usage.HasFlag(ELocalUsageScenarioType.StructView))
            {
                return $"TArray<{property.GetInnerPropertyCSharpTypeName(Context, 0)}>";
            }
            else if(usage.HasFlag(ELocalUsageScenarioType.Method))
            {
                return $"IList<{property.GetInnerPropertyCSharpTypeName(Context, 0)}>?";
            }

            return $"List<{property.GetInnerPropertyCSharpTypeName(Context, 0)}>?";
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
            if (property.IsClassProperty)
            {
                return base.GetInteropGetValueText(property, variable, address, offset, usage);
            }

            PropertyProcessor? innerPropcessor = Context.GetProcessor(property.InnerProperties[0]);
            Logger.EnsureNotNull(innerPropcessor, "Failed find processor for array element.");

            string ConstantsName = property.IsFunctionProperty ? $"{property.Parent!.Name}MetaData" : $"{property.Parent!.CppName}MetaData";

            if(usage.HasFlag(ELocalUsageScenarioType.StructView))
            {
                return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}new TArray<{property.GetInnerPropertyCSharpTypeName(Context, 0)}>({address}, {offset}, {ConstantsName}.{property.Name}_Property)";
            }
            else
            {
                return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}TArray<{property.GetInnerPropertyCSharpTypeName(Context, 0)}>.FromNative({address}, {offset}, {ConstantsName}.{property.Name}_Property)";
            }
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

            PropertyProcessor? innerPropcessor = Context.GetProcessor(property.InnerProperties[0]);
            Logger.EnsureNotNull(innerPropcessor, "Failed find processor for array element.");

            Logger.EnsureNotNull(property.Parent, $"{property}'s parent can't be null!");

            string ConstantsName = property.IsFunctionProperty ? $"{property.Parent!.Name}MetaData" : $"{property.Parent!.CppName}MetaData";

            return $"TArray<{property.GetInnerPropertyCSharpTypeName(Context, 0)}>.ToNative({address}, {offset}, {ConstantsName}.{property.Name}_Property, {variable})";
        }
    }
}
