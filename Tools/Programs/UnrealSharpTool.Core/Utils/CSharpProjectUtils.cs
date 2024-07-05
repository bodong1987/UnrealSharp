using System.Xml;
using UnrealSharp.Utils.Extensions.IO;
using UnrealSharp.Utils.Misc;

namespace UnrealSharpTool.Core.Utils;

public static class CSharpProjectUtils
{
    private static IEnumerable<string> GetDependProjectsInternal(string csprojFile, HashSet<string> processedFiles)
    {
        if(processedFiles.Contains(csprojFile.CanonicalPath().ToLower()))
        {
            return [];
        }

        processedFiles.Add(csprojFile.CanonicalPath().ToLower());

        var csprojDocument = new XmlDocument();
        csprojDocument.Load(csprojFile);

        var projectReferenceNodes = csprojDocument.SelectNodes("//ProjectReference");

        if (projectReferenceNodes == null)
        {
            return [];
        }

        var references = new List<string>();

        var directory = csprojFile.GetDirectoryPath();

        foreach (XmlNode node in projectReferenceNodes)
        {
            var includeAttribute = node.Attributes?["Include"];
            if (includeAttribute != null)
            {
                references.Add(includeAttribute.Value);

                var referencedFile = Path.Combine(directory, includeAttribute.Value).CanonicalPath();

                if(referencedFile.IsFileExists())
                {
                    references.AddRange(GetDependProjectsInternal(referencedFile, processedFiles));
                }                        
            }
        }

        return references.Distinct();
    }

    public static IEnumerable<string> GetDependProjects(string csprojFile)
    {
        var processed = new HashSet<string>();

        return GetDependProjectsInternal(csprojFile, processed);
    }

    public static IEnumerable<string> GetDependProjectNames(string csprojFile)
    {
        return GetDependProjects(csprojFile).Select(x => x.GetFileNameWithoutExtension());
    }

    public static IEnumerable<string> GetUnrealSharpProjectDepends(string unrealProjectDirectory, string unrealSharpProjectName)
    {
        var unrealsharpProjectPath = Path.Combine(unrealProjectDirectory, $"GameScripts/Game/{unrealSharpProjectName}/{unrealSharpProjectName}.csproj");

        if(!unrealsharpProjectPath.IsFileExists())
        {
            Logger.LogError("UnrealSharp project {0} is not exists.", unrealsharpProjectPath);
            return [];
        }

        return GetDependProjects(unrealsharpProjectPath);
    }

    public static IEnumerable<string> GetUnrealSharpProjectDependProjectNames(string unrealProjectDirectory, string unrealSharpProjectName)
    {
        return GetUnrealSharpProjectDepends(unrealProjectDirectory, unrealSharpProjectName).Select(x => x.GetFileNameWithoutExtension());
    }

    public static bool IsUnrealSharpGameProject(string unrealProjectDirectory, string projectName)
    {
        var path = Path.Combine(unrealProjectDirectory, $"GameScripts/Game/{projectName}");

        return path.IsDirectoryExists();
    }

    public static bool IsUnrealSharpGameProjectPlaceholdersExists(string unrealProjectDirectory, string projectName)
    {
        var path = Path.Combine(unrealProjectDirectory, $"GameScripts/Game/{projectName}/Bindings.Placeholders");

        return path.IsDirectoryExists();
    }
}