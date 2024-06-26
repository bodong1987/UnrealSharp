using UnrealSharpTool.Core.ErrorReports;

namespace UnrealSharpTool.Core.TypeInfo.MonoCecil
{
    /// <summary>
    /// Class MonoTypeDefinitionSourceInfoBuilder.
    /// </summary>
    internal static class MonoTypeDefinitionSourceInfoBuilder
    {
        /// <summary>
        /// Builds the specified member reference.
        /// </summary>
        /// <param name="memberReference">The member reference.</param>
        /// <param name="debugInformation">The debug information.</param>
        /// <returns>TypeDefinitionSourceInfo.</returns>
        public static TypeDefinitionSourceInfo Build(Mono.Cecil.MemberReference memberReference, IDebugInformation debugInformation)
        {
            if(memberReference is Mono.Cecil.PropertyDefinition pd)
            {
                return BuildInternal(pd, debugInformation);
            }
            else if(memberReference is Mono.Cecil.FieldDefinition fd)
            {
                return BuildInternal(fd, debugInformation);
            }
            else if(memberReference is Mono.Cecil.MethodDefinition md)
            {
                return BuildInternal(md, debugInformation);
            }
            else if(memberReference is Mono.Cecil.TypeDefinition td)
            {
                return BuildInternal(td, debugInformation);
            }

            return default;
        }

        /// <summary>
        /// Builds the specified property definition.
        /// </summary>
        /// <param name="propertyDefinition">The property definition.</param>
        /// <param name="debugInformation">The debug information.</param>
        /// <returns>TypeDefinitionSourceInfo.</returns>
        private static TypeDefinitionSourceInfo BuildInternal(Mono.Cecil.PropertyDefinition propertyDefinition, IDebugInformation debugInformation)
        {
            SymbolIdentifier identifier = new SymbolIdentifier();
            identifier.Name = propertyDefinition.Name;
            identifier.OwnerName = propertyDefinition.DeclaringType.Name;
            identifier.Type = SymbolIdentifierType.Property;

            return new TypeDefinitionSourceInfo(identifier, debugInformation);
        }

        /// <summary>
        /// Builds the specified field definition.
        /// </summary>
        /// <param name="fieldDefinition">The field definition.</param>
        /// <param name="debugInformation">The debug information.</param>
        /// <returns>TypeDefinitionSourceInfo.</returns>
        private static TypeDefinitionSourceInfo BuildInternal(Mono.Cecil.FieldDefinition fieldDefinition, IDebugInformation debugInformation)
        {
            SymbolIdentifier identifier = new SymbolIdentifier();
            identifier.Name = fieldDefinition.Name;
            identifier.OwnerName = fieldDefinition.DeclaringType.Name;
            identifier.Type = SymbolIdentifierType.Field;

            return new TypeDefinitionSourceInfo(identifier, debugInformation);
        }

        /// <summary>
        /// Builds the specified method definiton.
        /// </summary>
        /// <param name="methodDefiniton">The method definiton.</param>
        /// <param name="debugInformation">The debug information.</param>
        /// <returns>TypeDefinitionSourceInfo.</returns>
        private static TypeDefinitionSourceInfo BuildInternal(Mono.Cecil.MethodDefinition methodDefiniton, IDebugInformation debugInformation)
        {
            SymbolIdentifier identifier = new SymbolIdentifier();
            identifier.Name = methodDefiniton.Name;
            identifier.OwnerName = methodDefiniton.DeclaringType.Name;
            identifier.Type = SymbolIdentifierType.Method;

            return new TypeDefinitionSourceInfo(identifier, debugInformation);
        }

        /// <summary>
        /// Builds the specified definition.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="debugInformation">The debug information.</param>
        /// <returns>TypeDefinitionSourceInfo.</returns>
        /// <exception cref="System.Exception">Unsupported type??? {definition}</exception>
        private static TypeDefinitionSourceInfo BuildInternal(Mono.Cecil.TypeDefinition definition, IDebugInformation debugInformation)
        {
            SymbolIdentifier identifier = new SymbolIdentifier();
            identifier.Name = definition.Name;

            int index = identifier.Name.IndexOf('`');
            if (index != -1)
            {
                identifier.Name = identifier.Name.Substring(0, index);
            }

            if (definition.IsInterface)
            {
                identifier.Type = SymbolIdentifierType.Interface;
            }
            else if (definition.IsEnum)
            {
                identifier.Type = SymbolIdentifierType.Enum;
            }
            else if (definition.IsStructType())
            {
                identifier.Type = SymbolIdentifierType.Struct;
            }
            else if (definition.IsClass)
            {
                identifier.Type = SymbolIdentifierType.Class;
            }
            else
            {
                throw new Exception($"Unsupported type??? {definition}");
            }

            return new TypeDefinitionSourceInfo(identifier, debugInformation);
        }
    }
}
