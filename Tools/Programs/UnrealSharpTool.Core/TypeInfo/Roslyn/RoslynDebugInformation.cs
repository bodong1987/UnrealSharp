using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using UnrealSharp.Utils.Extensions.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.CodeDom.Compiler;

namespace UnrealSharpTool.Core.TypeInfo.Roslyn
{
    internal class RoslynSymbolInfo : SymbolIdentifier
    {
        public MemberDeclarationSyntax? MemberDeclaration;
        public readonly List<SymbolSourceInfo> SourceInfos = new List<SymbolSourceInfo>();
    }

    internal class RoslynDebugInformation : IDebugInformation
    {
        public readonly List<CSharpSourceFile> SourceFiles = new List<CSharpSourceFile>();
        public readonly Dictionary<string, RoslynSymbolInfo> Symbols = new Dictionary<string, RoslynSymbolInfo>();
                        
        public RoslynDebugInformation(IEnumerable<string> sourceFiles)
        {
            foreach(var file in sourceFiles)
            {
                if(file.IsFileExists())
                {
                    var sourceFile = new CSharpSourceFile(file, "");
                    SourceFiles.Add(sourceFile);

                    BuildSymbolDictionary(sourceFile);
                }
            }
        }

        private void BuildSymbolDictionary(CSharpSourceFile sourceFile)
        {
            foreach(var member in sourceFile.GetDeclarationMembers())
            {
                RoslynSymbolInfo identifier = new RoslynSymbolInfo();
                
                if (member is ClassDeclarationSyntax)
                {
                    identifier.Type = SymbolIdentifierType.Class;
                }
                else if (member is StructDeclarationSyntax)
                {
                    identifier.Type = SymbolIdentifierType.Struct;
                }
                else if (member is EnumDeclarationSyntax)
                {
                    identifier.Type = SymbolIdentifierType.Enum;
                }
                else if (member is InterfaceDeclarationSyntax)
                {
                    identifier.Type = SymbolIdentifierType.Interface;
                }
                else if (member is PropertyDeclarationSyntax)
                {
                    identifier.Type = SymbolIdentifierType.Property;
                    identifier.OwnerName = (member.Parent as MemberDeclarationSyntax)!.GetIdentifierName();
                }
                else if (member is FieldDeclarationSyntax)
                {
                    identifier.Type = SymbolIdentifierType.Field;
                    identifier.OwnerName = (member.Parent as MemberDeclarationSyntax)!.GetIdentifierName();
                }
                else if (member is MethodDeclarationSyntax)
                {
                    identifier.Type = SymbolIdentifierType.Method;
                    identifier.OwnerName = (member.Parent as MemberDeclarationSyntax)!.GetIdentifierName();
                }

                if(identifier.Type != SymbolIdentifierType.None)
                {
                    identifier.MemberDeclaration = member;
                    identifier.Name = member.GetIdentifierName();

                    string key = identifier.ToString();

                    if(!Symbols.TryGetValue(key, out var value))
                    {
                        value = identifier;
                        Symbols.Add(key, value);
                    }

                    value.SourceInfos.Add(new SymbolSourceInfo()
                    {
                        FilePath = sourceFile.FilePath,
                        Line = member.GetIdentifierLineNumber()
                    });
                }
            }
        }


        public IEnumerable<SymbolSourceInfo>? ResolveSourceInfo(SymbolIdentifier symbol)
        {
            if(Symbols.TryGetValue(symbol.ToString(), out var value))
            {
                return value.SourceInfos;
            }

            return default;
        }
    }
}
