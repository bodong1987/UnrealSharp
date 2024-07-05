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

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnrealSharp.UnrealEngine.Collections;
using UnrealSharp.UnrealEngine.InteropService;
using UnrealSharp.Utils.Extensions;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharp.UnrealEngine;

/// <summary>
/// Class GenericObjectFactory.
/// This class is generally not used directly on the C# side. 
/// It is mainly used to initiate calls from the C++ side, 
/// to dynamically create C# objects, and to call C# functions.
/// Various cached or delegated indexes will be created internally to speed up the creation process.
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public static class GenericObjectFactory
{
    #region Cached Types
    /// <summary>
    /// The object types
    /// string to Type map
    /// </summary>
    private static Dictionary<string, Type> _objectTypes = new();
        
    /// <summary>
    /// The field types
    /// UField* to Type map
    /// </summary>
    private static Dictionary<IntPtr, Type> _fieldTypes = new();

    /// <summary>
    /// Delegate CreateArrayDelegateType
    /// </summary>
    /// <param name="addressOfArray">The address of array.</param>
    /// <param name="addressOfArrayProperty">The address of array property.</param>
    /// <returns>System.Object.</returns>
    public delegate object CreateArrayDelegateType(IntPtr addressOfArray, IntPtr addressOfArrayProperty);

    /// <summary>
    /// Delegate CreateSetDelegateType
    /// </summary>
    /// <param name="addressOfSet">The address of set.</param>
    /// <param name="addressOfSetProperty">The address of set property.</param>
    /// <returns>System.Object.</returns>
    public delegate object CreateSetDelegateType(IntPtr addressOfSet, IntPtr addressOfSetProperty);

    /// <summary>
    /// Delegate CreateMapDelegateType
    /// </summary>
    /// <param name="addressOfMap">The address of map.</param>
    /// <param name="addressOfMapProperty">The address of map property.</param>
    /// <returns>System.Object.</returns>
    public delegate object CreateMapDelegateType(IntPtr addressOfMap, IntPtr addressOfMapProperty);

    /// <summary>
    /// Delegate CreateSoftObjectDelegateType
    /// </summary>
    /// <param name="addressOfSoftObjectPtr">The address of soft object PTR.</param>
    /// <returns>System.Object.</returns>
    public delegate object CreateSoftObjectDelegateType(IntPtr addressOfSoftObjectPtr);

    /// <summary>
    /// Delegate CreateSoftClassDelegateType
    /// </summary>
    /// <param name="addressOfSoftClassPtr">The address of soft class PTR.</param>
    /// <returns>System.Object.</returns>
    public delegate object CreateSoftClassDelegateType(IntPtr addressOfSoftClassPtr);

    /// <summary>
    /// array factory
    /// </summary>
    private static Dictionary<Type, CreateArrayDelegateType> _createArrayFactory = new();

    /// <summary>
    /// set factory
    /// </summary>
    private static Dictionary<Type, CreateSetDelegateType> _createSetFactory = new();

    /// <summary>
    /// map factory
    /// </summary>
    private static Dictionary<int, CreateMapDelegateType> _createMapFactory = new();

    /// <summary>
    /// soft object factory
    /// </summary>
    private static Dictionary<Type, CreateSoftObjectDelegateType> _createSoftObjectFactory = new();

    /// <summary>
    /// soft class factory
    /// </summary>
    private static Dictionary<Type, CreateSoftClassDelegateType> _createSoftClassFactory = new();

    #endregion

    #region Type Accessor
    /// <summary>
    /// Gets the type.
    /// </summary>
    /// <param name="fullPath">The fullPath.</param>
    /// <returns>System.Nullable&lt;System.Type&gt;.</returns>
    /// <exception cref="System.Exception">Failed find C# type:{fullPath}</exception>
    public static Type GetType(string fullPath)
    {
        if (_objectTypes.TryGetValue(fullPath, out var type))
        {
            return type;
        }

        type = TypeExtensions.GetType(fullPath);

        if (type == null)
        {
            throw new Exception($"Failed find C# type:{fullPath}");
        }
            
        _objectTypes.Add(fullPath, type);
            
        return type;

    }

    /// <summary>
    /// Gets the type.
    /// </summary>
    /// <param name="fieldPtr">The field PTR.</param>
    /// <returns>System.Nullable&lt;System.Type&gt;.</returns>
    public static Type GetType(IntPtr fieldPtr)
    {
        if(_fieldTypes.TryGetValue(fieldPtr, out var type))
        {
            return type;
        }

        var fullPath = ClassInteropUtils.GetCSharpFullPathOfNativeField(fieldPtr);

        Logger.Ensure<Exception>(fullPath.IsNotNullOrEmpty());

        type = GetType(fullPath!);

        _fieldTypes.Add(fieldPtr, type);

        return type;
    }

    /// <summary>
    /// Gets the type of the element.
    /// </summary>
    /// <param name="addressOfProperty">The address of property.</param>
    /// <returns>System.Type.</returns>
    /// <exception cref="System.Exception">Unsupported type. property address:0x{addressOfProperty:x}</exception>
    public static Type GetElementType(IntPtr addressOfProperty)
    {
        var castFlags = PropertyInteropUtils.GetPropertyCastFlags(addressOfProperty);

        if ((castFlags & (ulong)EClassCastFlags.FIntProperty) != 0)
        {
            return typeof(int);
        }

        if ((castFlags & (ulong)EClassCastFlags.FUInt32Property) != 0)
        {
            return typeof(uint);
        }

        if ((castFlags & (ulong)EClassCastFlags.FBoolProperty) != 0)
        {
            return typeof(bool);
        }

        if ((castFlags & (ulong)EClassCastFlags.FFloatProperty) != 0)
        {
            return typeof(float);
        }

        if ((castFlags & (ulong)EClassCastFlags.FDoubleProperty) != 0)
        {
            return typeof(double);
        }

        if ((castFlags & (ulong)EClassCastFlags.FInt64Property) != 0)
        {
            return typeof(long);
        }

        if ((castFlags & (ulong)EClassCastFlags.FUInt64Property) != 0)
        {
            return typeof(ulong);
        }

        if ((castFlags & (ulong)EClassCastFlags.FStrProperty) != 0)
        {
            return typeof(string);
        }

        if ((castFlags & (ulong)EClassCastFlags.FNameProperty) != 0)
        {
            return typeof(FName);
        }

        if ((castFlags & (ulong)EClassCastFlags.FObjectProperty) != 0 ||
            (castFlags & (ulong)EClassCastFlags.FStructProperty) != 0 ||
            (castFlags & (ulong)EClassCastFlags.FEnumProperty) != 0)
        {
            var fieldPtr = PropertyInteropUtils.GetPropertyInnerField(addressOfProperty);

            Logger.Ensure<Exception>(fieldPtr != IntPtr.Zero, $"Failed find C# type of property 0x{addressOfProperty:x}");

            return GetType(fieldPtr);
        }

        if ((castFlags & (ulong)EClassCastFlags.FByteProperty) != 0)
        {
            return typeof(byte);
        }

        if ((castFlags & (ulong)EClassCastFlags.FTextProperty) != 0)
        {
            return typeof(FText);
        }

        if ((castFlags & (ulong)EClassCastFlags.FInt8Property) != 0)
        {
            return typeof(sbyte);
        }

        if ((castFlags & (ulong)EClassCastFlags.FInt16Property) != 0)
        {
            return typeof(short);
        }

        if ((castFlags & (ulong)EClassCastFlags.FUInt16Property) != 0)
        {
            return typeof(ushort);
        }

        if ((castFlags & (ulong)EClassCastFlags.FArrayProperty) != 0)
        {
            return typeof(TArray<>);
        }

        if ((castFlags & (ulong)EClassCastFlags.FSetProperty) != 0)
        {
            return typeof(TSet<>);
        }

        if ((castFlags & (ulong)EClassCastFlags.FMapProperty) != 0)
        {
            return typeof(TMap<,>);
        }

        if ((castFlags & (ulong)EClassCastFlags.FSoftObjectProperty) != 0)
        {
            return typeof(TSoftObjectPtr<>);
        }

        if ((castFlags & (ulong)EClassCastFlags.FSoftObjectProperty) != 0)
        {
            return typeof(TSoftClassPtr<>);
        }

        throw new Exception($"Unsupported type. property address:0x{addressOfProperty:x}");
    }

    /// <summary>
    /// Queries create array delegate.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>CreateArrayDelegateType.</returns>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050:RequiresDynamicCode")]
    public static CreateArrayDelegateType QueryCreateArrayDelegate(Type type)
    {
        if(_createArrayFactory.TryGetValue(type, out var arrayFactory))
        {
            return arrayFactory;
        }

        Type? arrayType;

        try
        {
            arrayType = typeof(TArray<>).MakeGenericType(type);
        }
        catch (Exception ex)
        {
            // If you use AOT, then you should ensure that this type actually appears in the code, otherwise you will not be able to dynamically construct this type.
            Logger.LogError(ex.Message);
            Logger.LogError($"Failed construct Generic TArray<{type.Name}>{Environment.NewLine}If you use AOT, then you should ensure that this type actually appears in the code, otherwise you will not be able to dynamically construct this type.");
            throw;
        }

        arrayFactory = (addressOfArray, addressOfArrayProperty) => Activator.CreateInstance(arrayType!, addressOfArray, addressOfArrayProperty)!;

        _createArrayFactory.Add(type, arrayFactory);

        return arrayFactory;
    }

    /// <summary>
    /// Queries create set delegate.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>CreateSetDelegateType.</returns>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050:RequiresDynamicCode")]
    public static CreateSetDelegateType QueryCreateSetDelegate(Type type)
    {
        if(_createSetFactory.TryGetValue(type, out var setFactory))
        {
            return setFactory;
        }

        Type? setType;

        try
        {
            setType = typeof(TSet<>).MakeGenericType(type);
        }
        catch (Exception ex)
        {
            // If you use AOT, then you should ensure that this type actually appears in the code, otherwise you will not be able to dynamically construct this type.
            Logger.LogError(ex.Message);
            Logger.LogError($"Failed construct Generic TSet<{type.Name}>{Environment.NewLine}If you use AOT, then you should ensure that this type actually appears in the code, otherwise you will not be able to dynamically construct this type.");
            throw;
        }

        setFactory = (addressOfSet, addressOfSetProperty) => Activator.CreateInstance(setType!, addressOfSet, addressOfSetProperty)!;

        _createSetFactory.Add(type, setFactory);

        return setFactory;
    }

    /// <summary>
    /// Queries create map delegate.
    /// </summary>
    /// <param name="keyType">Type of the key.</param>
    /// <param name="valueType">Type of the value.</param>
    /// <returns>CreateMapDelegateType.</returns>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050:RequiresDynamicCode")]
    public static CreateMapDelegateType QueryCreateMapDelegate(Type keyType, Type valueType)
    {
        var hashCode = HashCode.Combine(keyType.GetHashCode(), valueType.GetHashCode());

        if(_createMapFactory.TryGetValue(hashCode, out var createMapFactory))
        {
            return createMapFactory;
        }

        Type? mapType;

        try
        {
            mapType = typeof(TMap<,>).MakeGenericType(keyType, valueType);
        }
        catch (Exception ex)
        {
            // If you use AOT, then you should ensure that this type actually appears in the code, otherwise you will not be able to dynamically construct this type.
            Logger.LogError(ex.Message);
            Logger.LogError($"Failed construct Generic TMap<{keyType.Name},{valueType.Name}>{Environment.NewLine}If you use AOT, then you should ensure that this type actually appears in the code, otherwise you will not be able to dynamically construct this type.");
            throw;
        }

        createMapFactory = (addressOfMap, addressOfMapProperty) => Activator.CreateInstance(mapType!, addressOfMap, addressOfMapProperty)!;

        _createMapFactory.Add(hashCode, createMapFactory);

        return createMapFactory;
    }

    /// <summary>
    /// Queries create soft object delegate.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>CreateSoftObjectDelegateType.</returns>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050:RequiresDynamicCode")]
    public static CreateSoftObjectDelegateType QueryCreateSoftObjectDelegate(Type type)
    {
        if(_createSoftObjectFactory.TryGetValue(type, out var createSoftObjectFactory))
        {
            return createSoftObjectFactory;
        }

        Type? softObjectPtrType;

        try
        {
            softObjectPtrType = typeof(TSoftObjectPtr<>).MakeGenericType(type);
        }
        catch (Exception ex)
        {
            // If you use AOT, then you should ensure that this type actually appears in the code, otherwise you will not be able to dynamically construct this type.
            Logger.LogError(ex.Message);
            Logger.LogError($"Failed construct Generic TSoftObjectPtr<{type.Name}>{Environment.NewLine}If you use AOT, then you should ensure that this type actually appears in the code, otherwise you will not be able to dynamically construct this type.");
            throw;
        }

        createSoftObjectFactory = addressOfSoftObjectPtr => Activator.CreateInstance(softObjectPtrType!, addressOfSoftObjectPtr)!;

        _createSoftObjectFactory.Add(type, createSoftObjectFactory);

        return createSoftObjectFactory;
    }

    /// <summary>
    /// Queries create soft class delegate.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>CreateSoftClassDelegateType.</returns>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050:RequiresDynamicCode")]
    public static CreateSoftClassDelegateType QueryCreateSoftClassDelegate(Type type)
    {
        if (_createSoftClassFactory.TryGetValue(type, out var createSoftClassFactory))
        {
            return createSoftClassFactory;
        }

        Type? softClassPtrType;

        try
        {
            softClassPtrType = typeof(TSoftClassPtr<>).MakeGenericType(type);
        }
        catch (Exception ex)
        {
            // If you use AOT, then you should ensure that this type actually appears in the code, otherwise you will not be able to dynamically construct this type.
            Logger.LogError(ex.Message);
            Logger.LogError($"Failed construct Generic TSoftClassPtr<{type.Name}>{Environment.NewLine}If you use AOT, then you should ensure that this type actually appears in the code, otherwise you will not be able to dynamically construct this type.");
            throw;
        }

        createSoftClassFactory = addressOfSoftClassPtr => Activator.CreateInstance(softClassPtrType!, addressOfSoftClassPtr)!;

        _createSoftClassFactory.Add(type, createSoftClassFactory);

        return createSoftClassFactory;
    }
    #endregion

    #region Array
    /// <summary>
    /// Creates the array.
    /// </summary>
    /// <param name="addressOfArray">The address of array.</param>
    /// <param name="addressOfArrayProperty">The address of array property.</param>
    /// <returns>System.Object.</returns>        
    public static object CreateArray(IntPtr addressOfArray, IntPtr addressOfArrayProperty)
    {
        var elementProperty = ArrayInteropUtils.GetElementPropertyOfArray(addressOfArrayProperty);

        Logger.Ensure<Exception>(elementProperty != IntPtr.Zero, "Failed get ElementProperty of ArrayProperty:0x{0:x}", addressOfArrayProperty);

        var elementType = GetElementType(elementProperty);
        var factory = QueryCreateArrayDelegate(elementType);

        return factory(addressOfArray, addressOfArrayProperty);
    }

    /// <summary>
    /// Writes the array.
    /// </summary>
    /// <param name="addressOfArray">The address of array.</param>
    /// <param name="addressOfArrayProperty">The address of array property.</param>
    /// <param name="enumerable">The enumerable.</param>
    /// <returns><c>true</c> if success, <c>false</c> otherwise.</returns>
    public static bool WriteArray(IntPtr addressOfArray, IntPtr addressOfArrayProperty, IEnumerable enumerable)
    {
        var newArray = CreateArray(addressOfArray, addressOfArrayProperty);
        var dataView = newArray as IUnrealCollectionDataView;

        return dataView != null && dataView.CopyFrom(enumerable);
    }
    #endregion

    #region Set
    /// <summary>
    /// Creates the set.
    /// </summary>
    /// <param name="addressOfSet">The address of set.</param>
    /// <param name="addressOfSetProperty">The address of set property.</param>
    /// <returns>System.Object.</returns>
    public static object CreateSet(IntPtr addressOfSet, IntPtr addressOfSetProperty)
    {
        var elementProperty = SetInteropUtils.GetElementPropertyOfSet(addressOfSetProperty);

        Logger.Ensure<Exception>(elementProperty != IntPtr.Zero, "Failed get ElementProperty of SetProperty:0x{0:x}", addressOfSetProperty);

        var elementType = GetElementType(elementProperty);
        var factory = QueryCreateSetDelegate(elementType);

        return factory(addressOfSet, addressOfSetProperty);
    }

    /// <summary>
    /// Writes the set.
    /// </summary>
    /// <param name="addressOfSet">The address of set.</param>
    /// <param name="addressOfSetProperty">The address of set property.</param>
    /// <param name="enumerable">The enumerable.</param>
    /// <returns><c>true</c> if success, <c>false</c> otherwise.</returns>
    public static bool WriteSet(IntPtr addressOfSet, IntPtr addressOfSetProperty, IEnumerable enumerable)
    {
        var newSet = CreateSet(addressOfSet, addressOfSetProperty);
        var dataView = newSet as IUnrealCollectionDataView;

        return dataView != null && dataView.CopyFrom(enumerable);
    }
    #endregion

    #region Map
    /// <summary>
    /// Creates the map.
    /// </summary>
    /// <param name="addressOfMap">The address of map.</param>
    /// <param name="addressOfMapProperty">The address of map property.</param>
    /// <returns>System.Object.</returns>
    public static object CreateMap(IntPtr addressOfMap, IntPtr addressOfMapProperty)
    {
        var keyElementProperty = MapInteropUtils.GetKeyPropertyOfMap(addressOfMapProperty);
        var valueElementProperty = MapInteropUtils.GetValuePropertyOfMap(addressOfMapProperty);

        Logger.Ensure<Exception>(keyElementProperty != IntPtr.Zero, "Failed get KeyElementProperty of MapProperty:0x{0:x}", addressOfMapProperty);
        Logger.Ensure<Exception>(valueElementProperty != IntPtr.Zero, "Failed get ValueElementProperty of MapProperty:0x{0:x}", addressOfMapProperty);

        var keyElementType = GetElementType(keyElementProperty);
        var valueElementType = GetElementType(valueElementProperty);

        var factory = QueryCreateMapDelegate(keyElementType, valueElementType);

        return factory(addressOfMap, addressOfMapProperty);
    }

    /// <summary>
    /// Writes the map.
    /// </summary>
    /// <param name="addressOfMap">The address of map.</param>
    /// <param name="addressOfMapProperty">The address of map property.</param>
    /// <param name="enumerable">The enumerable.</param>
    /// <returns><c>true</c> if success, <c>false</c> otherwise.</returns>
    public static bool WriteMap(IntPtr addressOfMap, IntPtr addressOfMapProperty, IEnumerable enumerable)
    {
        var newMap = CreateMap(addressOfMap, addressOfMapProperty);
        var dataView = newMap as IUnrealCollectionDataView;

        return dataView != null && dataView.CopyFrom(enumerable);
    }
    #endregion

    #region Soft Object
    /// <summary>
    /// Creates the soft object PTR.
    /// </summary>
    /// <param name="addressOfSoftObjectPtr">The address of soft object PTR.</param>
    /// <param name="addressOfSoftObjectProperty">The address of soft object property.</param>
    /// <returns>System.Object.</returns>
    public static object CreateSoftObjectPtr(IntPtr addressOfSoftObjectPtr, IntPtr addressOfSoftObjectProperty)
    {
        var typeClass = PropertyInteropUtils.GetPropertyInnerField(addressOfSoftObjectProperty);

        Logger.Ensure<Exception>(typeClass != IntPtr.Zero);

        var type = GetType(typeClass);

        Logger.EnsureNotNull(type);

        var factory = QueryCreateSoftObjectDelegate(type);

        return factory(addressOfSoftObjectPtr);
    }

    /// <summary>
    /// Writes the soft object PTR.
    /// </summary>
    /// <param name="addressOfNativeSoftObjectPtr">The address of native soft object PTR.</param>
    /// <param name="softObjectPtr">The soft object PTR.</param>
    public static void WriteSoftObjectPtr(IntPtr addressOfNativeSoftObjectPtr, ISoftObjectPtr? softObjectPtr)
    {
        softObjectPtr?.WriteTo(addressOfNativeSoftObjectPtr);
    }
    #endregion

    #region Soft Class
    /// <summary>
    /// Creates the soft class PTR.
    /// </summary>
    /// <param name="addressOfSoftClassPtr">The address of soft class PTR.</param>
    /// <param name="addressOfSoftClassProperty">The address of soft class property.</param>
    /// <returns>System.Object.</returns>
    public static object CreateSoftClassPtr(IntPtr addressOfSoftClassPtr, IntPtr addressOfSoftClassProperty)
    {
        var typeClass = PropertyInteropUtils.GetPropertyInnerField(addressOfSoftClassProperty);

        Logger.Ensure<Exception>(typeClass != IntPtr.Zero);

        var type = GetType(typeClass);

        Logger.EnsureNotNull(type);

        var factory = QueryCreateSoftClassDelegate(type);

        return factory(addressOfSoftClassPtr);
    }

    /// <summary>
    /// Writes the soft class PTR.
    /// </summary>
    /// <param name="addressOfNativeSoftClassPtr">The address of native soft class PTR.</param>
    /// <param name="softClassPtr">The soft class PTR.</param>
    public static void WriteSoftClassPtr(IntPtr addressOfNativeSoftClassPtr, ISoftClassPtr? softClassPtr)
    {
        softClassPtr?.WriteTo(addressOfNativeSoftClassPtr);
    }
    #endregion
}