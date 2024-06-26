using UnrealSharp.Utils.Extensions;
using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen.Processors
{
    /// <summary>
    /// Class ObjectPropertyProcessor.
    /// Implements the <see cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
    /// </summary>
    /// <seealso cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
    [PropertyProcessPolicy]
    class ObjectPropertyProcessor : PropertyProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPropertyProcessor"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ObjectPropertyProcessor(BindingContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets the matched type class.
        /// </summary>
        /// <returns>System.String[].</returns>
        public override string[] GetMatchedTypeClass()
        {
            return [
                "ObjectProperty"
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
            if(property.CppTypeName!.StartsWith("TObjectPtr<"))
            {
                string MetaTypeName = property.CppTypeName.Substring("TObjectPtr<".Length, property.CppTypeName.Length - "TObjectPtr<".Length - 1).Trim();

                if (MetaTypeName == "UInterface")
                {
                    return usage.HasFlag(ELocalUsageScenarioType.GenericArgument) ? "IUObjectInterface" : "IUObjectInterface ?";
                }

                var typeDefinition = Context.Document.GetDefinition(MetaTypeName);

                if (typeDefinition != null && typeDefinition.IsInterface)
                {
                    return usage.HasFlag(ELocalUsageScenarioType.GenericArgument) ? $"I{MetaTypeName.Substring(1)}" : $"I{MetaTypeName.Substring(1)}?";
                }

                return usage.HasFlag(ELocalUsageScenarioType.GenericArgument) ? MetaTypeName : (MetaTypeName + "?");
            }
            else if(property.CppTypeName!.EndsWith('*'))
            {
                return usage.HasFlag(ELocalUsageScenarioType.GenericArgument) ?
                    property.CppTypeName.Substring(0, property.CppTypeName.Length - 1).Trim() :
                    (property.CppTypeName.Substring(0, property.CppTypeName.Length-1).Trim() + "?");
            }

            return usage.HasFlag(ELocalUsageScenarioType.GenericArgument) ? base.GetCSharpTypeName(property) : (base.GetCSharpTypeName(property) + "?");
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
            if(usage.HasFlag(ELocalUsageScenarioType.PropertyGetter))
            {
                return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}Z_{property.Name}Cache.GetObject({address}, {offset})";
            }

            return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}InteropUtils.GetObject<{property.GetCSharpTypeName(Context, usage|ELocalUsageScenarioType.GenericArgument)}>({address}, {offset})";
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
            return $"InteropUtils.SetObject({address}, {offset}, {variable})";
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
                if(value == "None" || value.IsNullOrEmpty())
                {
                    return null;
                }
            }

            if(value.iEquals("None"))
            {
                return "null";
            }

            return base.DecorateDefaultValueText(property, value, usage);
        }

        /// <summary>
        /// Befores the class property write.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="property">The property.</param>
        public override void BeforeClassPropertyWrite(CodeWriter writer, PropertyDefinition property)
        {
            base.BeforeClassPropertyWrite(writer, property);

            writer.Write("[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            writer.Write($"private TObjectPropertyCache<{property.GetCSharpTypeName(Context, ELocalUsageScenarioType.GenericArgument)}> Z_{property.Name}Cache;");
            writer.WriteNewLine();
        }            

        public override string GetFastInvokeDelegateParameterTypeName(PropertyDefinition property, ELocalUsageScenarioType localUsageScenarioType)
        {
            if(property.IsPassByReferenceInCpp)
            {
                return "IntPtr*";
            }

            return "IntPtr";
        }

        public override string GetBeforeFastInvokeText(PropertyDefinition property)
        {
            if(property.IsPassByReferenceInCpp)
            {
                return $"IntPtr __unrealObjectOf{property.Name} = {property.Name}.GetNativePtrSafe();";
            }

            return string.Empty;
        }

        public override string GetPostFastInvokeText(PropertyDefinition property)
        {
            if (property.IsPassByReferenceInCpp)
            {
                return $"{property.Name} = InteropUtils.GetObjectByNativePointer<{GetCSharpTypeName(property, ELocalUsageScenarioType.GenericArgument)}>(__unrealObjectOf{property.Name});";
            }

            return string.Empty;
        }

        public override string GetFastInvokeParameterName(PropertyDefinition property)
        {
            if(property.IsPassByReferenceInCpp)
            {
                return $"&__unrealObjectOf{property.Name}";
            }

            return $"{property.Name}.GetNativePtrSafe()";
        }

        public override string DecorateFastInvokeReturnValue(PropertyDefinition property, string returnValue)
        {
            return $"InteropUtils.GetObjectByNativePointer<{GetCSharpTypeName(property, ELocalUsageScenarioType.GenericArgument)}>({returnValue})";
        }
    }
}
