/*
	MIT License

	Copyright (c) 2024 UnrealSharp

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all
	copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
	SOFTWARE.

	Project URL: https://github.com/bodong1987/UnrealSharp
*/
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UnrealSharp.UnrealEngine.InteropService;
using UnrealSharp.Utils.CommandLine;
using UnrealSharp.Utils.Extensions.IO;
using UnrealSharp.Utils.Misc;

namespace UnrealSharp.UnrealEngine.Main
{
    /// <summary>
    /// Class UnrealSharpEntry.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public static class UnrealSharpEntry
    {
        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        public static UnrealSharpStartOptions? Options { get; private set; }

        /// <summary>
        /// Gets the interop function information.
        /// </summary>
        /// <value>The interop function information.</value>
        public static FUnrealInteropFunctionsInfo InteropFunctionInfo { get; private set; }

        /// <summary>
        /// Gets the unreal version.
        /// </summary>
        /// <value>The unreal version.</value>
        public static string UnrealVersion => $"{InteropFunctionInfo.UnrealMajorVersion}.{InteropFunctionInfo.UnrealMinorVersion}.{InteropFunctionInfo.UnrealPatchVersion}";

        /// <summary>
        /// The log message pointer
        /// </summary>
        private static unsafe delegate* unmanaged[Cdecl]<int, string, void> LogMessagePointer;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="interopInfoPtr">The interop information PTR.</param>
        /// <param name="commandArgumentStringPtr">The command argument string PTR.</param>
        public static void Main(nint interopInfoPtr, nint commandArgumentStringPtr)
        {
            unsafe
            {
                InteropFunctionInfo = *(FUnrealInteropFunctionsInfo*)interopInfoPtr;
                Logger.Ensure<Exception>(
                    InteropFunctionInfo.SizeOfThis == sizeof(FUnrealInteropFunctionsInfo),
                    $"sizeof(FUnrealInteropFunctionsInfo) missmatch, in C# = {sizeof(FUnrealInteropFunctionsInfo)}, in C++ = {InteropFunctionInfo.SizeOfThis}"
                    );

                LogMessagePointer = (delegate* unmanaged[Cdecl]<int, string, void>)InteropFunctionInfo.LogMessageFunctionPointer;
            }

            InstallUnrealLogObserver();

            string args = Marshal.PtrToStringUni(commandArgumentStringPtr)!;

            Logger.Log("UnrealSharp Started. args={0}", args);

            var Result = Parser.Default.Parse<UnrealSharpStartOptions>(args);

            if (Result.Result == ParserResultType.NotParsed)
            {
                Logger.Log("Failed parse command line arguments.");
                return;
            }

            Options = Result.Value!;

            Logger.Log("Unreal Engine Version : {0}", UnrealVersion);

            MetaInteropUtils.DumpAllPossibleFastAccessInAssembly(typeof(UnrealSharpEntry).Assembly);

            unsafe
            {
                FUnrealSharpBuildInfo buildInfo = FUnrealSharpBuildInfo.Get();
                InteropFunctions.ValidateUnrealSharpBuildInfo(&buildInfo);
            }
        }

        /// <summary>
        /// Installs the unreal log observer.
        /// </summary>
        private static void InstallUnrealLogObserver()
        {
            UnrealLoggerObserver observer = new UnrealLoggerObserver();
            Logger.OnSystemLogEvent += (l, t, m) => observer.OnEvent(l, t, m);
        }

        /// <summary>
        /// Installs the temporary file log observer.
        /// </summary>
        private static void InstallTempFileLogObserver()
        {
#if DEBUG
            if (Options == null || !Options.UnrealSharpIntermediateDirectory.IsDirectoryExists())
            {
                return;
            }

            try
            {
                string logFile = Path.Combine(Options.UnrealSharpIntermediateDirectory!, "unrealsharp.log");
                LoggerFileObserver fileObserver = new LoggerFileObserver(logFile);
                Logger.OnSystemLogEvent += (l, t, m) => fileObserver.OnEvent(l, t, m);
            }
            catch
            {
                Logger.LogWarning("Failed create unrealsharp.log");
            }
#endif
        }

        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="message">The message.</param>
        public unsafe static void LogMessage(LoggerLevel level, string message)
        {
            // don't use Logger.Assert
            System.Diagnostics.Debug.Assert(LogMessagePointer != null);

            // send to C++
            LogMessagePointer((int)level, message);
        }
    }
}