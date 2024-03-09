# Add UnrealSharp Dependency

**If you have not enabled fast invocation of UFunction (USharpBindingGenSettings::bEnableFastFunctionInvoke), then this step can be ignored.**  

Add a dependency on UnrealSharp in Build.cs of your game project.  
for example(DemoGame.Build.cs):   

```C#
using UnrealBuildTool;

public class DemoGame : ModuleRules
{
	public DemoGame(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicDependencyModuleNames.AddRange(new string[] 
		{ 
			"Core", 
			"CoreUObject", 
			"Engine",
			"InputCore", 
			"EnhancedInput", 

			// add unreal sharp
			"UnrealSharp", 

			// If you don’t need the test code in UnrealSharpTests, you don’t need to add this.
			"UnrealSharpTests"
		});

		// allow header search in module root.
		// If you enable quick access to C++ interactive functions[Default On],
		// this must be added, because this will ensure that the automatically generated interactive functions can correctly find the header file.
		PublicIncludePaths.Add(ModuleDirectory);
	}
}

```