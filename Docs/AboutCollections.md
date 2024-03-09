# About Unreal Collections in C#
Three Unreal containers are supported, `TArray<T>`, `TSet<T>`, and `TMap<TKey,TValue>`. For performance reasons, they are only Views of the Unreal container on the C# side. In other words, these C# side classes only save the pointers of these containers on the C++ side, and all your operations on these containers will be immediately forwarded to the C++ side for execution.  
```C#
    /// <summary>
    /// Class TArray.
    /// Please note that this container is just a View.
    /// Saving it outside the binding class is dangerous.
    /// If you want to do such an operation for any purpose, you need to understand the principle behind it.
    /// Implements the <see cref="IList{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="IList{T}" />
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(TArrayDebugView<>))]
    public class TArray<T> : IList<T>, IUnrealCollectionDataView<T>
```

```C#
    /// <summary>
    /// Class TSet.
    /// Please note that this container is just a View.
    /// Saving it outside the binding class is dangerous.
    /// If you want to do such an operation for any purpose, you need to understand the principle behind it.
    /// Implements the <see cref="ISet{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="ISet{T}" />
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(TSetDebugView<>))]
    public class TSet<T> : ISet<T>, IUnrealCollectionDataView<T>
```

```C#
    /// <summary>
    /// Class TMap.
    /// Please note that this container is just a View.
    /// Saving it outside the binding class is dangerous.
    /// If you want to do such an operation for any purpose, you need to understand the principle behind it.
    /// </summary>
    /// <typeparam name="TKey">The type of the t key.</typeparam>
    /// <typeparam name="TValue">The type of the t value.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(TMapDebugView<,>))]
    public class TMap<TKey, TValue> : IDictionary<TKey, TValue>, IUnrealCollectionDataView<KeyValuePair<TKey, TValue>>
        where TKey : notnull
```
Therefore, passing TArray out and saving it on the C# side is dangerous behavior, because you may not be notified when UnrealObject is released, then the pointers saved inside these containers will become invalid, and accessing the data at this time will cause a crash.  
  
**So when you want to actually save the data on the C# side, you need to actively call the Retain function, which will return a safe IEnumerable to you.**

In addition, classes such as TArray, TSet, and TMap are all used internally. You usually cannot create them directly by yourself. I recommend that you use `IList<T>`, `ISet<T>`, and `IDictionary<TKey, TValue>`. Especially in UFunction, the tool will only declare these types for you, so that your UFunction will be able to accept data from different sources when calling, such as `TArray<T>`, `TSet<T>`, `TMap<TKey,TValue>` from the engine, and also accept `List<T>`, `Set<T>`, `Dictionary<TKey,TValue>` from C#.  

example:  
```C#
    [UFUNCTION(Category = "UnrealSharp_CSharp")]
    public static IList<string?> CSharpGetStringArrayAndReturnByRef(IList<string?> a, IList<string?> b, ref IList<string?> outA, ref IList<string?> outB)
    {
        /*
        * You need to use retain here to obtain the complete resource ownership of the input parameters, 
        * because the ones that accept outA and outB may come from Unreal or C#. 
        * If they come from C# and are not retained, it may cause a crash caused by the loss of resource references. 
        * Unless you clearly know that neither a nor b are Views from Unreal data.
        */
        outA = a.Retain()!;  
        outB = b.Retain()!;

        List<string?> Result = [.. a, .. b];

        return Result;
    }

    [UFUNCTION(Category = "UnrealSharp_CSharp")]
    public static ISet<FName> CSharpGetNameSetAndReturnByRef(ISet<FName> a, ISet<FName> b, ref ISet<FName> outA, ref ISet<FName> outB)
    {
        /*
        * You need to use retain here to obtain the complete resource ownership of the input parameters, 
        * because the ones that accept outA and outB may come from Unreal or C#. 
        * If they come from C# and are not retained, it may cause a crash caused by the loss of resource references. 
        * Unless you clearly know that neither a nor b are Views from Unreal data.
        */
        outA = a.Retain()!;
        outB = b.Retain()!;

        HashSet<FName> Result = [.. a, .. b];
        
        return Result;
    }

    [UFUNCTION(Category = "UnrealSharp_CSharp")]
    public static IDictionary<Int64, double> CSharpGetInt64DoubleMapAndReturnByRef(IDictionary<Int64, double> a, IDictionary<Int64, double> b, ref IDictionary<Int64, double> outA, ref IDictionary<Int64, double> outB)
    {
        /*
        * You need to use retain here to obtain the complete resource ownership of the input parameters, 
        * because the ones that accept outA and outB may come from Unreal or C#. 
        * If they come from C# and are not retained, it may cause a crash caused by the loss of resource references. 
        * Unless you clearly know that neither a nor b are Views from Unreal data.
        */
        outA = a.Retain()!;
        outB = b.Retain()!;

        Dictionary<Int64, double> Result = new Dictionary<long, double>();

        foreach(var p in a)
        {
            Result.TryAdd(p.Key, p.Value);
        }

        foreach (var p in b)
        {
            Result.TryAdd(p.Key, p.Value);
        }

        return Result;
    }
```