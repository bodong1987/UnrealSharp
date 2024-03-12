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

namespace UnrealSharp.UnrealEngine
{
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
        private static Dictionary<string, Type> ObjectTypes = new Dictionary<string, Type>();
        /// <summary>
        /// The field types
        /// UField* to Type map
        /// </summary>
        private static Dictionary<IntPtr, Type> FieldTypes = new Dictionary<IntPtr, Type>();

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
        /// The create array factory
        /// </summary>
        private static Dictionary<Type, CreateArrayDelegateType> CreateArrayFactory = new Dictionary<Type, CreateArrayDelegateType>();

        /// <summary>
        /// The create set factory
        /// </summary>
        private static Dictionary<Type, CreateSetDelegateType> CreateSetFactory = new Dictionary<Type, CreateSetDelegateType>();

        /// <summary>
        /// The create map factory
        /// </summary>
        private static Dictionary<int, CreateMapDelegateType> CreateMapFactory = new Dictionary<int, CreateMapDelegateType>();

        /// <summary>
        /// The create soft object factory
        /// </summary>
        private static Dictionary<Type, CreateSoftObjectDelegateType> CreateSoftObjectFactory = new Dictionary<Type, CreateSoftObjectDelegateType>();

        /// <summary>
        /// The create soft class factory
        /// </summary>
        private static Dictionary<Type, CreateSoftClassDelegateType> CreateSoftClassFactory = new Dictionary<Type, CreateSoftClassDelegateType>();

        #endregion

        #region Type Accessor
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <param name="fullpath">The fullpath.</param>
        /// <returns>System.Nullable&lt;System.Type&gt;.</returns>
        /// <exception cref="System.Exception">Failed find C# type:{fullpath}</exception>
        public static System.Type? GetType(string fullpath)
        {
            if (ObjectTypes.TryGetValue(fullpath, out var type))
            {
                return type;
            }

            type = TypeExtensions.GetType(fullpath);

            if (type != null)
            {
                ObjectTypes.Add(fullpath, type);
                return type;
            }

            throw new Exception($"Failed find C# type:{fullpath}");
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <param name="fieldPtr">The field PTR.</param>
        /// <returns>System.Nullable&lt;System.Type&gt;.</returns>
        public static System.Type? GetType(IntPtr fieldPtr)
        {
            if(FieldTypes.TryGetValue(fieldPtr, out var type))
            {
                return type;
            }

            var fullpath = ClassInteropUtils.GetCSharpFullPathOfNativeField(fieldPtr);

            Logger.Ensure<Exception>(fullpath.IsNotNullOrEmpty());

            type = GetType(fullpath!);
            if (type != null)
            {
                FieldTypes.Add(fieldPtr, type);
            }

            return type;
        }

        /// <summary>
        /// Gets the type of the element.
        /// </summary>
        /// <param name="addressOfProperty">The address of property.</param>
        /// <returns>System.Type.</returns>
        /// <exception cref="System.Exception">Unsupported type. property address:0x{addressOfProperty:x}</exception>
        public static System.Type GetElementType(IntPtr addressOfProperty)
        {
            UInt64 castFlags = PropertyInteropUtils.GetPropertyCastFlags(addressOfProperty);

            if ((castFlags & (UInt64)EClassCastFlags.FIntProperty) != 0)
            {
                return typeof(int);
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FUInt32Property) != 0)
            {
                return typeof(uint);
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FBoolProperty) != 0)
            {
                return typeof(bool);
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FFloatProperty) != 0)
            {
                return typeof(float);
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FDoubleProperty) != 0)
            {
                return typeof(double);
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FInt64Property) != 0)
            {
                return typeof(Int64);
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FUInt64Property) != 0)
            {
                return typeof(UInt64);
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FStrProperty) != 0)
            {
                return typeof(string);
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FNameProperty) != 0)
            {
                return typeof(FName);
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FObjectProperty) != 0 ||
                (castFlags & (UInt64)EClassCastFlags.FStructProperty) != 0 ||
                (castFlags & (UInt64)EClassCastFlags.FEnumProperty) != 0)
            {
                var fieldPtr = PropertyInteropUtils.GetPropertyInnerField(addressOfProperty);

                Logger.Ensure<Exception>(fieldPtr != IntPtr.Zero, $"Failed find C# type of property 0x{addressOfProperty:x}");

                return GetType(fieldPtr)!;
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FByteProperty) != 0)
            {
                return typeof(byte);
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FTextProperty) != 0)
            {
                return typeof(FText);
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FInt8Property) != 0)
            {
                return typeof(sbyte);
            }            
            else if ((castFlags & (UInt64)EClassCastFlags.FInt16Property) != 0)
            {
                return typeof(Int16);
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FUInt16Property) != 0)
            {
                return typeof(UInt16);
            }
            
            else if ((castFlags & (UInt64)EClassCastFlags.FArrayProperty) != 0)
            {
                return typeof(TArray<>);
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FSetProperty) != 0)
            {
                return typeof(TSet<>);
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FMapProperty) != 0)
            {
                return typeof(TMap<,>);
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FSoftObjectProperty) != 0)
            {
                return typeof(TSoftObjectPtr<>);
            }
            else if ((castFlags & (UInt64)EClassCastFlags.FSoftObjectProperty) != 0)
            {
                return typeof(TSoftClassPtr<>);
            }

            throw new Exception($"Unsupported type. property address:0x{addressOfProperty:x}");
        }

        /// <summary>
        /// Queries the create array delegate.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>CreateArrayDelegateType.</returns>
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050:RequiresDynamicCode")]
        public static CreateArrayDelegateType QueryCreateArrayDelegate(Type type)
        {
            if(CreateArrayFactory.TryGetValue(type, out var arrayFactory))
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

            arrayFactory = (IntPtr addressOfArray, IntPtr addressOfArrayProperty) => Activator.CreateInstance(arrayType!, addressOfArray, addressOfArrayProperty)!;

            CreateArrayFactory.Add(type, arrayFactory);

            return arrayFactory;
        }

        /// <summary>
        /// Queries the create set delegate.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>CreateSetDelegateType.</returns>
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050:RequiresDynamicCode")]
        public static CreateSetDelegateType QueryCreateSetDelegate(Type type)
        {
            if(CreateSetFactory.TryGetValue(type, out var setFactory))
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

            setFactory = (IntPtr addressOfSet, IntPtr addressOfSetProperty) => Activator.CreateInstance(setType!, addressOfSet, addressOfSetProperty)!;

            CreateSetFactory.Add(type, setFactory);

            return setFactory;
        }

        /// <summary>
        /// Queries the create map delegate.
        /// </summary>
        /// <param name="keyType">Type of the key.</param>
        /// <param name="valueType">Type of the value.</param>
        /// <returns>CreateMapDelegateType.</returns>
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050:RequiresDynamicCode")]
        public static CreateMapDelegateType QueryCreateMapDelegate(Type keyType, Type valueType)
        {
            int hashCode = HashCode.Combine(keyType.GetHashCode(), valueType.GetHashCode());

            if(CreateMapFactory.TryGetValue(hashCode, out var createMapFactory))
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

            createMapFactory = (IntPtr addressOfMap, IntPtr addressOfMapProperty) => Activator.CreateInstance(mapType!, addressOfMap, addressOfMapProperty)!;

            CreateMapFactory.Add(hashCode, createMapFactory);

            return createMapFactory;
        }

        /// <summary>
        /// Queries the create soft object delegate.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>CreateSoftObjectDelegateType.</returns>
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050:RequiresDynamicCode")]
        public static CreateSoftObjectDelegateType QueryCreateSoftObjectDelegate(Type type)
        {
            if(CreateSoftObjectFactory.TryGetValue(type, out var createSoftObjectFactory))
            {
                return createSoftObjectFactory;
            }

            Type? SoftObjectPtrType = null;

            try
            {
                SoftObjectPtrType = typeof(TSoftObjectPtr<>).MakeGenericType(type!);
            }
            catch (Exception ex)
            {
                // If you use AOT, then you should ensure that this type actually appears in the code, otherwise you will not be able to dynamically construct this type.
                Logger.LogError(ex.Message);
                Logger.LogError($"Failed construct Generic TSoftObjectPtr<{type!.Name}>{Environment.NewLine}If you use AOT, then you should ensure that this type actually appears in the code, otherwise you will not be able to dynamically construct this type.");
                throw;
            }

            createSoftObjectFactory = (IntPtr addressOfSoftObjectPtr) => Activator.CreateInstance(SoftObjectPtrType!, addressOfSoftObjectPtr)!;

            CreateSoftObjectFactory.Add(type, createSoftObjectFactory);

            return createSoftObjectFactory;
        }

        /// <summary>
        /// Queries the create soft class delegate.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>CreateSoftClassDelegateType.</returns>
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050:RequiresDynamicCode")]
        public static CreateSoftClassDelegateType QueryCreateSoftClassDelegate(Type type)
        {
            if (CreateSoftClassFactory.TryGetValue(type, out var createSoftClassFactory))
            {
                return createSoftClassFactory;
            }

            Type? SoftClassPtrType = null;

            try
            {
                SoftClassPtrType = typeof(TSoftClassPtr<>).MakeGenericType(type!);
            }
            catch (Exception ex)
            {
                // If you use AOT, then you should ensure that this type actually appears in the code, otherwise you will not be able to dynamically construct this type.
                Logger.LogError(ex.Message);
                Logger.LogError($"Failed construct Generic TSoftClassPtr<{type!.Name}>{Environment.NewLine}If you use AOT, then you should ensure that this type actually appears in the code, otherwise you will not be able to dynamically construct this type.");
                throw;
            }

            createSoftClassFactory = (IntPtr addressOfSoftClassPtr) => Activator.CreateInstance(SoftClassPtrType!, addressOfSoftClassPtr)!;

            CreateSoftClassFactory.Add(type, createSoftClassFactory);

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
            IntPtr ElementProperty = ArrayInteropUtils.GetElementPropertyOfArray(addressOfArrayProperty);

            Logger.Ensure<Exception>(ElementProperty != IntPtr.Zero, "Failed get ElementProperty of ArrayProperty:0x{0:x}", addressOfArrayProperty);

            Type elementType = GetElementType(ElementProperty);
            var factory = QueryCreateArrayDelegate(elementType);

            return factory(addressOfArray, addressOfArrayProperty);
        }

        /// <summary>
        /// Writes the array.
        /// </summary>
        /// <param name="addressOfArray">The address of array.</param>
        /// <param name="addressOfArrayProperty">The address of array property.</param>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool WriteArray(IntPtr addressOfArray, IntPtr addressOfArrayProperty, IEnumerable enumerable)
        {
            object NewArray = CreateArray(addressOfArray, addressOfArrayProperty);
            IUnrealCollectionDataView? dataView = NewArray as IUnrealCollectionDataView;

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
            IntPtr ElementProperty = SetInteropUtils.GetElementPropertyOfSet(addressOfSetProperty);

            Logger.Ensure<Exception>(ElementProperty != IntPtr.Zero, "Failed get ElementProperty of SetProperty:0x{0:x}", addressOfSetProperty);

            Type elementType = GetElementType(ElementProperty);
            var factory = QueryCreateSetDelegate(elementType);

            return factory(addressOfSet, addressOfSetProperty);
        }

        /// <summary>
        /// Writes the set.
        /// </summary>
        /// <param name="addressOfSet">The address of set.</param>
        /// <param name="addressOfSetProperty">The address of set property.</param>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool WriteSet(IntPtr addressOfSet, IntPtr addressOfSetProperty, IEnumerable enumerable)
        {
            object NewSet = CreateSet(addressOfSet, addressOfSetProperty);
            IUnrealCollectionDataView? dataView = NewSet as IUnrealCollectionDataView;

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
            IntPtr KeyElementProperty = MapInteropUtils.GetKeyPropertyOfMap(addressOfMapProperty);
            IntPtr ValueElementProperty = MapInteropUtils.GetValuePropertyOfMap(addressOfMapProperty);

            Logger.Ensure<Exception>(KeyElementProperty != IntPtr.Zero, "Failed get KeyElementProperty of MapProperty:0x{0:x}", addressOfMapProperty);
            Logger.Ensure<Exception>(ValueElementProperty != IntPtr.Zero, "Failed get ValueElementProperty of MapProperty:0x{0:x}", addressOfMapProperty);

            Type KeyElementType = GetElementType(KeyElementProperty);
            Type ValueElementType = GetElementType(ValueElementProperty);

            var factory = QueryCreateMapDelegate(KeyElementType, ValueElementType);

            return factory(addressOfMap, addressOfMapProperty);
        }

        /// <summary>
        /// Writes the map.
        /// </summary>
        /// <param name="addressOfMap">The address of map.</param>
        /// <param name="addressOfMapProperty">The address of map property.</param>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool WriteMap(IntPtr addressOfMap, IntPtr addressOfMapProperty, IEnumerable enumerable)
        {
            object NewMap = CreateMap(addressOfMap, addressOfMapProperty);
            IUnrealCollectionDataView? dataView = NewMap as IUnrealCollectionDataView;

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
            IntPtr TypeClass = PropertyInteropUtils.GetPropertyInnerField(addressOfSoftObjectProperty);

            Logger.Ensure<Exception>(TypeClass != IntPtr.Zero);

            var type = GetType(TypeClass);

            Logger.EnsureNotNull(type);

            var factory = QueryCreateSoftObjectDelegate(type);

            return factory(addressOfSoftObjectPtr);
        }

        /// <summary>
        /// Writes the soft object PTR.
        /// </summary>
        /// <param name="addressOfNativeSoftObjectPtr">The address of native soft object PTR.</param>
        /// <param name="softObjectPtr">The soft object PTR.</param>
        public static void WriteSoftObjectPtr(IntPtr addressOfNativeSoftObjectPtr, ISoftObjectPtr softObjectPtr)
        {
            if(softObjectPtr != null)
            {
                softObjectPtr.WriteTo(addressOfNativeSoftObjectPtr);
            }
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
            IntPtr TypeClass = PropertyInteropUtils.GetPropertyInnerField(addressOfSoftClassProperty);

            Logger.Ensure<Exception>(TypeClass != IntPtr.Zero);

            var type = GetType(TypeClass);

            Logger.EnsureNotNull(type);

            var factory = QueryCreateSoftClassDelegate(type);

            return factory(addressOfSoftClassPtr);
        }

        /// <summary>
        /// Writes the soft class PTR.
        /// </summary>
        /// <param name="addressOfNativeSoftClassPtr">The address of native soft class PTR.</param>
        /// <param name="softClassPtr">The soft class PTR.</param>
        public static void WriteSoftClassPtr(IntPtr addressOfNativeSoftClassPtr, ISoftClassPtr softClassPtr)
        {
            if(softClassPtr != null)
            {
                softClassPtr.WriteTo(addressOfNativeSoftClassPtr);
            }            
        }
        #endregion
    }
}
