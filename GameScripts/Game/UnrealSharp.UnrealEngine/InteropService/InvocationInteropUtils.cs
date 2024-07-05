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
// ReSharper disable MemberHidesStaticFromOuterClass
namespace UnrealSharp.UnrealEngine.InteropService;

/// <summary>
/// Class InvocationInteropUtils.
/// </summary>
public static unsafe class InvocationInteropUtils
{
    #region Interop Function Pointers     
    /// <summary>
    /// Class InteropFunctionPointers
    /// Since mono does not support setting delegate* unmanaged type fields directly through reflection,
    /// Therefore we cannot directly declare delegate* unmanaged fields and set them through reflection
    /// So we use this method to set it indirectly, first save the external function pointer to these IntPtr,
    /// and then solve it through forced type conversion when calling.Although this is a bit inconvenient,
    /// there is currently no other way unless Mono supports it in the future.
    /// ReSharper disable once CommentTypo
    /// @reference check here: https://github.com/dotnet/runtime/blob/main/src/mono/mono/metadata/icall.c#L2134  ves_icall_RuntimeFieldInfo_SetValueInternal
    /// </summary>
    private static class InteropFunctionPointers
    {
#pragma warning disable CS0649 // The compiler detected an uninitialized private or internal field declaration that is never assigned a value. [We use reflection to bind all fields of this class]
        /// <summary>
        /// create unreal invocation
        /// </summary>
        public static readonly IntPtr CreateUnrealInvocation;
        /// <summary>
        /// create unreal invocation from delegate property
        /// </summary>
        public static readonly IntPtr CreateUnrealInvocationFromDelegateProperty;
        /// <summary>
        /// The destroy unreal invocation
        /// </summary>
        public static readonly IntPtr DestroyUnrealInvocation;
        /// <summary>
        /// The invoke unreal invocation
        /// </summary>
        public static readonly IntPtr InvokeUnrealInvocation;
        /// <summary>
        /// The get unreal invocation function
        /// </summary>
        public static readonly IntPtr GetUnrealInvocationFunction;
        /// <summary>
        /// The get unreal invocation parameter size
        /// </summary>
        public static readonly IntPtr GetUnrealInvocationParameterSize;
        /// <summary>
        /// The initialize unreal invocation parameters
        /// </summary>
        public static readonly IntPtr InitializeUnrealInvocationParameters;
        /// <summary>
        /// The un initialize unreal invocation parameters
        /// </summary>
        public static readonly IntPtr UnInitializeUnrealInvocationParameters;
#pragma warning restore CS0649

        /// <summary>
        /// static constructor
        /// </summary>
        static InteropFunctionPointers()
        {
            InteropFunctions.BindInteropFunctionPointers(typeof(InteropFunctionPointers));
        }
    }
    #endregion

    #region Imports        
    /// <summary>
    /// Creates UnrealInvocation by UClass and MethodName
    /// </summary>
    /// <param name="classPtr">The class PTR.</param>
    /// <param name="methodName">Name of the method.</param>
    /// <returns>nint.</returns>
    public static IntPtr CreateUnrealInvocation(IntPtr classPtr, string methodName)
    {
        return ((delegate* unmanaged[Cdecl]<IntPtr, string, IntPtr>)InteropFunctionPointers.CreateUnrealInvocation)(classPtr, methodName);
    }

    /// <summary>
    /// Creates the unreal invocation from delegate property.
    /// </summary>
    /// <param name="delegatePropertyPtr">The delegate property PTR.</param>
    /// <returns>nint.</returns>
    public static IntPtr CreateUnrealInvocationFromDelegateProperty(IntPtr delegatePropertyPtr)
    {
        return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.CreateUnrealInvocationFromDelegateProperty)(delegatePropertyPtr);
    }

    /// <summary>
    /// Destroys the unreal invocation.
    /// </summary>
    /// <param name="invocationPtr">The invocation PTR.</param>
    public static void DestroyUnrealInvocation(IntPtr invocationPtr)
    {
        ((delegate* unmanaged[Cdecl]<IntPtr, void>)InteropFunctionPointers.DestroyUnrealInvocation)(invocationPtr);
    }

    /// <summary>
    /// Invokes the unreal invocation.
    /// </summary>
    /// <param name="invocationPtr">The invocation PTR.</param>
    /// <param name="unrealObjectPtr">The unreal object PTR.</param>
    /// <param name="addressOfParameterBuffer">The address of parameter buffer.</param>
    /// <param name="paramSize">Size of the parameter.</param>
    public static void InvokeUnrealInvocation(IntPtr invocationPtr, IntPtr unrealObjectPtr, IntPtr addressOfParameterBuffer, int paramSize)
    {
        ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, int, void>)InteropFunctionPointers.InvokeUnrealInvocation)(invocationPtr, unrealObjectPtr, addressOfParameterBuffer, paramSize);
    }

    /// <summary>
    /// Gets the unreal invocation function.
    /// </summary>
    /// <param name="invocationPtr">The invocation PTR.</param>
    /// <returns>nint.</returns>
    public static IntPtr GetUnrealInvocationFunction(IntPtr invocationPtr)
    {
        return ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr>)InteropFunctionPointers.GetUnrealInvocationFunction)(invocationPtr);
    }

    /// <summary>
    /// Gets the size of the unreal invocation parameters.
    /// </summary>
    /// <param name="invocationPtr">The invocation PTR.</param>
    /// <returns>int.</returns>
    public static int GetUnrealInvocationParameterSize(IntPtr invocationPtr)
    {
        return ((delegate* unmanaged[Cdecl]<IntPtr, int>)InteropFunctionPointers.GetUnrealInvocationParameterSize)(invocationPtr);
    }

    /// <summary>
    /// Initializes the unreal invocation parameters.
    /// </summary>
    /// <param name="invocationPtr">The invocation PTR.</param>
    /// <param name="addressOfParameterBuffer">The address of parameter buffer.</param>
    /// <param name="paramSize">Size of the parameter.</param>
    public static void InitializeUnrealInvocationParameters(IntPtr invocationPtr, IntPtr addressOfParameterBuffer, int paramSize)
    {
        ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, void>)InteropFunctionPointers.InitializeUnrealInvocationParameters)(invocationPtr, addressOfParameterBuffer, paramSize);
    }

    /// <summary>
    /// Uns the initialize unreal invocation parameters.
    /// </summary>
    /// <param name="invocationPtr">The invocation PTR.</param>
    /// <param name="addressOfParameterBuffer">The address of parameter buffer.</param>
    /// <param name="paramSize">Size of the parameter.</param>
    public static void UnInitializeUnrealInvocationParameters(IntPtr invocationPtr, IntPtr addressOfParameterBuffer, int paramSize)
    {
        ((delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, void>)InteropFunctionPointers.UnInitializeUnrealInvocationParameters)(invocationPtr, addressOfParameterBuffer, paramSize);
    }
    #endregion
}