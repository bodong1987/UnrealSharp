using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharpTool.Core.TypeInfo.Roslyn;

/// <summary>
/// Class MemberDeclarationSyntaxExtensions.
/// </summary>
internal static class MemberDeclarationSyntaxExtensions
{
    /// <summary>
    /// Determines whether [is c sharp implement type attribute] [the specified attribute data].
    /// </summary>
    /// <param name="attributeData">The attribute data.</param>
    /// <returns><c>true</c> if [is c sharp implement type attribute] [the specified attribute data]; otherwise, <c>false</c>.</returns>
    public static bool IsCSharpImplementTypeAttribute(this AttributeData attributeData)
    {
        // ReSharper disable once MergeIntoLogicalPattern
        return attributeData.AttributeClass != null && (
            attributeData.AttributeClass.Name == nameof(UCLASSAttribute) ||
            attributeData.AttributeClass.Name == nameof(USTRUCTAttribute) ||
            attributeData.AttributeClass.Name == nameof(UFUNCTIONAttribute) ||
            attributeData.AttributeClass.Name == nameof(UPROPERTYAttribute) ||
            attributeData.AttributeClass.Name == nameof(UEVENTAttribute) ||
            attributeData.AttributeClass.Name == nameof(UENUMAttribute) ||
            attributeData.AttributeClass.Name == "UCLASS" ||
            attributeData.AttributeClass.Name == "USTRUCT" ||
            attributeData.AttributeClass.Name == "UFUNCTION" ||
            attributeData.AttributeClass.Name == "UPROPERTY" ||
            attributeData.AttributeClass.Name == "UEVENT" ||
            attributeData.AttributeClass.Name == "UENUM"
        );
    }

    /// <summary>
    /// Is C# binding definition
    /// </summary>
    /// <param name="memberDeclarationSyntax">The member declaration syntax.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <returns><c>true</c> if [is c sharp placeholder type] [the specified semantic model]; otherwise, <c>false</c>.</returns>
    public static bool IsCSharpBindingDefinitionType(this MemberDeclarationSyntax memberDeclarationSyntax, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetDeclaredSymbol(memberDeclarationSyntax);

        return symbol != null && symbol.GetAttributes().Any(a =>
            // ReSharper disable once MergeIntoLogicalPattern
            a.AttributeClass != null && (a.AttributeClass.Name == nameof(BindingDefinitionAttribute) ||
                                         a.AttributeClass.Name == "BindingDefinition"));
    }

    /// <summary>
    /// Determines whether [is c sharp implement type] [the specified semantic model].
    /// </summary>
    /// <param name="memberDeclarationSyntax">The member declaration syntax.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <returns><c>true</c> if [is c sharp implement type] [the specified semantic model]; otherwise, <c>false</c>.</returns>
    public static bool IsCSharpImplementType(this MemberDeclarationSyntax memberDeclarationSyntax, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetDeclaredSymbol(memberDeclarationSyntax);

        return symbol != null && symbol.GetAttributes().Any(a => a.IsCSharpImplementTypeAttribute());
    }

    /// <summary>
    /// Determines whether [is c sharp native binding type] [the specified semantic model].
    /// </summary>
    /// <param name="memberDeclarationSyntax">The member declaration syntax.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <returns><c>true</c> if [is c sharp native binding type] [the specified semantic model]; otherwise, <c>false</c>.</returns>
    public static bool IsCSharpNativeBindingType(this MemberDeclarationSyntax memberDeclarationSyntax, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetDeclaredSymbol(memberDeclarationSyntax);

        // ReSharper disable once MergeIntoLogicalPattern
        return symbol != null && symbol.GetAttributes().Any(a =>
            a.AttributeClass != null && (a.AttributeClass.Name == nameof(NativeBindingAttribute) ||
                                         a.AttributeClass.Name == "NativeBinding"));
    }

    /// <summary>
    /// Determines whether [is c sharp blueprint binding type] [the specified semantic model].
    /// </summary>
    /// <param name="memberDeclarationSyntax">The member declaration syntax.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <returns><c>true</c> if [is c sharp blueprint binding type] [the specified semantic model]; otherwise, <c>false</c>.</returns>
    public static bool IsCSharpBlueprintBindingType(this MemberDeclarationSyntax memberDeclarationSyntax, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetDeclaredSymbol(memberDeclarationSyntax);

        // ReSharper disable once MergeIntoLogicalPattern
        return symbol != null && symbol.GetAttributes().Any(a =>
            a.AttributeClass != null && (a.AttributeClass.Name == nameof(BlueprintBindingAttribute) ||
                                         a.AttributeClass.Name == "BlueprintBinding"));
    }

    /// <summary>
    /// Gets the name of the identifier.
    /// </summary>
    /// <param name="memberDeclarationSyntax">The member declaration syntax.</param>
    /// <returns>System.String.</returns>
    public static string GetIdentifierName(this MemberDeclarationSyntax memberDeclarationSyntax)
    {
        return memberDeclarationSyntax switch
        {
            BaseTypeDeclarationSyntax baseTypeDeclarationSyntax => baseTypeDeclarationSyntax.Identifier.Text,
            MethodDeclarationSyntax methodDeclarationSyntax => methodDeclarationSyntax.Identifier.Text,
            PropertyDeclarationSyntax propertyDeclarationSyntax => propertyDeclarationSyntax.Identifier.Text,
            FieldDeclarationSyntax fieldDeclarationSyntax => fieldDeclarationSyntax.Declaration.Variables[0]
                .Identifier.Text,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Gets the identifier line number.
    /// </summary>
    /// <param name="memberDeclarationSyntax">The member declaration syntax.</param>
    /// <returns>System.Int32.</returns>
    public static int GetIdentifierLineNumber(this MemberDeclarationSyntax memberDeclarationSyntax)
    {
        return memberDeclarationSyntax switch
        {
            BaseTypeDeclarationSyntax baseTypeDeclarationSyntax => baseTypeDeclarationSyntax.Identifier
                .GetLocation()
                .GetLineSpan()
                .StartLinePosition.Line,
            MethodDeclarationSyntax methodDeclarationSyntax => methodDeclarationSyntax.Identifier.GetLocation()
                .GetLineSpan()
                .StartLinePosition.Line,
            PropertyDeclarationSyntax propertyDeclarationSyntax => propertyDeclarationSyntax.Identifier
                .GetLocation()
                .GetLineSpan()
                .StartLinePosition.Line,
            FieldDeclarationSyntax fieldDeclarationSyntax => fieldDeclarationSyntax.Declaration.Variables[0]
                .Identifier.GetLocation()
                .GetLineSpan()
                .StartLinePosition.Line,
            _ => memberDeclarationSyntax.GetLocation().GetLineSpan().StartLinePosition.Line
        };
    }

    /// <summary>
    /// Gets the namespace.
    /// </summary>
    /// <param name="memberDeclarationSyntax">The member declaration syntax.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <returns>System.String.</returns>
    public static string GetNamespace(this MemberDeclarationSyntax memberDeclarationSyntax, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetDeclaredSymbol(memberDeclarationSyntax);

        return symbol?.ContainingNamespace.ToDisplayString() ?? string.Empty;
    }

    /// <summary>
    /// Gets the default export namespace.
    /// </summary>
    /// <param name="memberDeclarationSyntax">The member declaration syntax.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <returns>System.String.</returns>
    public static string GetDefaultExportNamespace(this MemberDeclarationSyntax memberDeclarationSyntax, SemanticModel semanticModel)
    {
        if (memberDeclarationSyntax.IsCSharpBindingDefinitionType(semanticModel))
        {
            return "UnrealSharp.UnrealEngine";
        }

        var defaultExportNamespace = GetNamespace(memberDeclarationSyntax, semanticModel);
            
        return defaultExportNamespace.EndsWith(".Defs") ? defaultExportNamespace.Left(defaultExportNamespace.Length - ".Defs".Length) : defaultExportNamespace;
    }

    /// <summary>
    /// Gets the type of the symbol identifier.
    /// </summary>
    /// <param name="member">The member.</param>
    /// <returns>SymbolIdentifierType.</returns>
    public static SymbolIdentifierType GetSymbolIdentifierType(this MemberDeclarationSyntax member)
    {
        return member switch
        {
            ClassDeclarationSyntax => SymbolIdentifierType.Class,
            StructDeclarationSyntax => SymbolIdentifierType.Struct,
            EnumDeclarationSyntax => SymbolIdentifierType.Enum,
            InterfaceDeclarationSyntax => SymbolIdentifierType.Interface,
            PropertyDeclarationSyntax => SymbolIdentifierType.Property,
            FieldDeclarationSyntax => SymbolIdentifierType.Field,
            MethodDeclarationSyntax => SymbolIdentifierType.Method,
            _ => SymbolIdentifierType.None
        };
    }

    /// <summary>
    /// Finds the member declaration syntax.
    /// </summary>
    /// <param name="diagnostic">The diagnostic.</param>
    /// <returns>System.Nullable&lt;MemberDeclarationSyntax&gt;.</returns>
    public static MemberDeclarationSyntax? FindMemberDeclarationSyntax(this Diagnostic diagnostic)
    {            
        // Get the source span of the error
        var errorSpan = diagnostic.Location.SourceSpan;

        // Find the syntax node that corresponds to the error
        var root = diagnostic.Location.SourceTree!.GetRoot();
        var errorNode = root.FindNode(errorSpan);

        // Find the enclosing MemberDeclarationSyntax
        var memberDeclaration = errorNode.FirstAncestorOrSelf<MemberDeclarationSyntax>();

        return memberDeclaration;
    }
}