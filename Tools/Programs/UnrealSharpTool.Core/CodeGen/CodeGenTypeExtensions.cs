using System.Text;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen
{
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

            if (rType.IsByteEnum)
            {
                // force convert some byte property to enum
                return rType.ByteEnumName;
            }

            return rType.GetCSharpTypeName(context, ELocalUsageScenarioType.Method|ELocalUsageScenarioType.ReturnValue);
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
            StringBuilder stringBuilder = new StringBuilder();

            var outTag = function.IsEvent || context.SchemaType == EBindingSchemaType.NativeBinding ? "ref " : "out ";

            bool StopSearchDefaultValue = !withDefaultValue;
            
            foreach (var p in function.Properties.FindAll(x => !x.IsReturnParam).Reverse<PropertyDefinition>())
            {
                string type = p.GetCSharpTypeName(context, ELocalUsageScenarioType.Parameter|ELocalUsageScenarioType.Method);
                string name = p.SafeName;

                string referenceFlag = p.IsOutParam && !p.IsConstParam ? outTag : (p.IsReference ? "ref " : "");
                string defaultValueTag = "";

                if(!StopSearchDefaultValue)
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
                            StopSearchDefaultValue = true;
                        }
                    }
                    else
                    {
                        StopSearchDefaultValue = true;
                    }
                }

                stringBuilder.Insert(0, $", {referenceFlag}{type} {name}{defaultValueTag}");
            }

            string result = stringBuilder.ToString();

            if(result.StartsWith(", "))
            {
                return result.Substring(2);
            }

            return result;
        }

        /// <summary>
        /// Gets the export invoke parameters.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.String.</returns>
        public static string GetExportInvokeParameters(this FunctionTypeDefinition function, BindingContext context)
        {
            StringBuilder stringBuilder = new StringBuilder();

            var outTag = function.IsEvent || context.SchemaType == EBindingSchemaType.NativeBinding ? "ref " : "out ";

            foreach (var p in function.Properties.FindAll(x => !x.IsReturnParam))
            {
                string type = p.GetCSharpTypeName(context, ELocalUsageScenarioType.Parameter | ELocalUsageScenarioType.Method);
                string name = p.SafeName;

                string semicolon = p == function.Properties.First() ? "" : ", ";

                string referenceFlag = p.IsOutParam && !p.IsConstParam ? outTag : (p.IsReference ? "ref " : "");

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
}
