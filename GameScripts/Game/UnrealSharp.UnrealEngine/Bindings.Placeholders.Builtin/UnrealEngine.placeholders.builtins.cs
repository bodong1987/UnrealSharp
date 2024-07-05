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
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedTypeParameter
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharp.UnrealEngine.Bindings.Placeholders;

#pragma warning disable CS1591 // Missing XML annotation for publicly visible type or member

/*
 * We need to put the definitions of some built-in types here. 
 * If the original code is directly referenced, 
 * the reference range will be too large and the speed of preprocessing the Binding definition code will be too slow.
 * Therefore, if you add a new built-in binding type, you should add that type here as well.
*/

[BindingDefinition]
public interface IUnrealObject { }

[BindingDefinition]
public abstract class UObject : IUnrealObject { }

[BindingDefinition]
public abstract class UClass : UObject { }

[BindingDefinition]
public struct FName { }

[BindingDefinition]
public struct FText { }

[BindingDefinition]
public struct TSubclassOf<T> where T : IUnrealObject { }

[BindingDefinition]
public class TDelegate<T> where T : Delegate { }

[BindingDefinition]
public class TMulticastDelegate<T> where T : Delegate { }

[BindingDefinition]
public class TSoftObjectPtr<T> where T : UObject { }

[BindingDefinition]
public class TSoftClassPtr<T> where T : UObject { }

[BindingDefinition]
public class TMap<TKey, TValue> where TKey : notnull { }

[BindingDefinition]
public class TArray<T> { }

[BindingDefinition]
public class TSet<T> { }

#pragma warning restore CS1591 // Missing XML annotation for publicly visible type or member
