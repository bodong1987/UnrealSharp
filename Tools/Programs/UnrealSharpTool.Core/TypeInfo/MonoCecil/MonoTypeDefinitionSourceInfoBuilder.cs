using Mono.Cecil;
using UnrealSharpTool.Core.ErrorReports;

namespace UnrealSharpTool.Core.TypeInfo.MonoCecil;

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
    public static TypeDefinitionSourceInfo Build(MemberReference memberReference, IDebugInformation debugInformation)
    {
        return memberReference switch
        {
            Mono.Cecil.PropertyDefinition pd => BuildInternal(pd, debugInformation),
            FieldDefinition fd => BuildInternal(fd, debugInformation),
            MethodDefinition md => BuildInternal(md, debugInformation),
            TypeDefinition td => BuildInternal(td, debugInformation),
            _ => default
        };
    }

    /// <summary>
    /// Builds the specified property definition.
    /// </summary>
    /// <param name="propertyDefinition">The property definition.</param>
    /// <param name="debugInformation">The debug information.</param>
    /// <returns>TypeDefinitionSourceInfo.</returns>
    private static TypeDefinitionSourceInfo BuildInternal(Mono.Cecil.PropertyDefinition propertyDefinition, IDebugInformation debugInformation)
    {
        var identifier = new SymbolIdentifier
        {
            Name = propertyDefinition.Name,
            OwnerName = propertyDefinition.DeclaringType.Name,
            Type = SymbolIdentifierType.Property
        };

        return new TypeDefinitionSourceInfo(identifier, debugInformation);
    }

    /// <summary>
    /// Builds the specified field definition.
    /// </summary>
    /// <param name="fieldDefinition">The field definition.</param>
    /// <param name="debugInformation">The debug information.</param>
    /// <returns>TypeDefinitionSourceInfo.</returns>
    private static TypeDefinitionSourceInfo BuildInternal(FieldDefinition fieldDefinition, IDebugInformation debugInformation)
    {
        var identifier = new SymbolIdentifier
        {
            Name = fieldDefinition.Name,
            OwnerName = fieldDefinition.DeclaringType.Name,
            Type = SymbolIdentifierType.Field
        };

        return new TypeDefinitionSourceInfo(identifier, debugInformation);
    }

    /// <summary>
    /// Builds the specified method
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="debugInformation">The debug information.</param>
    /// <returns>TypeDefinitionSourceInfo.</returns>
    private static TypeDefinitionSourceInfo BuildInternal(MethodDefinition method, IDebugInformation debugInformation)
    {
        var identifier = new SymbolIdentifier
        {
            Name = method.Name,
            OwnerName = method.DeclaringType.Name,
            Type = SymbolIdentifierType.Method
        };

        return new TypeDefinitionSourceInfo(identifier, debugInformation);
    }

    /// <summary>
    /// Builds the specified definition.
    /// </summary>
    /// <param name="definition">The definition.</param>
    /// <param name="debugInformation">The debug information.</param>
    /// <returns>TypeDefinitionSourceInfo.</returns>
    /// <exception cref="System.Exception">Unsupported type??? {definition}</exception>
    private static TypeDefinitionSourceInfo BuildInternal(TypeDefinition definition, IDebugInformation debugInformation)
    {
        var identifier = new SymbolIdentifier
        {
            Name = definition.Name
        };

        var index = identifier.Name.IndexOf('`');
        if (index != -1)
        {
            identifier.Name = identifier.Name[..index];
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