using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.Generation;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.CodeGen.Processors
{
    /// <summary>
    /// Class CollectionPropertyProcessor.
    /// Implements the <see cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
    /// </summary>
    /// <seealso cref="UnrealSharpTool.Core.CodeGen.PropertyProcessor" />
    abstract class CollectionPropertyProcessor : PropertyProcessor
    {
        /// <summary>
        /// The property cache name
        /// </summary>
        protected string PropertyCacheName = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionPropertyProcessor"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        protected CollectionPropertyProcessor(BindingContext context) : base(context)
        {
        }

        /// <summary>
        /// Shoulds the export property in meta fields.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="propertyDefinition">The property definition.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool ShouldExportPropertyInMetaFields(StructTypeDefinition typeDefinition, PropertyDefinition propertyDefinition)
        {
            return true;
        }

        /// <summary>
        /// Befores the class property write.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="property">The property.</param>
        public override void BeforeClassPropertyWrite(CodeWriter writer, PropertyDefinition property)
        {
            Logger.Ensure<Exception>(property.InnerProperties.Count == 1);

            writer.Write("[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            writer.Write($"private {PropertyCacheName}<{property.GetInnerPropertyCSharpTypeName(Context, 0)}> Z_{property.Name}Private;");
        }

        /// <summary>
        /// Handles the class property write.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="property">The property.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool HandleClassPropertyWrite(CodeWriter writer, PropertyDefinition property)
        {
            Logger.EnsureNotNull(property.Parent, $"{property}'s parent can't be null!");

            writer.Write("get");
            {
                using ScopedCodeWriter GetScope = new ScopedCodeWriter(writer);

                writer.Write($"return Z_{property.Name}Private.Get(GetNativePtr(), {property.Parent!.CppName}MetaData.{property.Name}_Offset, {property.Parent!.CppName}MetaData.{property.Name}_Property);");
            }

            if(AllowWritePropertySetter(property))
            {
                writer.Write("set");
                {
                    using ScopedCodeWriter SetScope = new ScopedCodeWriter(writer);

                    writer.Write($"Z_{property.Name}Private.Get(GetNativePtr(), {property.Parent!.CppName}MetaData.{property.Name}_Offset, {property.Parent!.CppName}MetaData.{property.Name}_Property)?.CopyFrom(value);");
                }
            }            

            return true;
        }
    }
}
