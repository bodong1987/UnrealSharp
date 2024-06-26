using UnrealSharp.Utils.Extensions;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen.Processors
{
    /// <summary>
    /// Class ClassPropertyProcessor.
    /// Implements the <see cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
    /// </summary>
    /// <seealso cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
    [PropertyProcessPolicy]
    public class ClassPropertyProcessor : PropertyProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassPropertyProcessor"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ClassPropertyProcessor(BindingContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets the matched type class.
        /// </summary>
        /// <returns>System.String[].</returns>
        public override string[] GetMatchedTypeClass()
        {
            return ["ClassProperty", "ClassPtrProperty"];
        }

        /// <summary>
        /// Gets the name of the c sharp type.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>System.String.</returns>
        public override string GetCSharpTypeName(PropertyDefinition property, ELocalUsageScenarioType usage)
        {
            if (property.MetaClass.IsNotNullOrEmpty())
            {
                if (property.MetaClass == "UInterface")
                {
                    return $"TSubclassOf<IUObjectInterface>";
                }

                var typeDefinition = Context.Document.GetDefinition(property.MetaClass);

                if (typeDefinition != null && typeDefinition.IsInterface)
                {
                    return $"TSubclassOf<{"I" + property.MetaClass.Substring(1)}>";
                }

                return $"TSubclassOf<{property.MetaClass}>";
            }

            return "TSubclassOf<UObject>";
        }

        /// <summary>
        /// Gets the c sharp meta class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>System.String.</returns>
        private string GetCSharpMetaClass(PropertyDefinition property, ELocalUsageScenarioType usage)
        {
            if(property.MetaClass.IsNotNullOrEmpty())
            {
                if (property.MetaClass == "UInterface")
                {
                    return $"IUObjectInterface";
                }

                var typeDefinition = Context.Document.GetDefinition(property.MetaClass);

                if (typeDefinition != null && typeDefinition.IsInterface)
                {
                    return "I" + property.MetaClass.Substring(1);
                }

                return property.MetaClass;
            }

            return "UObject";
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
            string type = property.GetCSharpTypeName(Context, ELocalUsageScenarioType.Common | ELocalUsageScenarioType.GenericArgument);

            if (type.StartsWith("TSubclassOf"))
            {
                return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}InteropUtils.GetClass<{GetCSharpMetaClass(property, ELocalUsageScenarioType.GenericArgument)}>({address}, {offset})";
            }

            return $"{variable}{(variable.IsNotNullOrEmpty() ? " = " : "")}InteropUtils.GetClass({address}, {offset})";
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
            return $"InteropUtils.SetClass({address}, {offset}, {variable})";
        }

        /// <summary>
        /// Gets the type of the CPP function declaration.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>string.</returns>
        public override string GetCppFunctionDeclarationType(PropertyDefinition property, ELocalUsageScenarioType usage)
        {
            string constTag = property.IsConstParam & !usage.HasFlag(ELocalUsageScenarioType.ReturnValue) ? "const " : "";

            return $"{constTag}{property.CppTypeName}&";
            // return base.GetCppFunctionDeclarationType(property);
        }

        /// <summary>
        /// Gets the name of the fast invoke delegate parameter type.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="localUsageScenarioType">Type of the local usage scenario.</param>
        /// <returns>string.</returns>
        public override string GetFastInvokeDelegateParameterTypeName(PropertyDefinition property, ELocalUsageScenarioType localUsageScenarioType)
        {
            // struct force pass by pointer 
            if (localUsageScenarioType.HasFlag(ELocalUsageScenarioType.ReturnValue))
            {
                return base.GetFastInvokeDelegateParameterTypeName(property, localUsageScenarioType);
            }

            var CSharpType = GetCSharpTypeName(property, localUsageScenarioType);

            return $"{CSharpType}*";
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
