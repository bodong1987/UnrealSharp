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
using System.Reflection;
using UnrealSharp.Utils.Misc;

namespace UnrealSharp.UnrealEngine.InteropService;

#region Interface
/// <summary>
/// Interface IInteropPolicy
/// Mainly used to support the interaction between elements in the container and C++
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IInteropPolicy<T>
{
    /// <summary>
    /// Reads the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>T.</returns>
    T Read(IntPtr address);

    /// <summary>
    /// Writes the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="value">The value.</param>
    void Write(IntPtr address, T value);
}
#endregion

#region Boolean
/// <summary>
/// Class BooleanInteropPolicy.
/// </summary>
internal class BooleanInteropPolicy : IInteropPolicy<bool>
{
    /// <summary>
    /// Reads the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    public bool Read(nint address)
    {
        return InteropUtils.GetBoolean(address, 0);
    }

    /// <summary>
    /// Writes the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="value">if set to <c>true</c> [value].</param>
    public void Write(nint address, bool value)
    {
        InteropUtils.SetBoolean(address, 0, value);
    }
}
#endregion

#region Numeric
/// <summary>
/// Class TNumericInteropPolicy.
/// Implements the <see cref="UnrealSharp.UnrealEngine.InteropService.IInteropPolicy{T}" />
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="UnrealSharp.UnrealEngine.InteropService.IInteropPolicy{T}" />
// ReSharper disable once InconsistentNaming
internal class TNumericInteropPolicy<T> : IInteropPolicy<T>
    where T : struct
#if NET7_0_OR_GREATER
    , System.Numerics.INumber<T>
#endif
{
    /// <summary>
    /// Reads the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>T.</returns>
    public T Read(nint address)
    {
        return InteropUtils.GetNumeric<T>(address, 0);
    }

    /// <summary>
    /// Writes the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="value">The value.</param>
    public void Write(nint address, T value)
    {
        InteropUtils.SetNumeric(address, 0, value);
    }
}
#endregion

#region String
/// <summary>
/// Class StringInteropPolicy.
/// </summary>
internal class StringInteropPolicy : IInteropPolicy<string>
{
    /// <summary>
    /// Reads the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>System.String.</returns>
    public string Read(nint address)
    {
        var text = InteropUtils.GetString(address, 0);
        return text ?? string.Empty;
    }

    /// <summary>
    /// Writes the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="value">The value.</param>
    public void Write(nint address, string value)
    {
        InteropUtils.SetString(address, 0, value);
    }
}
#endregion

#region String View
/// <summary>
/// Class StringViewInteropPolicy.
/// </summary>
internal class StringViewInteropPolicy : IInteropPolicy<FStringView>
{
    /// <summary>
    /// Reads the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>FStringView.</returns>
    public FStringView Read(nint address)
    {
        return InteropUtils.GetStringView(address, 0);
    }

    /// <summary>
    /// Writes the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="value">The value.</param>
    public void Write(nint address, FStringView value)
    {
        InteropUtils.SetStringView(address, 0, value);
    }
}
#endregion

#region Name
/// <summary>
/// Class NameInteropPolicy.
/// </summary>
internal class NameInteropPolicy : IInteropPolicy<FName>
{
    /// <summary>
    /// Reads the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>FName.</returns>
    public FName Read(nint address)
    {
        return FName.FromNative(address, 0);
    }

    /// <summary>
    /// Writes the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="value">The value.</param>
    public void Write(nint address, FName value)
    {
        FName.ToNative(address, 0, ref value);
    }
}
#endregion

#region Text
/// <summary>
/// Class TextInteropPolicy.
/// </summary>
internal class TextInteropPolicy : IInteropPolicy<FText>
{
    /// <summary>
    /// Reads the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>FText.</returns>
    public FText Read(nint address)
    {
        return FText.FromNative(address, 0);
    }

    /// <summary>
    /// Writes the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="value">The value.</param>
    public void Write(nint address, FText value)
    {
        FText.ToNative(address, 0, ref value);
    }
}
#endregion

#region Enum
/// <summary>
/// Class TEnumInteropPolicy.
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once InconsistentNaming
public class TEnumInteropPolicy<T> : IInteropPolicy<T>
    where T : unmanaged, Enum
{
    /// <summary>
    /// Reads the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>T.</returns>
    public T Read(nint address)
    {
        unsafe
        {
            return InteropUtils.GetEnum<T>(address, 0, sizeof(T));
        }            
    }

    /// <summary>
    /// Writes the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="value">The value.</param>
    public void Write(nint address, T value)
    {
        unsafe
        {
            InteropUtils.SetEnum(address, 0, sizeof(T), value);
        }
    }
}
#endregion

#region Struct
/// <summary>
/// Class TStructInteropPolicy.
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once InconsistentNaming
internal class TStructInteropPolicy<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]T> : IInteropPolicy<T>
    where T : struct
{
    /// <summary>
    /// Delegate FromNativeDelegate
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>T.</returns>
    private delegate T FromNativeDelegate(IntPtr address, int offset);

    /// <summary>
    /// Delegate ToNativeDelegate
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="value">The value.</param>
    private delegate void ToNativeDelegate(IntPtr address, int offset, ref T value);

    /// <summary>
    /// From native
    /// </summary>
    private static readonly FromNativeDelegate FromNative;

    /// <summary>
    /// Converts to native.
    /// </summary>
    private static readonly ToNativeDelegate ToNative;

    /// <summary>
    /// Initializes static members of the <see cref="TStructInteropPolicy{T}"/> class.
    /// </summary>
    static TStructInteropPolicy()
    {
        var fromNativeMethod = typeof(T).GetMethod("FromNative", BindingFlags.Public|BindingFlags.Static);
        var toNativeMethod = typeof(T).GetMethod("ToNative", BindingFlags.Public | BindingFlags.Static);

        Logger.EnsureNotNull(fromNativeMethod, $"Failed find {typeof(T).FullName}:FromNative");
        Logger.EnsureNotNull(toNativeMethod, $"Failed find {typeof(T).FullName}:ToNative");

        FromNative = (FromNativeDelegate)Delegate.CreateDelegate(typeof(FromNativeDelegate), fromNativeMethod);
        ToNative = (ToNativeDelegate)Delegate.CreateDelegate(typeof(ToNativeDelegate), toNativeMethod);
    }

    /// <summary>
    /// Reads the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>T.</returns>
    public T Read(nint address)
    {
        return FromNative(address, 0);
    }

    /// <summary>
    /// Writes the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="value">The value.</param>
    public void Write(nint address, T value)
    {
        ToNative(address, 0, ref value);
    }
}
#endregion

#region Object
/// <summary>
/// Class TObjectInteropPolicy.
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once InconsistentNaming
public class TObjectInteropPolicy<T> : IInteropPolicy<T>
    where T : class, IUObjectInterface
{
#pragma warning disable CS8766 // Nullability of reference types in return type of doesn't match implicitly implemented member (possibly because of nullability attributes).
    /// <summary>
    /// Reads the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>System.Nullable&lt;T&gt;.</returns>
    public T? Read(IntPtr address)
#pragma warning restore CS8766 
    {
        return InteropUtils.GetObject<T>(address, 0);
    }

    /// <summary>
    /// Writes the specified address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="value">The value.</param>
    public void Write(IntPtr address, T value)
    {
        InteropUtils.SetObject(address, 0, value);
    }
}
#endregion

#region Interop Policy Factory
/// <summary>
/// Class InteropPolicyFactory.
/// Mainly used to support the interaction between elements in the container and C++
/// </summary>
public static class InteropPolicyFactory
{
    /// <summary>
    /// The interop policies
    /// </summary>
    private static readonly Dictionary<Type, object> InteropPolicies = new();

    /// <summary>
    /// Initializes static members of the <see cref="InteropPolicyFactory"/> class.
    /// </summary>
    static InteropPolicyFactory()
    {
        // add built-in types
        AddPolicy(new BooleanInteropPolicy());
        AddPolicy(new TNumericInteropPolicy<sbyte>());
        AddPolicy(new TNumericInteropPolicy<byte>());
        AddPolicy(new TNumericInteropPolicy<short>());
        AddPolicy(new TNumericInteropPolicy<ushort>());
        AddPolicy(new TNumericInteropPolicy<int>());
        AddPolicy(new TNumericInteropPolicy<uint>());
        AddPolicy(new TNumericInteropPolicy<long>());
        AddPolicy(new TNumericInteropPolicy<ulong>());
        AddPolicy(new TNumericInteropPolicy<float>());
        AddPolicy(new TNumericInteropPolicy<double>());
        AddPolicy(new StringInteropPolicy());
        AddPolicy(new StringViewInteropPolicy());
        AddPolicy(new NameInteropPolicy());
        AddPolicy(new TextInteropPolicy());

        // add built-in structures
        AddPolicy(new FVectorInteropPolicy());
        AddPolicy(new FRotatorInteropPolicy());
        AddPolicy(new FColorInteropPolicy());
        AddPolicy(new FLinearColorInteropPolicy());
        AddPolicy(new FGuidInteropPolicy());
        AddPolicy(new FTopLevelAssetPathInteropPolicy());

        // for base objects
        AddPolicy(new TObjectInteropPolicy<UObject>());
        AddPolicy(new TObjectInteropPolicy<UClass>());
    }

    /// <summary>
    /// Adds the policy.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="policy">The policy.</param>
    /// <param name="overrideIfExists">if set to <c>true</c> [override if exists].</param>
    public static void AddPolicy<T>(IInteropPolicy<T> policy, bool overrideIfExists = false)
    {
        if(!InteropPolicies.TryAdd(typeof(T), policy) && overrideIfExists)
        {
            InteropPolicies[typeof(T)] = policy;
        }            
    }

    /// <summary>
    /// Finds the policy.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>System.Nullable&lt;System.Object&gt;.</returns>        
    private static object? FindPolicy(Type type)
    {
        if(InteropPolicies.TryGetValue(type, out var policy))
        {
            return policy;
        }

        var interopPolicyTypeName = $"{type.Namespace}.{type.Name}InteropPolicy";

        // We use other methods to ensure that this type will not be trimmed
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        var interopType = type.Assembly.GetType(interopPolicyTypeName);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

        Logger.EnsureNotNull(interopType, $"Failed find interop policy type {interopPolicyTypeName} for {type.FullName}");

#pragma warning disable IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
        policy = Activator.CreateInstance(interopType);
#pragma warning restore IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.

        Logger.EnsureNotNull(interopType, $"Failed create interop policy type {interopPolicyTypeName} for {type.FullName}");

        InteropPolicies.Add(type, policy!);

        return policy;
    }

    /// <summary>
    /// Gets the policy.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>IInteropPolicy&lt;T&gt;.</returns>
    public static IInteropPolicy<T> GetPolicy<T>()
    {
        var policyObject = FindPolicy(typeof(T));

        var policy = policyObject as IInteropPolicy<T>;

        Logger.EnsureNotNull(policy, $"Failed get InteropPolicy for type: {typeof(T).FullName}");

        return policy;
    }
}
#endregion