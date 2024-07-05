# How to work without UnrealSharp.Toolkit
I encapsulated the C# and C++ code generation tool chain into UnrealSharp.Toolkit as a NUGET package. The main purpose is to facilitate integration. You can also compile the tool chain yourself without using UnrealSharp.Toolkit and modify the relevant configuration into the csproj file.
* First you need copy 'Tools' directory to your project root along with GameScripts，and then open the UnrealSharp.Programs.sln project in the Tools directory, which is the source code of the tools and UnrealSharp.Utils. Compile it and you will get the code generator.
* You need to remove UnrealSharp.Toolkit's NUGET package dependency from the project.
* Then you need to refer to the csproj configuration below to modify your local csproj so that you can use your locally compiled tool chain.

# UnrealSharp.UnrealEngine.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net8.0</TargetFrameworks>
	<LangVersion>11.0</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>	
    <AllowUnsafeBlocks>True</AllowUnsafeBlocks>
    <GenerateDocumentationFile>True</GenerateDocumentationFile>
    <IsAotCompatible>True</IsAotCompatible>
    <IsTrimmable>True</IsTrimmable>
	<OutputPath>../../../Managed/$(Configuration)</OutputPath>
	<AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
	<AppendRuntimeIdentifierToOutputPath>false</AppendRuntimeIdentifierToOutputPath>
	<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
    <Configurations>Debug-Windows-Editor;Release-Windows-Editor;Debug-Windows-Game;Release-Windows-Game;Debug-Mac-Editor;Release-Mac-Editor;Debug-Mac-Game;Release-Mac-Game;Debug-Linux-Editor;Release-Linux-Editor;Debug-Linux-Game;Release-Linux-Game;Debug-IOS-Game;Release-IOS-Game;Debug-Android-Game;Release-Android-Game</Configurations>
    <AssemblyVersion>1.1.0.0</AssemblyVersion>
    <FileVersion>1.1.0.0</FileVersion>
  </PropertyGroup>

	<Target Name="PreBuild" BeforeTargets="BeforeBuild" Condition="$(Configuration.Contains('Debug'))">
        <Error Text="Please note: The type database file $(ProjectDir)../../../Intermediate/UnrealSharp/NativeTypeDefinition.tdb does not exist. This compilation will not automatically generate C# binding code, which may cause your compilation to fail. &#xD;&#xA;You can generate this .tdb file from UnrealEditor or get more information from here: https://github.com/bodong1987/UnrealSharp" Condition="!Exists('$(ProjectDir)../../../Intermediate/UnrealSharp/NativeTypeDefinition.tdb')"></Error>
        <Message Text="Generate C# binding codes for Unreal C++, Please wait..." Importance="high" Condition="Exists('$(ProjectDir)../../../Intermediate/UnrealSharp/NativeTypeDefinition.tdb')"></Message>
        <Exec Command="dotnet $(ProjectDir)../../../Tools/Publish/UnrealSharpTool/DotNET/UnrealSharpTool.dll -m codegen -t JsonDoc -i $(ProjectDir)../../../Intermediate/UnrealSharp/NativeTypeDefinition.tdb -p $(ProjectDir)../../../ -s NativeBinding" Condition="Exists('$(ProjectDir)../../../Intermediate/UnrealSharp/NativeTypeDefinition.tdb')" />
	</Target>
    
	
	<PropertyGroup Condition="$(Configuration) == 'Debug-Windows-Editor'">
		<DefineConstants>DEBUG;WITH_EDITOR;PLATFORM_WINDOWS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Windows-Editor'">
		<DefineConstants>NDEBUG;WITH_EDITOR;PLATFORM_WINDOWS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Windows-Game'">
		<DefineConstants>DEBUG;PLATFORM_WINDOWS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Windows-Game'">
		<DefineConstants>NDEBUG;PLATFORM_WINDOWS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Mac-Editor'">
		<DefineConstants>DEBUG;WITH_EDITOR;PLATFORM_APPLE;PLATFORM_MAC</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Mac-Editor'">
		<DefineConstants>NDEBUG;WITH_EDITOR;PLATFORM_APPLE;PLATFORM_MAC</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Mac-Game'">
		<DefineConstants>DEBUG;PLATFORM_APPLE;PLATFORM_MAC</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Mac-Game'">
		<DefineConstants>NDEBUG;PLATFORM_APPLE;PLATFORM_MAC</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Linux-Editor'">
		<DefineConstants>DEBUG;WITH_EDITOR;PLATFORM_LINUX</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Linux-Editor'">
		<DefineConstants>NDEBUG;WITH_EDITOR;PLATFORM_LINUX</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Linux-Game'">
		<DefineConstants>DEBUG;PLATFORM_LINUX</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Linux-Game'">
		<DefineConstants>NDEBUG;PLATFORM_LINUX</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-IOS-Game'">
		<DefineConstants>DEBUG;PLATFORM_APPLE;PLATFORM_IOS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-IOS-Game'">
		<DefineConstants>NDEBUG;PLATFORM_APPLE;PLATFORM_IOS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Android-Game'">
		<DefineConstants>DEBUG;PLATFORM_ANDROID</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Android-Game'">
		<DefineConstants>NDEBUG;PLATFORM_ANDROID</DefineConstants>
	</PropertyGroup>

	<ItemGroup Condition="$(Configuration.Contains('Debug')) == ''">
      <None Include="Bindings.Placeholders\**" />
      <Compile Remove="Bindings.Placeholders\**" />
      <None Include="Bindings.Placeholders.Builtin\**" />
      <Compile Remove="Bindings.Placeholders.Builtin\**" />
     </ItemGroup>
	
  <ItemGroup>
    <PackageReference Include="UnrealSharp.Utils" Version="1.1.0" />
  </ItemGroup>
</Project>

```

## UnrealSharp.GameScripts
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
	<TargetFrameworks>net8.0</TargetFrameworks>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
	<OutputPath>../../../Managed/$(Configuration)</OutputPath>
	<AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
	<AppendRuntimeIdentifierToOutputPath>false</AppendRuntimeIdentifierToOutputPath>
	<AllowUnsafeBlocks>True</AllowUnsafeBlocks>
      <AssemblyVersion>1.1.0.0</AssemblyVersion>
      <FileVersion>1.1.0.0</FileVersion>
	  <Configurations>Debug-Windows-Editor;Release-Windows-Editor;Debug-Windows-Game;Release-Windows-Game;Debug-Mac-Editor;Release-Mac-Editor;Debug-Mac-Game;Release-Mac-Game;Debug-Linux-Editor;Release-Linux-Editor;Debug-Linux-Game;Release-Linux-Game;Debug-IOS-Game;Release-IOS-Game;Debug-Android-Game;Release-Android-Game</Configurations>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="UnrealSharp.Utils" Version="1.1.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\UnrealSharp.UnrealEngine\UnrealSharp.UnrealEngine.csproj" />
  </ItemGroup>
	
	<PropertyGroup>
		<BindingsPath>Bindings/CSharpBinding/**/*.cs</BindingsPath>			
	</PropertyGroup>
		
    <Target Name="PreBuild" BeforeTargets="PreBuildEvent" Condition="$(Configuration.Contains('Debug'))">
		<Exec Command="&#xD;&#xA;            echo Generate C# Binding Codes for UnrealSharp Types, Please wait...&#xD;&#xA;            dotnet $(ProjectDir)../../../Tools/Publish/UnrealSharpTool/DotNET/UnrealSharpTool.dll -m codegen -t CSharpCode -i $(ProjectDir) -p $(ProjectDir)../../../ -s CSharpBinding" />
	</Target>
	
	<Target Name="ForceAddBindings" AfterTargets="PreBuildEvent" BeforeTargets="BeforeCompile;CoreCompile">
		<ItemGroup>
			<Compile Remove="$(BindingsPath)" />
			<Compile Include="$(BindingsPath)" />
		</ItemGroup>
	</Target>

	<Target Name="PostBuild" AfterTargets="PostBuildEvent" Condition="$(Configuration.Contains('Debug'))">
      <Exec Command="&#xD;&#xA;            echo Export UnrealSharp Type Database to Unreal, Please wait...&#xD;&#xA;            dotnet $(ProjectDir)../../../Tools/Publish/UnrealSharpTool/DotNET/UnrealSharpTool.dll -m typegen -a $(TargetPath) -p $(ProjectDir)../../../ --sourceDirectory $(ProjectDir) --sourceFileIgnoreRegex &quot;[/\\\\]Bindings\.Defs|obj[/\\\\]&quot; -o $(TargetDir)$(ProjectName).tdb " />
    </Target>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Windows-Editor'">
		<DefineConstants>DEBUG;WITH_EDITOR;PLATFORM_WINDOWS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Windows-Editor'">
		<DefineConstants>NDEBUG;WITH_EDITOR;PLATFORM_WINDOWS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Windows-Game'">
		<DefineConstants>DEBUG;PLATFORM_WINDOWS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Windows-Game'">
		<DefineConstants>NDEBUG;PLATFORM_WINDOWS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Mac-Editor'">
		<DefineConstants>DEBUG;WITH_EDITOR;PLATFORM_APPLE;PLATFORM_MAC</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Mac-Editor'">
		<DefineConstants>NDEBUG;WITH_EDITOR;PLATFORM_APPLE;PLATFORM_MAC</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Mac-Game'">
		<DefineConstants>DEBUG;PLATFORM_APPLE;PLATFORM_MAC</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Mac-Game'">
		<DefineConstants>NDEBUG;PLATFORM_APPLE;PLATFORM_MAC</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Linux-Editor'">
		<DefineConstants>DEBUG;WITH_EDITOR;PLATFORM_LINUX</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Linux-Editor'">
		<DefineConstants>NDEBUG;WITH_EDITOR;PLATFORM_LINUX</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Linux-Game'">
		<DefineConstants>DEBUG;PLATFORM_LINUX</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Linux-Game'">
		<DefineConstants>NDEBUG;PLATFORM_LINUX</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-IOS-Game'">
		<DefineConstants>DEBUG;PLATFORM_APPLE;PLATFORM_IOS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-IOS-Game'">
		<DefineConstants>NDEBUG;PLATFORM_APPLE;PLATFORM_IOS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Android-Game'">
		<DefineConstants>DEBUG;PLATFORM_ANDROID</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Android-Game'">
		<DefineConstants>NDEBUG;PLATFORM_ANDROID</DefineConstants>
	</PropertyGroup>

	<ItemGroup Condition="$(Configuration.Contains('Debug')) == ''">
		<None Include="Bindings.Defs\**" />
		<Compile Remove="Bindings.Defs\**" />
		<None Include="Bindings.Placeholders\**" />
		<Compile Remove="Bindings.Placeholders\**" />
	</ItemGroup>
</Project>
```

## UnrealSharp.GameContent
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net8.0</TargetFrameworks>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>	
	<AllowUnsafeBlocks>True</AllowUnsafeBlocks>
	  <OutputPath>../../../Managed/$(Configuration)</OutputPath>
	  <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
	  <AppendRuntimeIdentifierToOutputPath>false</AppendRuntimeIdentifierToOutputPath>
      <AssemblyVersion>1.1.0.0</AssemblyVersion>
      <FileVersion>1.1.0.0</FileVersion>
	<Configurations>Debug-Windows-Editor;Release-Windows-Editor;Debug-Windows-Game;Release-Windows-Game;Debug-Mac-Editor;Release-Mac-Editor;Debug-Mac-Game;Release-Mac-Game;Debug-Linux-Editor;Release-Linux-Editor;Debug-Linux-Game;Release-Linux-Game;Debug-IOS-Game;Release-IOS-Game;Debug-Android-Game;Release-Android-Game</Configurations>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="UnrealSharp.Utils" Version="1.1.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\UnrealSharp.GameScripts\UnrealSharp.GameScripts.csproj" />
  </ItemGroup>
	<PropertyGroup>
		<BindingsPath>Bindings/CSharpBinding/**/*.cs</BindingsPath>
	</PropertyGroup>
	
	<Target Name="PreBuild" BeforeTargets="BeforeBuild" Condition="$(Configuration.Contains('Debug'))">
		<Exec Command="&#xD;&#xA;            echo Generate C# Binding Codes for Unreal Blueprint, Please wait...&#xD;&#xA;            dotnet $(ProjectDir)../../../Tools/Publish/UnrealSharpTool/DotNET/UnrealSharpTool.dll -m codegen -t JsonDoc -i $(ProjectDir)../../../Intermediate/UnrealSharp/BlueprintTypeDefinition.tdb -p $(ProjectDir)../../../ -s BlueprintBinding" Condition="Exists('$(ProjectDir)../../../Intermediate/UnrealSharp/BlueprintTypeDefinition.tdb')" />
		<Exec Command="&#xD;&#xA;            echo Generate C# Binding Codes for UnrealSharp Types, Please wait...&#xD;&#xA;            dotnet $(ProjectDir)../../../Tools/Publish/UnrealSharpTool/DotNET/UnrealSharpTool.dll -m codegen -t CSharpCode -i $(ProjectDir) -p $(ProjectDir)../../../ -s CSharpBinding" />
	</Target>

	<Target Name="ForceAddBindings" AfterTargets="PreBuildEvent" BeforeTargets="BeforeCompile;CoreCompile">
		<ItemGroup>
			<Compile Remove="$(BindingsPath)" />
			<Compile Include="$(BindingsPath)" />
		</ItemGroup>
	</Target>

	<Target Name="PostBuild" AfterTargets="PostBuildEvent" Condition="$(Configuration.Contains('Debug'))">
		<Exec Command="&#xD;&#xA;            echo Export UnrealSharp Type Database to Unreal, Please wait...&#xD;&#xA;            dotnet $(ProjectDir)../../../Tools/Publish/UnrealSharpTool/DotNET/UnrealSharpTool.dll -m typegen -a $(TargetPath) -p $(ProjectDir)../../../ --sourceDirectory $(ProjectDir) --sourceFileIgnoreRegex &quot;[/\\\\]Bindings\.Defs|obj[/\\\\]&quot; -o $(TargetDir)$(ProjectName).tdb " />
	</Target>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Windows-Editor'">
		<DefineConstants>DEBUG;WITH_EDITOR;PLATFORM_WINDOWS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Windows-Editor'">
		<DefineConstants>NDEBUG;WITH_EDITOR;PLATFORM_WINDOWS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Windows-Game'">
		<DefineConstants>DEBUG;PLATFORM_WINDOWS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Windows-Game'">
		<DefineConstants>NDEBUG;PLATFORM_WINDOWS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Mac-Editor'">
		<DefineConstants>DEBUG;WITH_EDITOR;PLATFORM_APPLE;PLATFORM_MAC</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Mac-Editor'">
		<DefineConstants>NDEBUG;WITH_EDITOR;PLATFORM_APPLE;PLATFORM_MAC</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Mac-Game'">
		<DefineConstants>DEBUG;PLATFORM_APPLE;PLATFORM_MAC</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Mac-Game'">
		<DefineConstants>NDEBUG;PLATFORM_APPLE;PLATFORM_MAC</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Linux-Editor'">
		<DefineConstants>DEBUG;WITH_EDITOR;PLATFORM_LINUX</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Linux-Editor'">
		<DefineConstants>NDEBUG;WITH_EDITOR;PLATFORM_LINUX</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Linux-Game'">
		<DefineConstants>DEBUG;PLATFORM_LINUX</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Linux-Game'">
		<DefineConstants>NDEBUG;PLATFORM_LINUX</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-IOS-Game'">
		<DefineConstants>DEBUG;PLATFORM_APPLE;PLATFORM_IOS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-IOS-Game'">
		<DefineConstants>NDEBUG;PLATFORM_APPLE;PLATFORM_IOS</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Debug-Android-Game'">
		<DefineConstants>DEBUG;PLATFORM_ANDROID</DefineConstants>
	</PropertyGroup>

	<PropertyGroup Condition="$(Configuration) == 'Release-Android-Game'">
		<DefineConstants>NDEBUG;PLATFORM_ANDROID</DefineConstants>
	</PropertyGroup>

	<ItemGroup Condition="$(Configuration.Contains('Debug')) == ''">
		<None Include="Bindings.Defs\**" />
		<Compile Remove="Bindings.Defs\**" />
		<None Include="Bindings.Placeholders\**" />
		<Compile Remove="Bindings.Placeholders\**" />
	</ItemGroup>
</Project>
```
