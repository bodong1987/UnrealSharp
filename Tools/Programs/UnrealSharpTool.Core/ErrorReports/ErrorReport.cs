using System.Text;
using UnrealSharp.Utils.Misc;
using UnrealSharpTool.Core.TypeInfo;

namespace UnrealSharpTool.Core.ErrorReports
{
    /// <summary>
    /// Struct TypeDefinitionSourceInfo
    /// </summary>
    public struct TypeDefinitionSourceInfo
    {
        /// <summary>
        /// The signature
        /// </summary>
        public string Signature => Identifier.ToString();
          
        /// <summary>
        /// The identifier
        /// </summary>
        public readonly SymbolIdentifier Identifier;

        /// <summary>
        /// The source infos
        /// </summary>
        public readonly IEnumerable<SymbolSourceInfo>? SourceInfos;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(Signature);
            builder.AppendLine();

            if(SourceInfos != null)
            {
                foreach (var i in SourceInfos)
                {
                    return $"  >{i.FilePath}({i.Line}))";
                }   
            }
            
            return builder.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDefinitionSourceInfo" /> struct.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="debugInformation">The debug information.</param>
        public TypeDefinitionSourceInfo(SymbolIdentifier identifier, IDebugInformation debugInformation)
        {
            Identifier = identifier;

            SourceInfos = debugInformation.ResolveSourceInfo(identifier);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDefinitionSourceInfo"/> struct.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="sourceInfo">The source information.</param>
        public TypeDefinitionSourceInfo(SymbolIdentifier identifier, SymbolSourceInfo sourceInfo)
        {
            Identifier = identifier;
            SourceInfos = [sourceInfo];
        }
    }

    /// <summary>
    /// Class UnrealSharpDefinitionException.
    /// Implements the <see cref="System.Exception" />
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class UnrealSharpDefinitionException : Exception
    {
        /// <summary>
        /// The source information
        /// </summary>
        public readonly TypeDefinitionSourceInfo SourceInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnrealSharpDefinitionException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public UnrealSharpDefinitionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnrealSharpDefinitionException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sourceInfo">The source information.</param>
        public UnrealSharpDefinitionException(string message, TypeDefinitionSourceInfo sourceInfo) : this(message)
        {
            SourceInfo = sourceInfo;
        }
    }

    /// <summary>
    /// Class ErrorReport.
    /// </summary>
    public static class ErrorReport
    {
        /// <summary>
        /// Reports the warning.
        /// </summary>
        /// <param name="warningCode">The warning code.</param>
        /// <param name="message">The message.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="debugInformation">The debug information.</param>
        public static void ReportWarning(WarningCode warningCode, string message, SymbolIdentifier identifier, IDebugInformation debugInformation)
        {
            ReportWarning(warningCode, message, new TypeDefinitionSourceInfo(identifier, debugInformation));
        }

        /// <summary>
        /// Reports the warning.
        /// </summary>
        /// <param name="warningCode">The warning code.</param>
        /// <param name="message">The message.</param>
        /// <param name="sourceInfo">The source information.</param>
        public static void ReportWarning(WarningCode warningCode, string message, TypeDefinitionSourceInfo sourceInfo)
        {
            string text = BuildReportString("warning", warningCode.ToString(), message, sourceInfo);

            Logger.LogWarning(text);
        }

        /// <summary>
        /// Reports the error.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message.</param>
        /// <param name="filePath">The file path.</param>
        /// <exception cref="UnrealSharpTool.Core.ErrorReports.UnrealSharpDefinitionException"></exception>
        public static void ReportError(ErrorCode errorCode, string message, SymbolIdentifier identifier, IDebugInformation debugInformation)
        {
            TypeDefinitionSourceInfo si = new TypeDefinitionSourceInfo(identifier, debugInformation);

            ReportError(errorCode, message, si);
        }

        /// <summary>
        /// Reports the error.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message.</param>
        /// <param name="sourceInfo">The source information.</param>
        /// <exception cref="UnrealSharpDefinitionException">text, sourceInfo</exception>
        public static void ReportError(ErrorCode errorCode, string message, TypeDefinitionSourceInfo sourceInfo)
        {
            string text = BuildReportString("error", errorCode.ToString(), message, sourceInfo);
            Logger.LogError(text);

            throw new UnrealSharpDefinitionException(text, sourceInfo);
        }

        /// <summary>
        /// Builds the report string.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="code">The code.</param>
        /// <param name="message">The message.</param>
        /// <param name="sourceInfo">The source information.</param>
        /// <returns>System.String.</returns>
        public static string BuildReportString(string category, string code, string message, TypeDefinitionSourceInfo sourceInfo)
        {
            string FileStartTag = System.Diagnostics.Debugger.IsAttached ? " >" : "";

            StringBuilder builder = new StringBuilder();

            if (sourceInfo.SourceInfos != null && sourceInfo.SourceInfos.Count() > 0)
            {
                var baseSourceInfo = sourceInfo.SourceInfos.First();

                builder.AppendLine($"{FileStartTag}{baseSourceInfo.FilePath}({baseSourceInfo.Line}): {category} {code}: {message}");
            }
            else
            {
                builder.AppendLine($"{category} {code}: {message}");
            }
            
            if(sourceInfo.SourceInfos != null && sourceInfo.SourceInfos.Count() > 1)
            {
                builder.AppendLine($"{FileStartTag}Other possible locations:");

                foreach (var i in sourceInfo.SourceInfos.Skip(1))
                {
                    builder.AppendLine($"{FileStartTag}{i.FilePath}({i.Line})");
                }   
            }

            return builder.ToString();
        }
    }
}
