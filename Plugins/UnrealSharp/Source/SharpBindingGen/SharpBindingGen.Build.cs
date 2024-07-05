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

using UnrealBuildTool;

// ReSharper disable all
/// <summary>
/// This Module is mainly used to generate FTypeDefinitionDocument, a document that describes type information. 
/// This document has an implementation on both the C++ and C# sides, and the data of the two can be communicated with each other. 
/// This data is mainly used on the C# side to generate binding code (via C# tools), 
/// and is also used on the C++ side to generate C# type blueprint proxies(via UnrealSharpEditor Module).
/// </summary>
public class SharpBindingGen : ModuleRules
{
    public SharpBindingGen(ReadOnlyTargetRules Target) : base(Target)
    {
        PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;

        PublicDependencyModuleNames.AddRange(
            new string[]
            {
                "Core",
                "CoreUObject",
                "Engine",
                "DeveloperSettings", // for settings
                "Json", // for Json Support
                "UnrealSharp" // for UnrealSharp shared codes
                // ... add other public dependencies that you statically link with here ...
            }
        );
    }
}
// ReSharper restore all
