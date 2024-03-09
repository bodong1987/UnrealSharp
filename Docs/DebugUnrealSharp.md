# Debug UnrealSharp's C# codes
It has been verified that it supports debugging of Visual Studio and Rider, which is achieved through the debugging channel of the Unity engine. UnrealSharp will try to simulate itself as a UnityPlayer when running, so that you can initiate a call to UnrealSharp through Unity's debugging tool.  
  
**The C# runtime is only initialized when playing, so you can only debug C# code when you play the game in the editor.**

## Debug In Visual Studio 
* Please make sure you choose to install Unity game development support when installing Visual Studio.  
* Open Unreal Editor and open your level, play this map.  
* Open UnrealSharp.sln in Visual Studio, and use Debug>Attach to Unity Process  

![AttachToUnity](./Images/AttachToUnity.png)
* Select Unreal Editor in next dialog.  
![Select Unreal Editor](./Images/SelectUnrealEditorWhenDebugger.png)
* If you connect the debugger successfully, you can set breakpoints in the C# code to debug:  
![DebugInVisualStuido](./Images/DebugInVisualStudio.png)

All debugging operations supported by Unity are supported here.

## Debug In Rider
* Open Unreal Editor, and Change UnrealSharpSettings, enable RiderDebuggerSupport[Default OFF].  
![EnableRiderDebuggerSupport](./Images/EnableRiderDebuggerSupport.png)
* Open UnrealSharp.sln in Rider, Rebuild with correct build configuration, start play in Unreal Editor and use Run > Attach to Unity Process:  
![AttachToUnityInRider](./Images/AttachToUnityInRider.png)  
* For first time use, please add local UnrealSharp debug configuration:  
![AddUnrealSharpConfigurationInRider](./Images/AddConfigInRider.png)  

Click Add Player Address Manually..., enter any name in the pop-up dialog box, and enter: localhost for Host, which is the local machine.  
The port is the port you configured in the UnrealSharp configuration, the default is 57000.  
* After the configuration is completed, double-click the configuration or click OK to open the connection to UnrealSharp.  
![StartDebugInRider](./Images/SelectTargetInRider.png)  

After the connection is successful, you will see the corresponding log output and you can debug in Rider:  

![DebugInRider](./Images/DebugInRider.png)

## Debugging Startup Code
If you use the regular Attach method, you may not be able to debug UnrealSharp's early running code. At this time, you need to turn on the option to wait for the debugger in the settings.   
![WaitDebuggerConfiguration](./Images/WaitDebuggerConfiguration.png)
After this option is enabled, as long as you start playing the game, it will be initialized when the C# runtime is initialized. Pause and wait for the debugger to connect. At this time, you can debug the C# code that started very early.  
![DebugMainEntry](./Images/DebugStartupCode.png)
As shown in the figure, we can even debug UnrealSharp's startup code, which is the earliest C# code triggered from C++.  

## Debug In dnspy Without Source Codes
This feature will comming soon  








