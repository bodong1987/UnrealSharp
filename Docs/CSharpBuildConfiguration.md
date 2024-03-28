# C# Build Configuration
C# projects have compilation configurations. The main purpose is to facilitate the realization of some differences on different platforms, and to support different type definitions in editor mode and game mode. For example, the size of FName in Unreal C++ is different in the editor and the game. Similarly, if you want to exchange data directly between C++ and C#, then the sizes must match.  

The currently supported configurations are:  
* Debug-Windows-Editor  
* Release-Windows-Editor  
* Debug-Windows-Game  
* Release-Windows-Game  
* Debug-Mac-Editor  
* Release-Mac-Editor  
* Debug-Mac-Game  
* Release-Mac-Game  
* Debug-Linux-Editor  
* Release-Linux-Editor  
* Debug-Linux-Game  
* Release-Linux-Game  
* Debug-IOS-Game  
* Release-IOS-Game  
* Debug-Android-Game  
* Release-Android-Game

select your correct build configuration here:  
![VisualStudioBuildConfiguration](./Images/buildconfiguration.png)

**Please be sure to select the correct and matching configuration when compiling, otherwise an error will be directly triggered when initializing UnrealSharp.  
Debug and Release can be mixed to facilitate debugging, while others need to match exactly.**  
**Before you package your game each time, be sure to select the correct configuration and recompile the C# code. For example, if you want to package a Windows game, you can choose Debug-Windows-Game or Release-Windows-Game, but Debug-Windows-Editor cannot.**


