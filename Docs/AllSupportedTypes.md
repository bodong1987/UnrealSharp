# Supported Types
**Please note that the type restrictions here only apply to those involved in generating Unreal classes and structure properties, UFunction parameters and return values, etc. For ordinary C# code, there are no restrictions on types.**  

| Type       | UPROPERTY Of USTRUCT| UPROPERTY of UCLASS | UFUNCTION PARAMS| UFUNCTION Return Type| FastAccess          |
| :----      | :------:            | :------:            | :------:        | :------:             |:------:             |
| bool       | &#x2714;            | &#x2714;            | &#x2714;        | &#x2714;             | &#x2714;            |
| char       | &#x2716;            | &#x2716;            | &#x2716;        | &#x2716;             | &#x2716;            |
| sbyte      | &#x2716;            | &#x2716;            | &#x2716;        | &#x2716;             | &#x2716;            |
| byte       | &#x2714;            | &#x2714;            | &#x2714;        | &#x2714;             | &#x2714;            |
| short      | &#x2716;            | &#x2716;            | &#x2716;        | &#x2716;             | &#x2716;            |
| ushort     | &#x2716;            | &#x2716;            | &#x2716;        | &#x2716;             | &#x2716;            |
| int        | &#x2714;            | &#x2714;            | &#x2714;        | &#x2714;             | &#x2714;            |
| uint       | &#x2716;            | &#x2716;            | &#x2716;        | &#x2716;             | &#x2716;            |
| Int64      | &#x2714;            | &#x2714;            | &#x2714;        | &#x2714;             | &#x2714;            |
| UInt64     | &#x2716;            | &#x2716;            | &#x2716;        | &#x2716;             | &#x2716;            |
| float      | &#x2714;            | &#x2714;            | &#x2714;        | &#x2714;             | &#x2714;            |
| double     | &#x2714;            | &#x2714;            | &#x2714;        | &#x2714;             | &#x2714;            |
| string     | &#x2714;            | &#x2714;            | &#x2714;        | &#x2714;             | partial             |
| FName      | &#x2714;            | &#x2714;            | &#x2714;        | &#x2714;             | &#x2714;            |
| FText      | &#x2714;            | &#x2714;            | &#x2714;        | &#x2714;             | &#x2716;            |
| Enums      | &#x2714;            | &#x2714;            | &#x2714;        | &#x2714;             | &#x2714;            |
| UObject    | &#x2714;            | &#x2714;            | &#x2714;        | &#x2714;             | &#x2714;            |
| UClass     | &#x2716;            | &#x2716;            | &#x2716;        | &#x2716;             | &#x2716;            |
| Structures | &#x2714;            | &#x2714;            | &#x2714;        | &#x2714;             | conditional         |
| `TSubClassOf<T>`    | &#x2714;            | &#x2714;            | &#x2714;        | &#x2714;      | &#x2714;            |
| `TSoftObjectPtr<T>` | &#x2716;            | &#x2714;            | &#x2714;        | &#x2716;      | &#x2716;            |
| `TSoftClassPtr<T>` | &#x2716;            | &#x2714;            | &#x2714;        | &#x2716;      | &#x2716;            |
| `TArray<T>` | &#x2714;            | &#x2714;            | &#x2714;        | &#x2714;      | &#x2716;            |
| `TSet<T>` | &#x2714;            | &#x2714;            | &#x2714;        | &#x2714;      | &#x2716;            |
| `TMap<TKey,TValue>` | &#x2714;            | &#x2714;            | &#x2714;        | &#x2714;      | &#x2716;            |
| `TDelegate<Signature>`    | &#x2716;            | &#x2716;            | &#x2716;        | &#x2716;             | &#x2716;            |
| `TMulticastDelegate<Signature>`    | &#x2716;            | &#x2714;            | &#x2714;        | &#x2716;             | &#x2716;            |
| Other C# types       | &#x2716;            | &#x2716;            | &#x2716;        | &#x2716;             | &#x2716;            |

* &#x2714; : Fully Support
* &#x2716; : Not Support, Not supported means not allowed in C#
Use this type in this form.  
* **conditional** : Conditional support, For example, among structures, only structures that meet the conditions can use fast access, that is, the memory size, layout, attribute type and offset of the C++ structure and the C# structure are exactly the same, and the attribute types also support fast access.
* **partial** : Partially supported,Strings support fast parameter passing, but when the passing method is by reference, fast access is not supported.  

**When there is a parameter type in the function that does not support fast access or the return value does not support fast access, the function call in normal mode will be used, which is slower than fast access.**

