# How to reset UnrealSharp
When you encounter some incomprehensible problems when using UnrealSharp, you may suspect that there is a problem with the generation; or you encounter some strange problems that cause the correct C# assets to not be automatically generated. At this time, you may consider resetting Check out UnrealSharp. This will delete all automatically generated files.  
The method is very simple. There is a `reset-unrealsharp.py` file in the project root directory. You can use python to execute this file. You can also directly double-click `reset-unrealsharp.bat` to execute. Of course, you need to install python3 correctly locally.  
```cmd
python ./reset-unrealsharp.py
```
After completing the reset, you need to do the following to complete the subsequent steps: 
1. Recompile C++, start Unreal Editor, generate Unreal type database, and exit Unreal Editor. 
2. Open UnrealSharp.sln, select the correct configuration and recompile the entire solution. 
3. If UFunction quick call is enabled, you need to compile C++ again; if this function is not enabled, it is not necessary. 
4. Open UnrealEditor and automatically re-import the C# assets. The reset is now complete.


