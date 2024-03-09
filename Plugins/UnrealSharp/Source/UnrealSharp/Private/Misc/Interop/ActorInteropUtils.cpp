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
#include "Misc/InteropUtils.h"
#include "Engine/World.h"

namespace UnrealSharp
{
	FCSharpObjectMarshalValue FInteropUtils::GetActorWorld(const AActor* InActor)
	{
		UWorld* World = InActor != nullptr ? InActor->GetWorld() : nullptr;

		return World != nullptr ? GetCSharpObjectOfUnrealObject(World) : FCSharpObjectMarshalValue();
	}

	FCSharpObjectMarshalValue FInteropUtils::GetActorGameInstance(const AActor* InActor)
	{
		UGameInstance* Instance = InActor != nullptr ? InActor->GetGameInstance() : nullptr;

		return Instance != nullptr ? GetCSharpObjectOfUnrealObject(Instance) : FCSharpObjectMarshalValue();
	}

	FCSharpObjectMarshalValue FInteropUtils::SpawnActorByTransform(UWorld* InWorld, UClass* InClass, const void* InTransformPtr, int InTransformSize)
	{		
		// C++'s FTransform is aligned according to a certain value. 
		// I'm not sure whether the address passed directly from C# will also be aligned. 
		// This may be risky, so I'll go around it here.
		FTransform Transform;

		check(InTransformSize == sizeof(Transform));
		memcpy(&Transform, InTransformPtr, InTransformSize);

		check(InWorld);

		AActor* Actor = InWorld->SpawnActor<AActor>(InClass, Transform);

		return Actor != nullptr ? GetCSharpObjectOfUnrealObject(Actor) : FCSharpObjectMarshalValue();
	}

	FCSharpObjectMarshalValue FInteropUtils::SpawnActor(UWorld* InWorld, UClass* InClass, const FVector* InLocation, const FRotator* InRotation)
	{
		check(InWorld);
		check(InLocation);
		check(InRotation);

		AActor* Actor = InWorld->SpawnActor<AActor>(InClass, *InLocation, *InRotation);

		return Actor != nullptr ? GetCSharpObjectOfUnrealObject(Actor) : FCSharpObjectMarshalValue();
	}
}

