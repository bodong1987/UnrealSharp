using UnrealSharp.Utils.Extensions;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen.Processors
{
    /// <summary>
    /// Class BooleanPropertyProcessor.
    /// Implements the <see cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
    /// </summary>
    /// <seealso cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
    [PropertyProcessPolicy]
    class BooleanPropertyProcessor : PropertyProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanPropertyProcessor"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public BooleanPropertyProcessor(BindingContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets the matched type class.
        /// </summary>
        /// <returns>System.String[].</returns>
        public override string[] GetMatchedTypeClass()
        {
            return ["BoolProperty"];
        }

        /// <summary>
        /// Gets the name of the c sharp type.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>System.String.</returns>
        public override string GetCSharpTypeName(PropertyDefinition property, ELocalUsageScenarioType usage)
        {
            return "bool";
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
            if(property.IsNativeBoolean)
            {
                return $"{variable}{(variable.IsNotNullOrEmpty()?" = ":"")}InteropUtils.GetBoolean({address}, {offset})";
            }
            else
            {
                if (Context.UnrealVersion < UE_5_3)
                {
                    return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}InteropUtils.GetBoolean({address}, {offset}, {property.Parent!.CppName}MetaData.{property.Name}_Property)";
                }
                else
                {
                    return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}InteropUtils.GetBoolean({address}, {offset}, {property.FieldMask})";
                }                    
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
            if (property.IsNativeBoolean)
            {
                return $"InteropUtils.SetBoolean({address}, {offset}, {variable})";
            }
            else
            {
                if(Context.UnrealVersion < UE_5_3)
                {
                    // set by property
                    return $"InteropUtils.SetBoolean({address}, {offset}, {variable}, {property.Parent!.CppName}MetaData.{property.Name}_Property)";
                }
                else
                {
                    return $"InteropUtils.SetBoolean({address}, {offset}, {property.FieldMask}, {variable})";
                }                
            }
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
            if (usage.HasFlag(ELocalUsageScenarioType.Field))
            {
                // false does not need export
                if(value.iEquals("false"))
                {
                    return null;
                }
            }

            if (value.iEquals("true") || value.iEquals("1"))
            {
                return "true";
            }

            return "false";
        }

        Version UE_5_3 = new Version(5, 3);
        /// <summary>
        /// Shoulds the export property in meta fields.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="propertyDefinition">The property definition.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool ShouldExportPropertyInMetaFields(StructTypeDefinition typeDefinition, PropertyDefinition propertyDefinition)
        {
            if(Context.IsNativeBinding && 
                !propertyDefinition.IsNativeBoolean &&
                Context.UnrealVersion < UE_5_3 // 
                )
            {
                // we need use FBoolProperty to set and get boolean value ...
                return true;
            }

            return base.ShouldExportPropertyInMetaFields(typeDefinition, propertyDefinition);
        }
    }
}
