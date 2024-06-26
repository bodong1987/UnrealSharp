using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharpTool.Core.TypeInfo.Roslyn
{
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
            return attributeData.AttributeClass != null && (
                    attributeData.AttributeClass.Name == typeof(UCLASSAttribute).Name ||
                    attributeData.AttributeClass.Name == typeof(USTRUCTAttribute).Name ||
                    attributeData.AttributeClass.Name == typeof(UFUNCTIONAttribute).Name ||
                    attributeData.AttributeClass.Name == typeof(UPROPERTYAttribute).Name ||
                    attributeData.AttributeClass.Name == typeof(UEVENTAttribute).Name ||
                    attributeData.AttributeClass.Name == typeof(UENUMAttribute).Name ||
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

            return symbol != null && symbol.GetAttributes().Any(a => a.AttributeClass != null && (a.AttributeClass.Name == typeof(BindingDefinitionAttribute).Name || a.AttributeClass.Name == "BindingDefinition"));
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

            return symbol != null && symbol.GetAttributes().Any(a => a.AttributeClass != null && (a.AttributeClass.Name == typeof(NativeBindingAttribute).Name || a.AttributeClass.Name == "NativeBinding"));
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

            return symbol != null && symbol.GetAttributes().Any(a => a.AttributeClass != null && (a.AttributeClass.Name == typeof(BlueprintBindingAttribute).Name || a.AttributeClass.Name == "BlueprintBinding"));
        }

        /// <summary>
        /// Gets the name of the identifier.
        /// </summary>
        /// <param name="memberDeclarationSyntax">The member declaration syntax.</param>
        /// <returns>System.String.</returns>
        public static string GetIdentifierName(this MemberDeclarationSyntax memberDeclarationSyntax)
        {
            if (memberDeclarationSyntax is BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
            {
                return baseTypeDeclarationSyntax.Identifier.Text;
            }
            else if(memberDeclarationSyntax is MethodDeclarationSyntax methodDeclarationSyntax) 
            {
                return methodDeclarationSyntax.Identifier.Text;
            }
            else if(memberDeclarationSyntax is PropertyDeclarationSyntax propertyDeclarationSyntax)
            {
                return propertyDeclarationSyntax.Identifier.Text;
            }
            else if(memberDeclarationSyntax is FieldDeclarationSyntax fieldDeclarationSyntax)
            {
                return fieldDeclarationSyntax.Declaration.Variables[0].Identifier.Text;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the identifier line number.
        /// </summary>
        /// <param name="memberDeclarationSyntax">The member declaration syntax.</param>
        /// <returns>System.Int32.</returns>
        public static int GetIdentifierLineNumber(this MemberDeclarationSyntax memberDeclarationSyntax)
        {
            if (memberDeclarationSyntax is BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
            {
                return baseTypeDeclarationSyntax.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line;
            }
            else if (memberDeclarationSyntax is MethodDeclarationSyntax methodDeclarationSyntax)
            {
                return methodDeclarationSyntax.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line;
            }
            else if (memberDeclarationSyntax is PropertyDeclarationSyntax propertyDeclarationSyntax)
            {
                return propertyDeclarationSyntax.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line;
            }
            else if (memberDeclarationSyntax is FieldDeclarationSyntax fieldDeclarationSyntax)
            {
                return fieldDeclarationSyntax.Declaration.Variables[0].Identifier.GetLocation().GetLineSpan().StartLinePosition.Line;
            }

            return memberDeclarationSyntax.GetLocation().GetLineSpan().StartLinePosition.Line;
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

            var Namespace = GetNamespace(memberDeclarationSyntax, semanticModel);
            if (Namespace.EndsWith(".Defs"))
            {
                return Namespace.Left(Namespace.Length - ".Defs".Length);
            }

            return Namespace;
        }

        /// <summary>
        /// Gets the type of the symbol identifier.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>SymbolIdentifierType.</returns>
        public static SymbolIdentifierType GetSymbolIdentifierType(this MemberDeclarationSyntax member)
        {
            if (member is ClassDeclarationSyntax)
            {
                return SymbolIdentifierType.Class;
            }
            else if (member is StructDeclarationSyntax)
            {
                return SymbolIdentifierType.Struct;
            }
            else if (member is EnumDeclarationSyntax)
            {
                return SymbolIdentifierType.Enum;
            }
            else if (member is InterfaceDeclarationSyntax)
            {
                return SymbolIdentifierType.Interface;
            }
            else if (member is PropertyDeclarationSyntax)
            {
                return SymbolIdentifierType.Property;
            }
            else if (member is FieldDeclarationSyntax)
            {
                return SymbolIdentifierType.Field;
            }
            else if (member is MethodDeclarationSyntax)
            {
                return SymbolIdentifierType.Method;
            }

            return SymbolIdentifierType.None;
        }

        /// <summary>
        /// Finds the member declaration syntax.
        /// </summary>
        /// <param name="diagnostic">The diagnostic.</param>
        /// <returns>System.Nullable&lt;MemberDeclarationSyntax&gt;.</returns>
        public static MemberDeclarationSyntax? FindMemberDeclarationSyntax(this Diagnostic diagnostic)
        {            
            // Get the source span of the error
            TextSpan errorSpan = diagnostic.Location.SourceSpan;

            // Find the syntax node that corresponds to the error
            SyntaxNode root = diagnostic.Location.SourceTree!.GetRoot();
            SyntaxNode errorNode = root.FindNode(errorSpan);

            // Find the enclosing MemberDeclarationSyntax
            var memberDeclaration = errorNode.FirstAncestorOrSelf<MemberDeclarationSyntax>();

            return memberDeclaration;
        }
    }
}
