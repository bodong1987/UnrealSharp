namespace UnrealSharpTool.Core.TypeInfo;

/// <summary>
/// Interface ITypeValidator
/// </summary>
public interface ITypeValidator
{
    /// <summary>
    /// Is it a valid type definition?
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns><c>true</c> if [is type definition] [the specified name]; otherwise, <c>false</c>.</returns>
    bool IsTypeDefinition(string name);

    /// <summary>
    /// is a valid placeholder definition
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns><c>true</c> if [is placeholder type] [the specified name]; otherwise, <c>false</c>.</returns>
    bool IsPlaceholderType(string name);

    /// <summary>
    /// Gets the definitions.
    /// </summary>
    /// <returns>IEnumerable&lt;System.String&gt;.</returns>
    IEnumerable<string> GetDefinitions();

    /// <summary>
    /// Gets the placeholders.
    /// </summary>
    /// <returns>IEnumerable&lt;System.String&gt;.</returns>
    IEnumerable<string> GetPlaceholders();

    /// <summary>
    /// Merges the specified other validator.
    /// </summary>
    /// <param name="otherValidator">The other validator.</param>
    void Merge(ITypeValidator otherValidator);
}