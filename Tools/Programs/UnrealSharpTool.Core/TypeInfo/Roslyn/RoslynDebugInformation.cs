using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnrealSharp.Utils.Extensions.IO;

namespace UnrealSharpTool.Core.TypeInfo.Roslyn;

internal class RoslynSymbolInfo : SymbolIdentifier
{
    public MemberDeclarationSyntax? MemberDeclaration;
    public readonly List<SymbolSourceInfo> SourceInfos = [];
}

internal class RoslynDebugInformation : IDebugInformation
{
    // ReSharper disable once CollectionNeverQueried.Global
    public readonly List<CSharpSourceFile> SourceFiles = [];
    public readonly Dictionary<string, RoslynSymbolInfo> Symbols = new();
                        
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
            var identifier = new RoslynSymbolInfo();
                
            switch (member)
            {
                case ClassDeclarationSyntax:
                    identifier.Type = SymbolIdentifierType.Class;
                    break;
                case StructDeclarationSyntax:
                    identifier.Type = SymbolIdentifierType.Struct;
                    break;
                case EnumDeclarationSyntax:
                    identifier.Type = SymbolIdentifierType.Enum;
                    break;
                case InterfaceDeclarationSyntax:
                    identifier.Type = SymbolIdentifierType.Interface;
                    break;
                case PropertyDeclarationSyntax:
                    identifier.Type = SymbolIdentifierType.Property;
                    identifier.OwnerName = (member.Parent as MemberDeclarationSyntax)!.GetIdentifierName();
                    break;
                case FieldDeclarationSyntax:
                    identifier.Type = SymbolIdentifierType.Field;
                    identifier.OwnerName = (member.Parent as MemberDeclarationSyntax)!.GetIdentifierName();
                    break;
                case MethodDeclarationSyntax:
                    identifier.Type = SymbolIdentifierType.Method;
                    identifier.OwnerName = (member.Parent as MemberDeclarationSyntax)!.GetIdentifierName();
                    break;
            }

            if(identifier.Type != SymbolIdentifierType.None)
            {
                identifier.MemberDeclaration = member;
                identifier.Name = member.GetIdentifierName();

                var key = identifier.ToString();

                if(!Symbols.TryGetValue(key, out var value))
                {
                    value = identifier;
                    Symbols.Add(key, value);
                }

                value.SourceInfos.Add(new SymbolSourceInfo
                {
                    FilePath = sourceFile.FilePath,
                    Line = member.GetIdentifierLineNumber()
                });
            }
        }
    }


    public IEnumerable<SymbolSourceInfo>? ResolveSourceInfo(SymbolIdentifier symbol)
    {
        return Symbols.TryGetValue(symbol.ToString(), out var value) ? value.SourceInfos : default(IEnumerable<SymbolSourceInfo>?);
    }
}