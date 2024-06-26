using System.Diagnostics.CodeAnalysis;
using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;
using UnrealSharpTool.Core.Utils;

namespace UnrealSharpTool.Core.CodeGen
{
    #region Compose
    /// <summary>
    /// Class PropertyProcessPolicyAttribute.
    /// Implements the <see cref="ExportAttribute" />
    /// </summary>
    /// <seealso cref="ExportAttribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PropertyProcessPolicyAttribute : ExportAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyProcessPolicyAttribute"/> class.
        /// </summary>
        public PropertyProcessPolicyAttribute() : base("PropertyProcessors", typeof(PropertyProcessor))
        {

        }
    }

    /// <summary>
    /// Class PropertyProcessorImporter.
    /// Implements the <see cref="AbstractComposableTarget" />
    /// </summary>
    /// <seealso cref="AbstractComposableTarget" />
    class PropertyProcessorImporter : AbstractComposableTarget
    {
        /// <summary>
        /// Gets or sets the property processors.
        /// </summary>
        /// <value>The property processors.</value>
        [Import("PropertyProcessors", typeof(PropertyProcessor))]
        public List<PropertyProcessor> PropertyProcessors { get; set; } = new List<PropertyProcessor>();

        /// <summary>
        /// The cached processors
        /// </summary>
        Dictionary<string, PropertyProcessor> CachedProcessors = new Dictionary<string, PropertyProcessor>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyProcessorImporter"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        [RequiresDynamicCode("Calls UnrealSharpTool.Core.Utils.ExtensibilityFramework.ComposeParts<T>(T, Object, params Object[])")]
        public PropertyProcessorImporter(BindingContext context)
        {
            ExtensibilityFramework.ComposeParts(this, this, context);

            foreach(var processor in PropertyProcessors)
            {
                foreach(var typeClass in processor.GetMatchedTypeClass())
                {
                    CachedProcessors.Add(typeClass, processor);
                }
            }
        }

        /// <summary>
        /// Gets the processor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.Nullable&lt;PropertyProcessor&gt;.</returns>
        public PropertyProcessor? GetProcessor(PropertyDefinition type)
        {
            CachedProcessors.TryGetValue(type.TypeClass!, out var processor); 
            return processor;
        }
    }
    #endregion

    /// <summary>
    /// Class PropertyProcessor.
    /// Property export policy
    /// Implements the <see cref="AbstractImportable" />
    /// </summary>
    /// <seealso cref="AbstractImportable" />
    public abstract class PropertyProcessor : AbstractImportable
    {
        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        public BindingContext Context { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyProcessor"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public PropertyProcessor(BindingContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Gets the matched type class.
        /// </summary>
        /// <returns>System.String[].</returns>
        public abstract string[] GetMatchedTypeClass();

        /// <summary>
        /// Gets the name of the not null c sharp type.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>System.String.</returns>
        public static string GetNotNullCSharpTypeName(string name)
        {
            if (name.EndsWith("?"))
            {
                return name.Substring(0, name.Length - 1);
            }

            return name;
        }

        /// <summary>
        /// Gets the name of the c sharp type.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>System.String.</returns>
        public virtual string GetCSharpTypeName(PropertyDefinition property, ELocalUsageScenarioType usage = ELocalUsageScenarioType.Common)
        {
            return property.CppTypeName!;
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
        public virtual string GetInteropGetValueText(
            PropertyDefinition property, 
            string variable, 
            string address, 
            string offset,
            ELocalUsageScenarioType usage = ELocalUsageScenarioType.Common
            )
        {
            return $"{variable} = InteropUtils.GetValue({address}, {offset})";
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
        public virtual string GetInteropSetValueText(
            PropertyDefinition property, 
            string variable, 
            string address,
            string offset,
            ELocalUsageScenarioType usage = ELocalUsageScenarioType.Common
            )
        {
            return $"InteropUtils.SetValue({address}, {offset}, {variable})";
        }

        /// <summary>
        /// Shoulds the export property in meta fields.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="propertyDefinition">The property definition.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public virtual bool ShouldExportPropertyInMetaFields(StructTypeDefinition typeDefinition, PropertyDefinition propertyDefinition)
        {
            return false;
        }

        /// <summary>
        /// Befores the class property write.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="property">The property.</param>
        public virtual void BeforeClassPropertyWrite(CodeWriter writer, PropertyDefinition property)
        {
        }

        /// <summary>
        /// Handles the class property write.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="property">The property.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public virtual bool HandleClassPropertyWrite(CodeWriter writer, PropertyDefinition property)
        {
            return false;
        }

        /// <summary>
        /// Ends the class property write.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="property">The property.</param>
        public virtual void EndClassPropertyWrite(CodeWriter writer, PropertyDefinition property)
        {
        }

        /// <summary>
        /// Allows the write property setter.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public virtual bool AllowWritePropertySetter(PropertyDefinition property)
        {
            return true;
        }

        /// <summary>
        /// Decorates the default value text.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        public virtual string? DecorateDefaultValueText(PropertyDefinition property, string value, ELocalUsageScenarioType usage)
        {
            return null;
        }

        /// <summary>
        /// Gets the type of the CPP function declaration.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>System.String.</returns>
        public virtual string GetCppFunctionDeclarationType(PropertyDefinition property, ELocalUsageScenarioType usage)
        {
            string ReferenceTag = property.IsPassByReferenceInCpp || usage.HasFlag(ELocalUsageScenarioType.ReturnValue) ? "&" : "";
            string constTag = property.IsConstParam ? "const " : "";

            return $"{constTag}{property.CppTypeName}{ReferenceTag}";
        }

        /// <summary>
        /// Gets the name of the CPP function invoke parameter.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>System.String.</returns>
        public virtual string GetCppFunctionInvokeParameterName(PropertyDefinition property)
        {
            return property.Name!;
        }

        /// <summary>
        /// Gets the name of the fast invoke delegate parameter type.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="localUsageScenarioType">Type of the local usage scenario.</param>
        /// <returns>System.String.</returns>
        public virtual string GetFastInvokeDelegateParameterTypeName(PropertyDefinition property, ELocalUsageScenarioType localUsageScenarioType)
        {
            var CSharpType = GetCSharpTypeName(property, localUsageScenarioType);

            if(property.IsPassByReferenceInCpp)
            {
                return CSharpType + "*";
            }

            return CSharpType;
        }

        /// <summary>
        /// Gets the before fast invoke text.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>System.String.</returns>
        public virtual string GetBeforeFastInvokeText(PropertyDefinition property)
        {
            if(property.IsPassByReferenceInCpp)
            {
                return $"var __{property.Name} = {property.Name};";
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the post fast invoke text.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>System.String.</returns>
        public virtual string GetPostFastInvokeText(PropertyDefinition property)
        {
            if(property.IsPassByReferenceInCpp)
            {
                return $"{property.Name} = __{property.Name};";
            }

            return string.Empty; 
        }

        /// <summary>
        /// Gets the name of the fast invoke parameter.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>System.String.</returns>
        public virtual string GetFastInvokeParameterName(PropertyDefinition property)
        {
            if(property.IsPassByReferenceInCpp)
            {
                return $"&__{property.Name}";
            }

            return property.Name!;
        }

        /// <summary>
        /// Decorates the fast invoke return value.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="returnValue">The return value.</param>
        /// <returns>System.String.</returns>
        public virtual string DecorateFastInvokeReturnValue(PropertyDefinition property, string returnValue)
        {
            if(property.IsPassByReferenceInCpp)
            {
                return $"*{returnValue}";
            }

            return returnValue;
        }
    }
}
