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
namespace UnrealSharp.UnrealEngine;

partial struct FLinearColor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FLinearColor" /> struct.
    /// </summary>
    /// <param name="r">The r.</param>
    /// <param name="g">The g.</param>
    /// <param name="b">The b.</param>
    /// <param name="a">a.</param>
    public FLinearColor(float r, float g, float b, float a = 1.0f)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FLinearColor" /> struct.
    /// </summary>
    /// <param name="color">The color.</param>
    public FLinearColor(FColor color)
    {
        R = color.R / (float)0xFF;
        G = color.G / (float)0xFF;
        B = color.B / (float)0xFF;
        A = color.A / (float)0xFF;
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        return $"(R={R},G={G},B={B},A={A})";
    }

    /// <summary>
    /// Implements the == operator.
    /// </summary>
    /// <param name="c1">The c1.</param>
    /// <param name="c2">The c2.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(FLinearColor c1, FLinearColor c2)
    {
        // same with unreal engine in C++
        // so we disable this warning
        // ReSharper disable CompareOfFloatsByEqualityOperator
        return c1.R == c2.R && c1.G == c2.G && c1.B == c2.B && c1.A == c2.A;
        // ReSharper restore CompareOfFloatsByEqualityOperator
    }

    /// <summary>
    /// Implements the != operator.
    /// </summary>
    /// <param name="c1">The c1.</param>
    /// <param name="c2">The c2.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(FLinearColor c1, FLinearColor c2)
    {
        return !(c1 == c2);
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is FLinearColor c)
        {
            return this == c;
        }

        return false;
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B, A);
    }

    /// <summary>
    /// Converts to color.
    /// </summary>
    /// <param name="bSRGB">if set to <c>true</c> [b SRGB].</param>
    /// <returns>FColor.</returns>
    // ReSharper disable once InconsistentNaming
    public FColor ToColor(bool bSRGB = false)
    {
        var floatR = bSRGB ? (float)Math.Sqrt(R) : R;
        var floatG = bSRGB ? (float)Math.Sqrt(G) : G;
        var floatB = bSRGB ? (float)Math.Sqrt(B) : B;
        var floatA = A;

        return new FColor(
            (byte)(floatR * 255),
            (byte)(floatG * 255),
            (byte)(floatB * 255),
            (byte)(floatA * 255));
    }

    /// <summary>
    /// Converts to vector.
    /// </summary>
    /// <returns>FVector.</returns>
    public FVector ToVector()
    {
        return new FVector(R, G, B);
    }

    #region Global Colors
    /// <summary>
    /// The white
    /// </summary>
    public static readonly FLinearColor White = new(1.0f, 1.0f, 1.0f);
    /// <summary>
    /// The gray
    /// </summary>
    public static readonly FLinearColor Gray = new(0.5f, 0.5f, 0.5f);
    /// <summary>
    /// The black
    /// </summary>
    public static readonly FLinearColor Black = new(0, 0, 0);
    /// <summary>
    /// The transparent
    /// </summary>
    public static readonly FLinearColor Transparent = new(0, 0, 0, 0);
    /// <summary>
    /// The red
    /// </summary>
    public static readonly FLinearColor Red = new(1.0f, 0, 0);
    /// <summary>
    /// The green
    /// </summary>
    public static readonly FLinearColor Green = new(0, 1.0f, 0);
    /// <summary>
    /// The blue
    /// </summary>
    public static readonly FLinearColor Blue = new(0, 0, 1.0f);
    /// <summary>
    /// The yellow
    /// </summary>
    public static readonly FLinearColor Yellow = new(1.0f, 1.0f, 0);
    #endregion
}