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
#pragma once

namespace UnrealSharp
{
    class ICSharpMethodInvocation;
    class ICSharpRuntime;

    /// <summary>
    /// Provide some common methods
    /// </summary>
    class UNREALSHARP_API FUnrealSharpUtils
    {
    public:
        // default: UnrealSharp.UnrealEngine
        static const FString UnrealSharpEngineProjectName;

        // default: UnrealSharp.UnrealEngine.dll
        static const FString UnrealSharpEngineAssemblyName;

        // default: UnrealSharp.UnrealEngine
        static const FString UnrealSharpEngineNamespace;

        // default: UnrealSharp.GameScripts
        static const FString UnrealSharpGameScriptsProjectName;

        // default: UnrealSharp.GameScripts.dll
        static const FString UnrealSharpGameScriptsAssemblyName;

        // default UnrealSharp.GameScripts
        static const FString UnrealSharpGameScriptsNamespace;

        // default: UnrealSharp.GameScripts
        static const FString UnrealSharpGameContentProjectName;

        // default: UnrealSharp.GameContent.dll
        static const FString UnrealSharpGameContentAssemblyName;

        // default: UnrealSharp.GameContent
        static const FString UnrealSharpGameContentNamespace;

        /// <summary>
        /// Is Native Type(implement in C++)
        /// </summary>
        /// <param name="InField">The in field.</param>
        /// <returns>bool.</returns>
        static bool IsNativeField(const UField* InField);

        /// <summary>
        /// is C# types(implement in C#)
        /// </summary>
        /// <param name="InField">The in field.</param>
        /// <returns>bool.</returns>
        static bool IsCSharpField(const UField* InField);

        /// <summary>
        /// Is blueprint types(implement in blueprint)
        /// </summary>
        /// <param name="InField">The in field.</param>
        /// <returns>bool.</returns>
        static bool IsBlueprintField(const UField* InField);

        /// <summary>
        /// export to GameScripts field?
        /// </summary>
        /// <param name="InField">The in field.</param>
        /// <returns>bool.</returns>
        static bool IsExportToGameScriptsField(const UField* InField);

        /// <summary>
        /// Is reserve class types
        /// </summary>
        /// <param name="InClass">The in class.</param>
        /// <returns>bool.</returns>
        static bool IsSpecialClass(const UClass* InClass);

        /// <summary>
        /// Is Native class ?? implement in C++
        /// </summary>
        /// <param name="InClass">The in class.</param>
        /// <returns>bool.</returns>
        static bool IsNativeClass(const UClass* InClass);

        /// <summary>
        /// Is Blueprint class ? implement in blueprint
        /// </summary>
        /// <param name="InClass">The in class.</param>
        /// <returns>bool.</returns>
        static bool IsBlueprintClass(const UClass* InClass);

        /// <summary>
        /// is blueprint class and inherit from an C# generated class ?
        /// </summary>
        /// <param name="InClass">The in class.</param>
        /// <returns>bool.</returns>
        static bool IsCSharpInheritBlueprintClass(const UClass* InClass);

        /// <summary>
        /// Is C# Class, Implement in C#
        /// </summary>
        /// <param name="InClass">The in class.</param>
        /// <returns>bool.</returns>
        static bool IsCSharpClass(const UClass* InClass);

        /// <summary>
        /// Is Native struct ?? implement in C++
        /// </summary>
        /// <param name="InStruct">The in struct.</param>
        /// <returns>bool.</returns>
        static bool IsNativeStruct(const UScriptStruct* InStruct);

        /// <summary>
        /// Is Blueprint struct ? implement in blueprint
        /// </summary>
        /// <param name="InStruct">The in struct.</param>
        /// <returns>bool.</returns>
        static bool IsBlueprintStruct(const UScriptStruct* InStruct);

        /// <summary>
        /// Is C# Struct, Implement in C#
        /// </summary>
        /// <param name="InStruct">The in struct.</param>
        /// <returns>bool.</returns>
        static bool IsCSharpStruct(const UScriptStruct* InStruct);

        /// <summary>
        /// Is Native Enum ?? implement in C++
        /// </summary>
        /// <param name="InEnum">The in Enum.</param>
        /// <returns>bool.</returns>
        static bool IsNativeEnum(const UEnum* InEnum);

        /// <summary>
        /// Is Blueprint Enum ? implement in blueprint
        /// </summary>
        /// <param name="InEnum">The in Enum.</param>
        /// <returns>bool.</returns>
        static bool IsBlueprintEnum(const UEnum* InEnum);

        /// <summary>
        /// Is C# Enum, Implement in C#
        /// </summary>
        /// <param name="InEnum">The in Enum.</param>
        /// <returns>bool.</returns>
        static bool IsCSharpEnum(const UEnum* InEnum);

        /// <summary>
        /// Gets the name of the CPP type.
        /// </summary>
        /// <param name="InField">The in field.</param>
        /// <returns>FString.</returns>
        static FString GetCppTypeName(const UField* InField);

        /// <summary>
        /// Gets the name of the field module.
        /// </summary>
        /// <param name="InField">The in field.</param>
        /// <returns>FName.</returns>
        static FName GetFieldModuleName(const UField* InField);

        /// <summary>
        /// Gets the default name of the export project.
        /// UFunction* is not supported.
        /// </summary>
        /// <param name="InField">The in field.</param>
        /// <returns>FString.</returns>
        static const FString& GetDefaultExportProjectName(const UField* InField);

        /// <summary>
        /// Gets the default export namespace.
        /// UFunction* is not supported.
        /// </summary>
        /// <param name="InField">The in field.</param>
        /// <returns>FString.</returns>
        static const FString& GetDefaultExportNamespace(const UField* InField);

        /// <summary>
        /// Gets the name of the assembly.
        /// if it is Unreal native type, return UnrealSharpAssemblyName
        /// if it is C# type, return the real assembly name
        /// </summary>
        /// <param name="InField">The in field.</param>
        /// <returns>FString.</returns>
        static const FString& GetAssemblyName(const UField* InField);

        /// <summary>
        /// Gets the C# full path.
        /// </summary>
        /// <param name="InField">The in field.</param>
        /// <returns>FString.</returns>
        static FString GetCSharpFullPath(const UField* InField);

        /// <summary>
        /// Gets the name of the assembly.
        /// use underlying type of property
        /// </summary>
        /// <param name="InProperty">The in property.</param>
        /// <returns>FString.</returns>
        static const FString& GetAssemblyName(const FProperty* InProperty);

        /// <summary>
        /// Gets the full path.
        /// use underlying type of property
        /// </summary>
        /// <param name="InProperty">The in property.</param>
        /// <returns>FString.</returns>
        static FString GetCSharpFullPath(const FProperty* InProperty);

        /// <summary>
        /// Gets the property inner field.
        /// </summary>
        /// <param name="InProperty">The in property.</param>
        /// <returns>UField *.</returns>
        static UField* GetPropertyInnerField(const FProperty* InProperty);

        /// <summary>
        /// A simple estimate of the size of the type behind it, not necessarily 100% accurate
        /// </summary>
        /// <param name="InEnum">The in enum.</param>
        /// <returns>int.</returns>
        static int CalcEnumUnderlyingTypeSize(const UEnum* InEnum);

        /// <summary>
        /// Get property count
        /// </summary>
        /// <returns>int.</returns>
        static int GetPropertyCount(const UStruct* InStruct);

        /// <summary>
        /// Get property count witch filter
        /// </summary>
        /// <returns>int.</returns>
        static int GetPropertyCount(const UStruct* InStruct, TFunction<bool(const FProperty*)> InFilter);

        /// <summary>
        /// pickup property name from blueprint struct property name: {propertyName}_{index}_{guid}
        /// </summary>
        /// <param name="InProperty">The in property.</param>
        /// <returns>FName.</returns>
        static FName ExtraUserDefinedStructPropertyName(const FProperty* InProperty);

        /// <summary>
        /// Binds the c sharp method checked.
        /// </summary>
        /// <param name="InRuntime">The in runtime.</param>
        /// <param name="InAssemblyName">Name of the in assembly.</param>
        /// <param name="InNamespace">The in namespace.</param>
        /// <param name="InClassName">Name of the in class.</param>
        /// <param name="InBaseSignature">The in base signature.</param>
        /// <returns>TSharedPtr&lt;ObjectType, InMode&gt;.</returns>
        static TSharedPtr<ICSharpMethodInvocation> BindCSharpMethodChecked(
            ICSharpRuntime* InRuntime,
            const FString& InAssemblyName,
            const FString& InNamespace,
            const FString& InClassName,
            const FString& InBaseSignature
        );

        /// <summary>
        /// Binds the unreal engine c sharp method checked.
        /// default use UnrealSharp.UnrealEngine.dll namespace : UnrealSharp.UnrealEngine
        /// </summary>
        /// <param name="InRuntime">The in runtime.</param>
        /// <param name="InClassName">Name of the in class.</param>
        /// <param name="InBaseSignature">The in base signature.</param>
        /// <returns>TSharedPtr&lt;ObjectType, InMode&gt;.</returns>
        static TSharedPtr<ICSharpMethodInvocation> BindUnrealEngineCSharpMethodChecked(
            ICSharpRuntime* InRuntime, 
            const FString& InClassName, 
            const FString& InBaseSignature
            );

    private:
        static bool IsExportToGameScriptsNativeField(const UField* InNativeField);
    };
}