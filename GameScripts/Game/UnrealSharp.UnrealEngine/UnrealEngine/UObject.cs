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

using System.Diagnostics;
using UnrealSharp.UnrealEngine.InteropService;
using UnrealSharp.Utils.Misc;
using UnrealSharp.Utils.UnrealEngine;

namespace UnrealSharp.UnrealEngine;

#region Interface

/// <summary>
/// Interface IUnrealObject
/// </summary>
public interface IUObjectInterface : IUnrealDataView
{
    /// <summary>
    /// Gets the class.
    /// </summary>
    /// <returns>System.Nullable&lt;UClass&gt;.</returns>
    UClass? GetClass();

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <returns>System.Nullable&lt;System.String&gt;.</returns>
    string? GetName();
}

#endregion

/// <summary>
/// Class UObject.
/// </summary>
[NativeBinding("Object", "UObject", "/Script/CoreUObject.Object")]
public partial class UObject : IUObjectInterface
{
    #region Fields

    /// <summary>
    /// The unreal sharp name
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public const string UnrealSharpName = "UnrealSharp";

    /// <summary>
    /// Gets the native PTR.
    /// </summary>
    /// <value>The native PTR.</value>
#if !DEBUG
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
    private IntPtr _nativePtr;

    /// <summary>
    /// The class cache
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private UClass? _classCache;

    /// <summary>
    /// The object class
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static UClass? _objectClass;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="UObject" /> class.
    /// </summary>
    /// <param name="unrealObjectPtr">The unreal object PTR.</param>
    public UObject(IntPtr unrealObjectPtr)
    {
        Logger.Ensure<AccessViolationException>(unrealObjectPtr != IntPtr.Zero, "Invalid native address!");

        _nativePtr = unrealObjectPtr;

        // Logger.LogD("{0} is created, UnrealObjectPtr = 0x{1:x}", GetType().Name, NativePtr);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether this instance is binding to unreal.
    /// </summary>
    /// <value><c>true</c> if this instance is binding to unreal; otherwise, <c>false</c>.</value>
    public bool IsBindingToUnreal => _nativePtr != IntPtr.Zero;

    #endregion

    #region Methods

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <returns>System.Nullable&lt;System.String&gt;.</returns>
    public string? GetName()
    {
        return _nativePtr == IntPtr.Zero ? null : ObjectInteropUtils.GetUnrealObjectName(_nativePtr);
    }

    /// <summary>
    /// Gets the name of the path.
    /// </summary>
    /// <returns>System.Nullable&lt;System.String&gt;.</returns>
    public string? GetPathName()
    {
        return _nativePtr == IntPtr.Zero ? null : ObjectInteropUtils.GetUnrealObjectPathName(_nativePtr);
    }

    /// <summary>
    /// Gets the outer.
    /// </summary>
    /// <returns>System.Nullable&lt;UObject&gt;.</returns>
    public UObject? GetOuter()
    {
        return _nativePtr == IntPtr.Zero ? null : ObjectInteropUtils.GetUnrealObjectOuter(_nativePtr);
    }

    /// <summary>
    /// Gets the class.
    /// </summary>
    /// <returns>System.Nullable&lt;UClass&gt;.</returns>
    public UClass? GetClass()
    {
        if (_classCache != null)
        {
            return _classCache;
        }

        if (_nativePtr == IntPtr.Zero)
        {
            return null;
        }

        var classPtr = ClassInteropUtils.GetClassPointerOfUnrealObject(_nativePtr);

        Logger.Assert(classPtr != IntPtr.Zero);

        _classCache = new UClass(classPtr);

        return _classCache;
    }

    /// <summary>
    /// Statics the class.
    /// </summary>
    /// <returns>UClass.</returns>
    public static UClass StaticClass()
    {
        MetaInteropUtils.LoadClassIfNeed(ref _objectClass, "/Script/CoreUObject.Object");

        return _objectClass;
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
    /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is UObject u && u._nativePtr == _nativePtr;
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return _nativePtr.GetHashCode();
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string? ToString()
    {
        var name = GetName();

        //return $"{(Name ?? "Null")}[{GetType().Name}](0x{NativePtr:x})";
        return $"{name ?? "null"}";
    }

    /// <summary>
    /// Disconnects from native.
    /// </summary>
    public void DisconnectFromNative()
    {
        // Logger.LogD("DisconnectFromNative 0x{0:x}", NativePtr);

        _nativePtr = IntPtr.Zero;
    }

    /// <summary>
    /// Gets the native PTR.
    /// </summary>
    /// <returns>IntPtr.</returns>
    public IntPtr GetNativePtr()
    {
        return _nativePtr;
    }

    /// <summary>
    /// Gets the native PTR checked.
    /// </summary>
    /// <returns>IntPtr.</returns>
    public IntPtr GetNativePtrChecked()
    {
        Logger.Ensure<AccessViolationException>(IsBindingToUnreal,
            "Invalid access: install of {0} is not binding to unreal object.", GetType().FullName!);

        return _nativePtr;
    }

    /// <summary>
    /// Adds to root.
    /// </summary>
    public void AddToRoot()
    {
        ObjectInteropUtils.AddUnrealObjectToRoot(this);
    }

    /// <summary>
    /// Removes from root.
    /// </summary>
    public void RemoveFromRoot()
    {
        ObjectInteropUtils.RemoveUnrealObjectFromRoot(this);
    }

    /// <summary>
    /// Determines whether this instance is rooted.
    /// </summary>
    /// <returns><c>true</c> if this instance is rooted; otherwise, <c>false</c>.</returns>
    public bool IsRooted()
    {
        return ObjectInteropUtils.IsUnrealObjectRooted(this);
    }

    /// <summary>
    /// Returns true if UObject* is valid.
    /// equal to ::IsValid in C++
    /// </summary>
    /// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
    public bool IsUnrealObjectValid()
    {
        return ObjectInteropUtils.IsUnrealObjectValid(this);
    }

    #endregion

    #region OnObjectConstrct

    /// <summary>
    /// Invoked from native
    /// Only valid for C# Script class
    /// </summary>
    /// <param name="nativeObjectInitializerPtr">The native object initializer PTR.</param>
    // ReSharper disable once UnusedMember.Local
    private void BeforeObjectConstructorInternal(IntPtr nativeObjectInitializerPtr)
    {
        BeforeObjectConstructor(new FObjectInitializer(nativeObjectInitializerPtr));
    }

    /// <summary>
    /// Invoked from native
    /// Only valid for C# Script class
    /// </summary>
    /// <param name="objectInitializer">The object initializer.</param>
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once UnusedParameter.Global
    protected virtual void BeforeObjectConstructor(FObjectInitializer objectInitializer)
    {
        //Logger.Log("C# {0} BeforeObjectConstructor", GetType().FullName!);
    }

    /// <summary>
    /// Invoked from native
    /// Only valid for C# Script class
    /// </summary>
    protected virtual void PostObjectConstructor()
    {
        //Logger.Log("C# {0} PostObjectConstructor", GetType().FullName!);
    }

    #endregion

    #region Create Subobject

    /// <summary>
    /// Creates the default subobject.
    /// </summary>
    /// <param name="subobjectName">Name of the subobject.</param>
    /// <param name="returnType">Type of the return.</param>
    /// <param name="classToCreateByDefault">The class to create by default.</param>
    /// <param name="isRequired">if set to <c>true</c> [is required].</param>
    /// <param name="isTransient">if set to <c>true</c> [is transient].</param>
    /// <returns>System.Nullable&lt;UObject&gt;.</returns>
    public UObject? CreateDefaultSubobject(string subobjectName, UClass? returnType, UClass? classToCreateByDefault,
        bool isRequired, bool isTransient)
    {
        return ObjectInteropUtils.CreateDefaultSubobject(
            GetNativePtr(),
            subobjectName,
            returnType != null ? returnType.GetNativePtr() : IntPtr.Zero,
            classToCreateByDefault != null ? classToCreateByDefault.GetNativePtr() : IntPtr.Zero,
            isRequired,
            isTransient
        );
    }

    /// <summary>
    /// Creates the default subobject.
    /// </summary>
    /// <typeparam name="TReturnType">The type of the t return type.</typeparam>
    /// <param name="subobjectName">Name of the subobject.</param>
    /// <param name="isTransient">if set to <c>true</c> [is transient].</param>
    /// <returns>System.Nullable&lt;TReturnType&gt;.</returns>
    public TReturnType? CreateDefaultSubobject<TReturnType>(string subobjectName, bool isTransient = false)
        where TReturnType : UObject
    {
        var returnType = UClass.GetClassOf<TReturnType>();

        return CreateDefaultSubobject(subobjectName, returnType, returnType, true, isTransient) as TReturnType;
    }

    /// <summary>
    /// Creates the default subobject.
    /// </summary>
    /// <typeparam name="TReturnType">The type of the t return type.</typeparam>
    /// <typeparam name="TClassToConstructByDefault">The type of the t class to construct by default.</typeparam>
    /// <param name="subobjectName">Name of the subobject.</param>
    /// <param name="isTransient">if set to <c>true</c> [is transient].</param>
    /// <returns>System.Nullable&lt;TReturnType&gt;.</returns>
    public TReturnType? CreateDefaultSubobject<TReturnType, TClassToConstructByDefault>(string subobjectName,
        bool isTransient = false)
        where TReturnType : UObject
        where TClassToConstructByDefault : UObject
    {
        var returnType = UClass.GetClassOf<TReturnType>();
        var classToCreateByDefault = UClass.GetClassOf<TClassToConstructByDefault>();

        return CreateDefaultSubobject(subobjectName, returnType, classToCreateByDefault, true, isTransient) as
            TReturnType;
    }

    /// <summary>
    /// Creates the optional default subobject.
    /// </summary>
    /// <typeparam name="TReturnType">The type of the t return type.</typeparam>
    /// <param name="subobjectName">Name of the subobject.</param>
    /// <param name="isTransient">if set to <c>true</c> [is transient].</param>
    /// <returns>System.Nullable&lt;TReturnType&gt;.</returns>
    public TReturnType? CreateOptionalDefaultSubobject<TReturnType>(string subobjectName, bool isTransient = false)
        where TReturnType : UObject
    {
        var returnType = UClass.GetClassOf<TReturnType>();

        return CreateDefaultSubobject(subobjectName, returnType, returnType, false, isTransient) as TReturnType;
    }

    /// <summary>
    /// Creates the optional default subobject.
    /// </summary>
    /// <typeparam name="TReturnType">The type of the t return type.</typeparam>
    /// <typeparam name="TClassToConstructByDefault">The type of the t class to construct by default.</typeparam>
    /// <param name="subobjectName">Name of the subobject.</param>
    /// <param name="isTransient">if set to <c>true</c> [is transient].</param>
    /// <returns>System.Nullable&lt;TReturnType&gt;.</returns>
    public TReturnType? CreateOptionalDefaultSubobject<TReturnType, TClassToConstructByDefault>(string subobjectName,
        bool isTransient = false)
        where TReturnType : UObject
        where TClassToConstructByDefault : UObject
    {
        var returnType = UClass.GetClassOf<TReturnType>();
        var classToCreateByDefault = UClass.GetClassOf<TClassToConstructByDefault>();

        return CreateDefaultSubobject(subobjectName, returnType, classToCreateByDefault, false, isTransient) as
            TReturnType;
    }

    #endregion

    #region Object Factory

    /// <summary>
    /// Creates new object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>System.Nullable&lt;T&gt;.</returns>
    public static T? NewObject<T>() where T : UObject
    {
        var value = ObjectInteropUtils.NewUnrealObject(null, UClass.GetClassOf<T>(), FName.NAME_None,
            EObjectFlags.NoFlags, null, false);

        return value as T;
    }

    /// <summary>
    /// Creates new object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="outer">The outer.</param>
    /// <returns>System.Nullable&lt;T&gt;.</returns>
    public static T? NewObject<T>(UObject? outer) where T : UObject
    {
        var value = ObjectInteropUtils.NewUnrealObject(outer, UClass.GetClassOf<T>(), FName.NAME_None,
            EObjectFlags.NoFlags, null, false);

        return value as T;
    }

    /// <summary>
    /// Creates new object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="outer">The outer.</param>
    /// <param name="name">The name.</param>
    /// <param name="flags">The flags.</param>
    /// <param name="template">The template.</param>
    /// <param name="copyTransientsFromClassDefaults">if set to <c>true</c> [copy transients from class defaults].</param>
    /// <returns>System.Nullable&lt;T&gt;.</returns>
    public static T? NewObject<T>(UObject? outer, FName name, EObjectFlags flags = EObjectFlags.NoFlags,
        UObject? template = null, bool copyTransientsFromClassDefaults = false) where T : UObject
    {
        var value = ObjectInteropUtils.NewUnrealObject(outer, UClass.GetClassOf<T>(), name, flags, template,
            copyTransientsFromClassDefaults);

        return value as T;
    }

    /// <summary>
    /// Creates new object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="outer">The outer.</param>
    /// <param name="class">The class.</param>
    /// <param name="name">The name.</param>
    /// <param name="flags">The flags.</param>
    /// <param name="template">The template.</param>
    /// <param name="copyTransientsFromClassDefaults">if set to <c>true</c> [copy transients from class defaults].</param>
    /// <returns>System.Nullable&lt;T&gt;.</returns>
    public static T? NewObject<T>(UObject? outer, UClass @class, FName name, EObjectFlags flags = EObjectFlags.NoFlags,
        UObject? template = null, bool copyTransientsFromClassDefaults = false) where T : UObject
    {
        var value = ObjectInteropUtils.NewUnrealObject(outer, @class, name, flags, template,
            copyTransientsFromClassDefaults);

        return value as T;
    }

    /// <summary>
    /// Duplicates the object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sourceObject">The source object.</param>
    /// <param name="outer">The outer.</param>
    /// <param name="name">The name.</param>
    /// <returns>System.Nullable&lt;T&gt;.</returns>
    public static T? DuplicateObject<T>(UObject sourceObject, UObject? outer, FName name) where T : UObject
    {
        var value = ObjectInteropUtils.DuplicateUnrealObject(UClass.GetClassOf<T>(), sourceObject, outer, name);

        return value as T;
    }

    /// <summary>
    /// Gets the transient package.
    /// </summary>
    /// <returns>System.Nullable&lt;UObject&gt;.</returns>
    public static UObject? GetTransientPackage()
    {
        return ObjectInteropUtils.GetUnrealTransientPackage();
    }

    /// <summary>
    /// Finds the object fast.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="outer">The outer.</param>
    /// <param name="name">The name.</param>
    /// <param name="exactClass">if set to <c>true</c> [exact class].</param>
    /// <param name="exclusiveFlags">The exclusive flags.</param>
    /// <returns>System.Nullable&lt;T&gt;.</returns>
    public static T? FindObjectFast<T>(UObject? outer, FName name, bool exactClass = false,
        EObjectFlags exclusiveFlags = EObjectFlags.NoFlags) where T : UObject
    {
        var result = ObjectInteropUtils.FindObjectFast(UClass.GetClassOf<T>(), outer, name, exactClass, exclusiveFlags);

        return result as T;
    }

    /// <summary>
    /// Finds the object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="outer">The outer.</param>
    /// <param name="name">The name.</param>
    /// <param name="exactClass">if set to <c>true</c> [exact class].</param>
    /// <returns>System.Nullable&lt;T&gt;.</returns>
    public static T? FindObject<T>(UObject? outer, string name, bool exactClass = false) where T : UObject
    {
        var result = ObjectInteropUtils.FindObject(UClass.GetClassOf<T>(), outer, name, exactClass);

        return result as T;
    }

    /// <summary>
    /// Finds the object checked.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="outer">The outer.</param>
    /// <param name="name">The name.</param>
    /// <param name="exactClass">if set to <c>true</c> [exact class].</param>
    /// <returns>System.Nullable&lt;T&gt;.</returns>
    public static T? FindObjectChecked<T>(UObject? outer, string name, bool exactClass = false) where T : UObject
    {
        var result = ObjectInteropUtils.FindObjectChecked(UClass.GetClassOf<T>(), outer, name, exactClass);

        return result as T;
    }

    /// <summary>
    /// Finds the object safe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="outer">The outer.</param>
    /// <param name="name">The name.</param>
    /// <param name="exactClass">if set to <c>true</c> [exact class].</param>
    /// <returns>System.Nullable&lt;T&gt;.</returns>
    public static T? FindObjectSafe<T>(UObject? outer, string name, bool exactClass = false) where T : UObject
    {
        var result = ObjectInteropUtils.FindObjectSafe(UClass.GetClassOf<T>(), outer, name, exactClass);

        return result as T;
    }

    /// <summary>
    /// Loads the object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="outer">The outer.</param>
    /// <param name="name">The name.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>System.Nullable&lt;T&gt;.</returns>
    public static T? LoadObject<T>(UObject? outer, string name, string? fileName) where T : UObject
    {
        var result = ObjectInteropUtils.LoadObject(UClass.GetClassOf<T>(), outer, name, fileName);

        return result as T;
    }

    #endregion
}

#region Object Flags

/// <summary>
/// Flags describing an object instance
/// </summary>    
public enum EObjectFlags : uint
{
    /// <summary>
    /// No flags, used to avoid a cast
    /// </summary>
    NoFlags = 0x00000000,

    // This first group of flags mostly has to do with what kind of object it is. Other than transient, these are the persistent object flags.
    // The garbage collector also tends to look at these.

    /// <summary>
    /// Object is visible outside its package.
    /// </summary>
    Public = 0x00000001,

    /// <summary>
    /// Keep object around for editing even if unreferenced.
    /// </summary>
    Standalone = 0x00000002,

    /// <summary>
    /// Object (UField) will be marked as native on construction (DO NOT USE THIS FLAG in HasAnyFlags() etc.)
    /// </summary>
    MarkAsNative = 0x00000004,

    /// <summary>
    /// Object is transactional.
    /// </summary>
    Transactional = 0x00000008,

    /// <summary>
    /// This object is its class's default object
    /// </summary>
    ClassDefaultObject = 0x00000010,

    /// <summary>
    /// This object is a template for another object - treat like a class default object
    /// </summary>
    ArchetypeObject = 0x00000020,

    /// <summary>
    /// Don't save object.
    /// </summary>
    Transient = 0x00000040,

    // This group of flags is primarily concerned with garbage collection.
    /// <summary>
    /// Object will be marked as root set on construction and not be garbage collected, even if unreferenced (DO NOT USE THIS FLAG in HasAnyFlags() etc.)
    /// </summary>
    MarkAsRootSet = 0x00000080,

    /// <summary>
    /// This is a temp user flag for various utilities that need to use the garbage collector. The garbage collector itself does not interpret it.
    /// </summary>
    TagGarbageTemp = 0x00000100,

    // The group of flags tracks the stages of the lifetime of an uobject
    /// <summary>
    /// This object has not completed its initialization process. Cleared when ~FObjectInitializer completes
    /// </summary>
    NeedInitialization = 0x00000200,

    /// <summary>
    /// During load, indicates object needs loading.
    /// </summary>
    NeedLoad = 0x00000400,

    /// <summary>
    /// Keep this object during garbage collection because it's still being used by the cooker
    /// </summary>
    KeepForCooker = 0x00000800,

    /// <summary>
    /// Object needs to be post loaded.
    /// </summary>
    NeedPostLoad = 0x00001000,

    /// <summary>
    /// During load, indicates that the object still needs to instance subobjects and fixup serialized component references
    /// </summary>
    NeedPostLoadSubobjects = 0x00002000,

    /// <summary>
    /// Object has been consigned to oblivion due to its owner package being reloaded, and a newer version currently exists
    /// </summary>
    NewerVersionExists = 0x00004000,

    /// <summary>
    /// BeginDestroy has been called on the object.
    /// </summary>
    BeginDestroyed = 0x00008000,

    /// <summary>
    /// FinishDestroy has been called on the object.
    /// </summary>
    FinishDestroyed = 0x00010000,

    // Misc. Flags
    /// <summary>
    /// Flagged on UObjects that are used to create UClasses (e.g. Blueprints) while they are regenerating their UClass on load (See FLinkerLoad::CreateExport()), as well as UClass objects in the midst of being created
    /// </summary>
    BeingRegenerated = 0x00020000,

    /// <summary>
    /// Flagged on subobjects that are defaults
    /// </summary>
    DefaultSubObject = 0x00040000,

    /// <summary>
    /// Flagged on UObjects that were loaded
    /// </summary>
    WasLoaded = 0x00080000,

    /// <summary>
    /// Do not export object to text form (e.g. copy/paste). Generally used for sub-objects that can be regenerated from data in their parent object.
    /// </summary>
    TextExportTransient = 0x00100000,

    /// <summary>
    /// Object has been completely serialized by linker load at least once. DO NOT USE THIS FLAG, It should be replaced with WasLoaded.
    /// </summary>
    LoadCompleted = 0x00200000,

    /// <summary>
    /// Archetype of the object can be in its super class
    /// </summary>
    InheritableComponentTemplate = 0x00400000,

    /// <summary>
    /// Object should not be included in any type of duplication (copy/paste, binary duplication, etc.)
    /// </summary>
    DuplicateTransient = 0x00800000,

    /// <summary>
    /// References to this object from persistent function frame are handled as strong ones.
    /// </summary>
    StrongRefOnFrame = 0x01000000,

    /// <summary>
    /// Object should not be included for duplication unless it's being duplicated for a PIE session
    /// </summary>
    NonPieDuplicateTransient = 0x02000000,

    /// <summary>
    /// Field Only. Dynamic field - doesn't get constructed during static initialization, can be constructed multiple times  // @todo: BP2CPP_remove
    /// </summary>
    [Obsolete("Dynamic should no longer be used. It is no longer being set by engine code.")]
    Dynamic = 0x04000000,

    /// <summary>
    /// This object was constructed during load and will be loaded shortly
    /// </summary>
    WillBeLoaded = 0x08000000,

    /// <summary>
    /// This object has an external package assigned and should look it up when getting the outermost package
    /// </summary>
    HasExternalPackage = 0x10000000,

    // Garbage and PendingKill are mirrored in EInternalObjectFlags because checking the internal flags is much faster for the Garbage Collector
    // while checking the object flags is much faster outside of it where the Object pointer is already available and most likely cached.
    // PendingKill is mirrored in EInternalObjectFlags because checking the internal flags is much faster for the Garbage Collector
    // while checking the object flags is much faster outside of it where the Object pointer is already available and most likely cached.
    /// <summary>
    /// Objects that are pending destruction (invalid for gameplay but valid objects). This flag is mirrored in EInternalObjectFlags as PendingKill for performance
    /// </summary>
    [Obsolete(
        "PendingKill should not be used directly. Make sure references to objects are released using one of the existing engine callbacks or use weak object pointers.")]
    PendingKill = 0x20000000,

    /// <summary>
    /// Garbage from logical point of view and should not be referenced. This flag is mirrored in EInternalObjectFlags as Garbage for performance
    /// </summary>
    [Obsolete("Garbage should not be used directly. Use MarkAsGarbage and ClearGarbage instead.")]
    Garbage = 0x40000000,

    /// <summary>
    /// Allocated from a ref-counted page shared with other UObjects
    /// </summary>
    AllocatedInSharedPage = 0x80000000
}

#endregion

#region Property Cache

/// <summary>
/// Struct TObjectPropertyCache
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once InconsistentNaming
public struct TObjectPropertyCache<T>
    where T : class, IUObjectInterface
{
    /// <summary>
    /// The value
    /// </summary>
    private T? _cachedValue;

    /// <summary>
    /// The address
    /// </summary>
    private IntPtr _cachedPointer;

    /// <summary>
    /// The constructor
    /// </summary>
    private static readonly IntelligentCacheUtils.ConstructDelegateType<T> Constructor = address =>
        InteropUtils.GetObject<T>(address, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="TObjectPropertyCache{T}" /> struct.
    /// </summary>
    public TObjectPropertyCache()
    {
    }

    /// <summary>
    /// Gets the object.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>System.Nullable&lt;T&gt;.</returns>
    public T? GetObject(IntPtr address, int offset)
    {
        Logger.Assert(address != IntPtr.Zero);

        return IntelligentCacheUtils.GetPointerBasedValue(ref _cachedPointer, ref _cachedValue, address, offset,
            Constructor);
    }
}

#endregion

#region Scoped Prevent Unreal Object Garbage Collection

/// <summary>
/// Class ScopedPreventUnrealObjectGC.
/// Implements the <see cref="System.IDisposable" />
/// </summary>
/// <seealso cref="System.IDisposable" />
public sealed class ScopedPreventUnrealObjectGc : IDisposable
{
    /// <summary>
    /// The object
    /// </summary>
    public readonly UObject Object;

    private readonly bool _isAddToRootByMe;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScopedPreventUnrealObjectGc"/> class.
    /// </summary>
    /// <param name="object">The object.</param>
    public ScopedPreventUnrealObjectGc(UObject @object)
    {
        Object = @object;

        if (Object.IsRooted())
        {
            return;
        }

        _isAddToRootByMe = true;
        Object.AddToRoot();
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        if (_isAddToRootByMe)
        {
            Object.RemoveFromRoot();
        }
    }
}

#endregion