# Known Issues

## Running In UnrealEditor on MacOS
The .NET Mono runtime does not support reentrancy, that is, repeatedly initializing and releasing Domains during the life cycle of a program, nor does it support the creation of multiple Domains. Therefore, in order to solve the problem of repeatedly playing games under the editor, I will not Stop loading and releasing the corresponding dynamic library, which has been tested under Windows. However, in addition to libcoreclr.so under macos, there are several other so that are used to implement some key C# dlls, and these dlls are also non-reentrant, which will cause assert to occur during secondary playback under the editor. crashes, so that's an issue that needs to be addressed.  

## JIT is not compatible with Debugger
After initializing the debugger under Windows, if the JIT is not closed, a crash will occur. This may be a problem with Mono's underlying layer. I've tagged the question in the code.  
```C++
// in debug support mode, we need force use interop mode
// because jit mode will cause crash.
// comment this code, you will cause the crash
mono_jit_set_aot_mode(MONO_AOT_MODE_INTERP_ONLY);

mono_jit_parse_options(sizeof(options)/sizeof(options[0]), (char**)options);
mono_debug_init(MONO_DEBUG_FORMAT_MONO);
```
When debugging support is not enabled, JIT can be used normally.  