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
using UnrealSharp.Utils.Misc;

namespace UnrealSharp.UnrealEngine.InteropService
{
    /// <summary>
    /// Class UnrealInvocation.
    /// Implements the <see cref="IDisposable" />
    /// </summary>
    /// <seealso cref="IDisposable" />
    public class UnrealInvocation : IDisposable
    {
        /// <summary>
        /// Gets the native PTR.
        /// </summary>
        /// <value>The native PTR.</value>
        public IntPtr NativePtr { get; private set; }

        /// <summary>
        /// The method name
        /// only for debug
        /// </summary>
        readonly string MethodName;

        /// <summary>
        /// The parameter size
        /// Param size, invalid value is -1
        /// </summary>
        public readonly int ParamSize;

        private UnrealInvocation(IntPtr invocationNativePtr, string methodName)
        {
            MethodName = methodName;
            NativePtr = invocationNativePtr;
            Logger.Ensure<Exception>(NativePtr != IntPtr.Zero, "Failed bind Unreal Function. {0}", methodName);

            ParamSize = InvocationInteropUtils.GetUnrealInvocationParameterSize(NativePtr);

            Logger.Ensure<Exception>(ParamSize != -1, "Invalid function param size of {0}", methodName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnrealInvocation"/> class.
        /// </summary>
        /// <param name="classPtr">The class PTR.</param>
        /// <param name="methodName">Name of the method.</param>
        public UnrealInvocation(UClass classPtr, string methodName) :
            this(InvocationInteropUtils.CreateUnrealInvocation(classPtr.GetNativePtr(), methodName), methodName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnrealInvocation"/> class.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="methodName">Name of the method.</param>
        public UnrealInvocation(UObject @object, string methodName) :
            this(InvocationInteropUtils.CreateUnrealInvocation(@object.GetClass()!.GetNativePtr(), methodName), methodName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnrealInvocation"/> class.
        /// </summary>
        /// <param name="addressOfDelegateProperty">The address of delegate property.</param>
        public UnrealInvocation(IntPtr addressOfDelegateProperty) :
            this(InvocationInteropUtils.CreateUnrealInvocationFromDelegateProperty(addressOfDelegateProperty), "DelegateProperty")
        {
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid => NativePtr != IntPtr.Zero;

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            if (NativePtr != IntPtr.Zero)
            {
                InvocationInteropUtils.DestroyUnrealInvocation(NativePtr);
                NativePtr = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Invokes the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="addressOfParameters">The address of parameters.</param>
        /// <param name="paramsSize">Size of the parameters.</param>
        public void Invoke(UObject? target, IntPtr addressOfParameters, int paramsSize)
        {
            if (NativePtr != IntPtr.Zero)
            {
                IntPtr ObjectPtr = target != null ? target.GetNativePtr() : IntPtr.Zero;

                // Logger.LogD("Invoke {0} InvocationPtr = 0x{1:x} with UnrealObjectPtr = 0x{2:x} paramAddress = 0x{3:x} paramSize = {4}", MethodName, NativePtr, ObjectPtr, addressOfParameters, paramsSize);

                InvocationInteropUtils.InvokeUnrealInvocation(NativePtr, ObjectPtr, addressOfParameters, paramsSize);
            }
        }

        /// <summary>
        /// Invokes the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="addressOfParameters">The address of parameters.</param>
        public void Invoke(UObject? target, IntPtr addressOfParameters)
        {
            Invoke(target, addressOfParameters, ParamSize);
        }

        /// <summary>
        /// Gets the function.
        /// </summary>
        /// <returns>IntPtr.</returns>
        public IntPtr GetFunction()
        {
            return NativePtr != IntPtr.Zero ? InvocationInteropUtils.GetUnrealInvocationFunction(NativePtr) : IntPtr.Zero;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return MethodName;
        }
    }

    /// <summary>
    /// Class ScopedUnrealInvocation.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class ScopedUnrealInvocation : IDisposable
    {
        /// <summary>
        /// The invocation
        /// </summary>
        readonly UnrealInvocation Invocation;

        /// <summary>
        /// The address of parameter buffer
        /// </summary>
        readonly IntPtr AddressOfParameterBuffer;

        /// <summary>
        /// The buffer size
        /// </summary>
        readonly int BufferSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopedUnrealInvocation" /> class.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        /// <param name="addressOfParameterBuffer">The address of parameter buffer.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        public ScopedUnrealInvocation(
            UnrealInvocation invocation,
            IntPtr addressOfParameterBuffer,
            int bufferSize
            )
        {
            Invocation = invocation;
            AddressOfParameterBuffer = addressOfParameterBuffer;
            BufferSize = bufferSize;

            InvocationInteropUtils.InitializeUnrealInvocationParameters(invocation.NativePtr, AddressOfParameterBuffer, BufferSize);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopedUnrealInvocation"/> class.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        /// <param name="addressOfParameterBuffer">The address of parameter buffer.</param>
        public ScopedUnrealInvocation(
            UnrealInvocation invocation,
            IntPtr addressOfParameterBuffer
            ) :
            this(invocation, addressOfParameterBuffer, invocation.ParamSize)
        {            
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            InvocationInteropUtils.UnInitializeUnrealInvocationParameters(Invocation.NativePtr, AddressOfParameterBuffer, BufferSize);
        }
    }
}
