# UnrealSharp
UnrealSharp is a plug-in developed for UnrealEngine 5. Through this plug-in, you can use pure C# to develop UnrealEngine 5 projects.  

## Main Features
* Support C# `Hot Reloading` in non-playing state    
* Supports `.NET 6.0`~`.NET 8.0` [default is `.NET 8.0`], supports `C#12  `
* Supports creating new `Unreal classes`, `Unreal structures`, `Unreal enumerations`, etc. in C#  
* Supports creating new `Unreal properties`, `Unreal functions`, and `Unreal multicast delegates` for C# class.  
* Supports C# classes to `inherit Unreal C++ classes`, which means you can implement your own UObject, UActorComponent, and AActor subclasses in C#   
* Supports `overriding C++` Event functions in C#   
* Support access to `all` C# classes, structures, enumerations, methods, delegates, etc. from Unreal Blueprints  
* Supports Unreal Blueprint classes `inheriting C# classes` and `overriding all C# UFunctions` in blueprints  
* Support `debugging` C# code through `Visual Studio` or `Rider`, and support simultaneous debugging of C++ and C#  
* When the C# code you write does not comply with Unreal Sharp's specifications, you will get a compilation error during the `compilation phase`. At the same time, UnrealSharp will tell you the error type of the `error code`, the code `file` and the `line number`. You can jump directly with a double-click. This No different than a normal C# compilation error.  
* Supports automatically generating corresponding C# binding code for Unreal C++ or Unreal Blueprint [optional]. As long as Unreal's classes, structures, enumerations, functions, etc. can be accessed in blueprints, almost all of them can be accessed in C#.  
* Most C++ APIs are automatically generated as versions based on C# Function Pointers to improve calling speed.  
* This interactive function framework based on C# Function Pointers is public. You can register a new C++ API with the framework if necessary.    
* Automatically align Unreal and C# memory management and object lifetimes so you almost never have to handle anything yourself.  
* The style of C# binding code remains the same as Unreal C++, and there is almost no difference between calling these C++ interfaces and in C++.
* `AOT friendly`, support for AOT has been considered from the initial design.  
* Pure plug-in design, you don’t need to modify any engine source code  
* In addition to the three special projects reserved by the framework, you can add any number of C# projects, and it also supports implementing corresponding Unreal types in these projects.  
* ......  


Visit the project homepage for more information: https://github.com/bodong1987/UnrealSharp  
