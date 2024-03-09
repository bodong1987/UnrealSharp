/*
	MIT License

	Copyright (c) 2024 UnrealSharp

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all
	copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
	SOFTWARE.

	Project URL: https://github.com/bodong1987/UnrealSharp
*/
#include "Misc/CSharpStructures.h"

namespace UnrealSharp
{	
	FCSharpTopLevelAssetPath::FCSharpTopLevelAssetPath()
	{
	}

	FCSharpTopLevelAssetPath::FCSharpTopLevelAssetPath(const FTopLevelAssetPath& InAssetPath) :
		PackageName(InAssetPath.GetPackageName()),
		AssetName(InAssetPath.GetAssetName())
	{
	}        

	FUnrealSharpBuildInfo FUnrealSharpBuildInfo::GetNativeBuildInfo()
	{
		FUnrealSharpBuildInfo Result;

#if WITH_EDITOR
		Result.bWithEditor = true;
#else
		Result.bWithEditor = false;
#endif

#if PLATFORM_WINDOWS
		Result.Platform = EUnrealSharpPlatform::Windows;
#elif PLATFORM_MAC
		Result.Platform = EUnrealSharpPlatform::Mac;
#elif PLATFORM_LINUX
		Result.Platform = EUnrealSharpPlatform::Linux;
#elif PLATFORM_IOS
		Result.Platform = EUnrealSharpPlatform::IOS;
#elif PLATFORM_ANDROID
		Result.Platform = EUnrealSharpPlatform::Android;
#else
#error "Unsupported platform"
#endif

#if UE_BUILD_DEBUG
		Result.Configuration = EUnrealSharpBuildConfiguration::Debug;
#elif NDEBUG
		Result.Configuration = EUnrealSharpBuildConfiguration::Release;
#endif

		return Result;
	}

	FString FUnrealSharpBuildInfo::GetPlatformString(EUnrealSharpPlatform InPlatform)
	{
		switch (InPlatform)
		{
		case UnrealSharp::EUnrealSharpPlatform::Windows:
			return TEXT("Windows");
		case UnrealSharp::EUnrealSharpPlatform::Mac:
			return TEXT("Mac");
		case UnrealSharp::EUnrealSharpPlatform::Linux:
			return TEXT("Linux");
		case UnrealSharp::EUnrealSharpPlatform::IOS:
			return TEXT("IOS");
		case UnrealSharp::EUnrealSharpPlatform::Android:
			return TEXT("Android");
		default:
			return TEXT("Unknown");
		}
	}

	FString FUnrealSharpBuildInfo::GetBuildConfigurationString(EUnrealSharpBuildConfiguration InConfiguration)
	{
		switch (InConfiguration)
		{
		case UnrealSharp::EUnrealSharpBuildConfiguration::Debug:
			return TEXT("Debug");
		case UnrealSharp::EUnrealSharpBuildConfiguration::Release:
			return TEXT("Release");
		default:
			return TEXT("Unknown");
		}
	}
}


