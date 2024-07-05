namespace UnrealSharp.Utils.UnrealEngine;

/// <summary>
/// Class BindingDefinitionAttribute.
/// A statement used to mark that a class should not participate in specific logic and is only used to bind code.
/// Implements the <see cref="System.Attribute" />
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct| AttributeTargets.Enum)]
public class BindingDefinitionAttribute : Attribute;