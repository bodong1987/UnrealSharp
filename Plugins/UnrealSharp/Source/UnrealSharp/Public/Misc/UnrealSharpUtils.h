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

        // Is Native Type(implement in C++)
        static bool IsNativeField(const UField* InField);

        // Is C# types(implement in C#)
        static bool IsCSharpField(const UField* InField);

        // Is blueprint types(implement in blueprint)
        static bool IsBlueprintField(const UField* InField);

        // Is export to GameScripts field?
        static bool IsExportToGameScriptsField(const UField* InField);

        // Is reserve class types
        // these types can't be exported to C#
        static bool IsSpecialClass(const UClass* InClass);

        // Is Native class ?? implement in C++
        static bool IsNativeClass(const UClass* InClass);

        // Is Blueprint class ? implement in blueprint
        static bool IsBlueprintClass(const UClass* InClass);

        // is blueprint class and inherit from an C# generated class ?
        static bool IsCSharpInheritBlueprintClass(const UClass* InClass);

        // Is C# Class, Implement in C#
        static bool IsCSharpClass(const UClass* InClass);

        // Is Native struct ?? implement in C++
        static bool IsNativeStruct(const UScriptStruct* InStruct);

        // Is Blueprint struct ? implement in blueprint
        static bool IsBlueprintStruct(const UScriptStruct* InStruct);

        // Is C# Struct, Implement in C#
        static bool IsCSharpStruct(const UScriptStruct* InStruct);

        // Is Native Enum ?? implement in C++
        static bool IsNativeEnum(const UEnum* InEnum);

        // Is Blueprint Enum ? implement in blueprint
        static bool IsBlueprintEnum(const UEnum* InEnum);

        // Is C# Enum, Implement in C#
        static bool IsCSharpEnum(const UEnum* InEnum);

        // Gets the name of the CPP type.
        // add C++ class prefix string for script name
        // Actor -> AActor
        // Object -> UObject
        static FString GetCppTypeName(const UField* InField);

        // Gets the name of the field module.
        static FName GetFieldModuleName(const UField* InField);

        // Gets the default name of the export project.
        static const FString& GetDefaultExportProjectName(const UField* InField);

        // Gets the default export namespace.
        // UFunction* is not supported.
        static const FString& GetDefaultExportNamespace(const UField* InField);

        // Gets the name of the assembly.
        // if it is Unreal native type, return UnrealSharpAssemblyName
        // if it is C# type, return the real assembly name
        static const FString& GetAssemblyName(const UField* InField);

        // Gets the C# full path.
        // If it is a C# type, the data will be obtained from the type data itself
        // if not, combine string by configuration
        static FString GetCSharpFullPath(const UField* InField);

        // Gets the name of the assembly.
        // use underlying type of property
        static const FString& GetAssemblyName(const FProperty* InProperty);

        // Gets the full path.
        // use underlying type of property
        static FString GetCSharpFullPath(const FProperty* InProperty);

        // Gets the property inner field.
        static UField* GetPropertyInnerField(const FProperty* InProperty);

        // A simple estimate of the size of the type behind it, not necessarily 100% accurate
        // It just calculates the smallest type that can fit all enumeration fields. 
        // The real size should be obtained from FProperty
        static int CalcEnumUnderlyingTypeSize(const UEnum* InEnum);

        // Get property count
        static int GetPropertyCount(const UStruct* InStruct);

        // Get property count witch filter        
        static int GetPropertyCount(const UStruct* InStruct, const TFunction<bool(const FProperty*)>& InFilter);

        // pickup property name from blueprint struct property name: {propertyName}_{index}_{guid}
        static FName ExtraUserDefinedStructPropertyName(const FProperty* InProperty);

        // Binds the c sharp method checked.
        static TSharedPtr<ICSharpMethodInvocation> BindCSharpMethodChecked(
            ICSharpRuntime* InRuntime,
            const FString& InAssemblyName,
            const FString& InNamespace,
            const FString& InClassName,
            const FString& InBaseSignature
        );

        // Binds the unreal engine c sharp method checked.
        // default use UnrealSharp.UnrealEngine.dll namespace : UnrealSharp.UnrealEngine
        static TSharedPtr<ICSharpMethodInvocation> BindUnrealEngineCSharpMethodChecked(
            ICSharpRuntime* InRuntime, 
            const FString& InClassName, 
            const FString& InBaseSignature
            );

    private:
        static bool IsExportToGameScriptsNativeField(const UField* InNativeField);
    };
}
