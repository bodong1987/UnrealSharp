# Prepare UnrealSharp ThirdParty
You have two methods to obtain UnrealSharp’s dependency packages:
1. Download the precompiled .NET 8.0 runtime binary package:  
    <a href="https://drive.google.com/file/d/1JX56bX2vNex8-cRUooltcc6CeVy-Deak/view?usp=sharing" target="_blank">From GoogleDrive</a>  
    <a href="https://pan.baidu.com/s/1s22ry5MplzeJXyyTV3NWIw?pwd=t8yq" target="_blank">From BaiduDisk</a>  
2. Build it by yourself  
``Although the design of UnrealSharp does not mandate the use of .NET's underlying runtime type, it currently only implements a Mono-based Runtime, so the mono runtime must be selected when compiling .net. Runtime based on CoreCLR will be provided in subsequent development  
``

## Build
### Related Links
<a href="https://github.com/dotnet/runtime/blob/main/docs/workflow/README.md" target="_blank">.NET runtime build workflow</a>  
<a href="https://github.com/dotnet/runtime/blob/main/docs/workflow/building/mono/README.md" target="_blank">Building Mono</a>  

clone [.NET runtime source code(UnrealSharp fork)](https://github.com/bodong1987/runtime/tree/unrealsharp/8.0)  

To build a complete runtime environment, you need to build both the Mono runtime and libraries.  At the repo root, simply execute:

```bash
./build.sh mono+libs
```
or on Windows,
```cmd
build.cmd mono+libs
```
Note that the debug configuration is the default option. It generates a 'debug' output and that includes asserts, fewer code optimizations, and is easier for debugging. If you want to make performance measurements, or just want tests to execute more quickly, you can also build the 'release' version which does not have these checks by adding the flag `-configuration release` (or `-c release`).  
If you want to compile libraries for other architectures on the current platform, you can use the command line -a:
```bash
./build.sh mono+libs -a arm64 -c release
```
You can use -help to see the full list of supported commands:  
```bash
./build.sh -help
```

## Copy Packages to UnrealSharp
After compilation is completed, you will see the directory artifacts/bin in the runtime source code root directory. There will be two directories in this directory, mono and runtime, which UnrealSharp depends on.  
- runtime
    - artifacts
        - bin
            - mono
                - ios.arm64.Debug
                - ios.arm64.Release
                - osx.arm64.Debug
                - osx.arm64.Release
                - osx.x64.Debug
                - osx.x64.Release
                - windows.x64.Debug
                - ...
            - runtime
                - net8.0-ios-Debug-arm64
                - net8.0-ios-Release-arm64
                - net8.0-osx-Debug-arm64
                - ...  

The specific subfolders depend on your compilation platform and compilation configuration.  
Copy the entire mono and runtime directories to the Plugins/UnrealSharp/ThirdParty directory in the root directory of your Unreal project.  
Your final directory should look like this:  

- Your Unreal Project
    - Plugins
        - UnrealSharp
            - ThirdParty
                - mono
                    - windows.x64.Debug
                    - ...
                - runtime
                    - net8.0-windows-Debug-x64
                    - ...
    - GameScripts
    - Managed




                










