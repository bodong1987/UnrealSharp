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
using UnrealSharp.UnrealEngine.InteropService;

namespace UnrealSharp.UnrealEngine;

public partial struct FGuid
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FGuid" /> struct.
    /// </summary>
    /// <param name="a">a.</param>
    /// <param name="b">The b.</param>
    /// <param name="c">The c.</param>
    /// <param name="d">The d.</param>
    public FGuid(uint a, uint b, uint c, uint d)
    {
        A = (int)a;
        B = (int)b;
        C = (int)c;
        D = (int)d;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FGuid"/> struct.
    /// </summary>
    /// <param name="a">a.</param>
    /// <param name="b">The b.</param>
    /// <param name="c">The c.</param>
    /// <param name="d">The d.</param>
    public FGuid(int a, int b, int c, int d)
    {
        A = a;
        B = b;
        C = c;
        D = d;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FGuid"/> struct.
    /// </summary>
    /// <param name="guidString">The unique identifier string.</param>
    public FGuid(string guidString)
    {
        this = MiscInteropUtils.MakeGuidFromString(guidString);
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        return $"{A:X8}-{B:X8}-{C:X8}-{D:X8}";
    }

    /// <summary>
    /// Invalidates this instance.
    /// </summary>
    public void Invalidate()
    {
        A = B = C = D = 0;
    }

    /// <summary>
    /// Implements the == operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(FGuid left, FGuid right)
    {
        return left.A == right.A && left.B == right.B && left.C == right.C && left.D == right.D;
    }

    /// <summary>
    /// Implements the != operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(FGuid left, FGuid right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is FGuid guid)
        {
            return A == guid.A && B == guid.B && C == guid.C && D == guid.D;
        }

        return false;
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(A, B, C, D);
    }

    /// <summary>
    /// Creates new guid.
    /// </summary>
    /// <returns>FGuid.</returns>
    public static FGuid NewGuid()
    {
        var guid = Guid.NewGuid();

        var bytes = guid.ToByteArray();
        
        var formattedGuid =
            $"{BitConverter.ToInt32(bytes, 0):X8}-{BitConverter.ToInt32(bytes, 4):X8}-{BitConverter.ToInt32(bytes, 8):X8}-{BitConverter.ToInt32(bytes, 12):X8}";

        return new FGuid(formattedGuid);
    }
}