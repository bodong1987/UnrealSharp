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

partial struct FVector : IEquatable<FVector>
{
    /// <summary>
    /// The zero
    /// </summary>
    public static readonly FVector Zero = new (0,0,0);

    /// <summary>
    /// Initializes a new instance of the <see cref="FVector"/> struct.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="z">The z.</param>
    public FVector(double x, double y, double z)
    {
        X = x; Y = y; Z = z;
    }

    /// <summary>
    /// Equals the specified v.
    /// </summary>
    /// <param name="otherVector">The v.</param>
    /// <returns><c>true</c> if equal, <c>false</c> otherwise.</returns>
    public bool Equals(FVector otherVector)
    {
        // ReSharper disable CompareOfFloatsByEqualityOperator
        return X == otherVector.X && Y == otherVector.Y && Z == otherVector.Z;
        // ReSharper restore CompareOfFloatsByEqualityOperator
    }

    /// <summary>
    /// Determines whether [contains na n].
    /// </summary>
    /// <returns><c>true</c> if [contains na n]; otherwise, <c>false</c>.</returns>
    public bool ContainsNaN()
    {
        return !double.IsFinite(X) || !double.IsNaN(Y) || !double.IsNaN(Z);
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        return $"X={X:F3} Y={Y:F3} Z={Z:F3}";
    }

    /// <summary>
    /// Implements the == operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(FVector left, FVector right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Implements the != operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(FVector left, FVector right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Implements the + operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static FVector operator +(FVector left, FVector right)
    {
        return new FVector(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    /// <summary>
    /// Implements the - operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static FVector operator -(FVector left, FVector right)
    {
        return new FVector(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    /// <summary>
    /// Implements the * operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static FVector operator *(FVector left, FVector right)
    {
        return new FVector(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
    }

    /// <summary>
    /// Implements the / operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static FVector operator /(FVector left, FVector right)
    {
        return new FVector(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
    }

    /// <summary>
    /// Implements the - operator.
    /// </summary>
    /// <param name="vector">The other vector.</param>
    /// <returns>The result of the operator.</returns>
    public static FVector operator -(FVector vector)
    {
        return new FVector(-vector.X, -vector.Y, -vector.Z);
    }

    /// <summary>
    /// Implements the * operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="scale">The scale.</param>
    /// <returns>The result of the operator.</returns>
    public static FVector operator *(FVector left, double scale)
    {
        return new FVector(left.X * scale, left.Y * scale, left.Z * scale);
    }

    /// <summary>
    /// Implements the / operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="scale">The scale.</param>
    /// <returns>The result of the operator.</returns>
    public static FVector operator /(FVector left, double scale)
    {
        var reversedScale = 1.0 / scale;
        return new FVector(left.X * reversedScale, left.Y * reversedScale, left.Z * reversedScale);
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is FVector otherVector && Equals(otherVector);
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    /// <summary>
    /// Alls the components equal.
    /// </summary>
    /// <param name="tolerance">The tolerance.</param>
    /// <returns><c>true</c> if equal with tolerance, <c>false</c> otherwise.</returns>
    public bool AllComponentsEqual(double tolerance = UnrealConstants.KindaSmallNumber)
    {
        return Math.Abs(X - Y) <= tolerance && Math.Abs(X - Z) <= tolerance && Math.Abs(Y - Z) <= tolerance;
    }

    /// <summary>
    /// Sets the specified x.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="z">The z.</param>
    public void Set(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Sizes this instance.
    /// </summary>
    /// <returns>System.Double.</returns>
    public double Size()
    {
        return Math.Sqrt(X * X + Y * Y + Z * Z);
    }

    /// <summary>
    /// get the Length of this instance.
    /// </summary>
    /// <returns>System.Double.</returns>
    public double Length()
    {
        return Size();
    }

    /// <summary>
    /// Sizes the squared.
    /// </summary>
    /// <returns>System.Double.</returns>
    public double SizeSquared()
    {
        return X * X + Y * Y + Z * Z;
    }

    /// <summary>
    /// Calculate Squared length.
    /// </summary>
    /// <returns>System.Double.</returns>
    public double SquaredLength()
    {
        return SizeSquared();
    }

    /// <summary>
    /// Determines whether [is nearly zero] [the specified tolerance].
    /// </summary>
    /// <param name="tolerance">The tolerance.</param>
    /// <returns><c>true</c> if [is nearly zero] [the specified tolerance]; otherwise, <c>false</c>.</returns>
    public bool IsNearlyZero(double tolerance = UnrealConstants.KindaSmallNumber)
    {
        return Math.Abs(X) <= tolerance && Math.Abs(Y) <= tolerance && Math.Abs(Z) <= tolerance;
    }

    /// <summary>
    /// Determines whether [is nearly equal] [the specified other].
    /// </summary>
    /// <param name="other">The other.</param>
    /// <returns><c>true</c> if [is nearly equal] [the specified other]; otherwise, <c>false</c>.</returns>
    public bool IsNearlyEqual(FVector other)
    {
        return (other - this).IsNearlyZero();
    }

    /// <summary>
    /// Determines whether this instance is zero.
    /// </summary>
    /// <returns><c>true</c> if this instance is zero; otherwise, <c>false</c>.</returns>
    public bool IsZero()
    {
        return X == 0 && Y == 0 && Z == 0;
    }

    /// <summary>
    /// Normalizes the specified tolerance.
    /// </summary>
    /// <param name="tolerance">The tolerance.</param>
    /// <returns><c>true</c> if normalize success, <c>false</c> otherwise.</returns>
    public bool Normalize(double tolerance = UnrealConstants.SmallNumber)
    {
        var squareSum = X * X + Y * Y + Z * Z;
        if (!(squareSum > tolerance))
        {
            return false;
        }
            
        var scale = 1.0 / Math.Sqrt(squareSum);
        X *= scale;
        Y *= scale;
        Z *= scale;

        return true;

    }

    /// <summary>
    /// Determines whether this instance is normalized.
    /// </summary>
    /// <returns><c>true</c> if this instance is normalized; otherwise, <c>false</c>.</returns>
    public bool IsNormalized()
    {
        return Math.Abs(1.0 - SizeSquared()) < UnrealConstants.ThreshVectorNormalized;
    }
}