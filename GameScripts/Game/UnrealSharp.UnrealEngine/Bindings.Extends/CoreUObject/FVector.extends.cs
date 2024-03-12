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
namespace UnrealSharp.UnrealEngine
{
    partial struct FVector : IEquatable<FVector>
    {
        /// <summary>
        /// The zero
        /// </summary>
        public readonly static FVector Zero = new FVector(0,0,0);

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
        /// Equalses the specified v.
        /// </summary>
        /// <param name="V">The v.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Equals(FVector V)
        {
            return X == V.X && Y == V.Y && Z == V.Z;
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
            return string.Format("X={0:F3} Y={1:F3} Z={2:F3}", X, Y, Z);
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
        /// <param name="V">The v.</param>
        /// <returns>The result of the operator.</returns>
        public static FVector operator -(FVector V)
        {
            return new FVector(-V.X, -V.Y, -V.Z);
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
            double rscale = 1.0 / scale;
            return new FVector(left.X * rscale, left.Y * rscale, left.Z * rscale);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is FVector V)
            {
                return Equals(V);
            }

            return false;
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
        /// <param name="Tolerance">The tolerance.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool AllComponentsEqual(double Tolerance = UnrealConstants.KindaSmallNumber)
        {
            return System.Math.Abs(X - Y) <= Tolerance && System.Math.Abs(X - Z) <= Tolerance && System.Math.Abs(Y - Z) <= Tolerance;
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
            return System.Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        /// <summary>
        /// Lengthes this instance.
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
        /// Squareds the length.
        /// </summary>
        /// <returns>System.Double.</returns>
        public double SquaredLength()
        {
            return SizeSquared();
        }

        /// <summary>
        /// Determines whether [is nearly zero] [the specified tolerance].
        /// </summary>
        /// <param name="Tolerance">The tolerance.</param>
        /// <returns><c>true</c> if [is nearly zero] [the specified tolerance]; otherwise, <c>false</c>.</returns>
        public bool IsNearlyZero(double Tolerance = UnrealConstants.KindaSmallNumber)
        {
            return System.Math.Abs(X) <= Tolerance && System.Math.Abs(Y) <= Tolerance && System.Math.Abs(Z) <= Tolerance;
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
        /// <param name="Tolerance">The tolerance.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Normalize(double Tolerance = UnrealConstants.SmallNumber)
        {
            double SquareSum = X * X + Y * Y + Z * Z;
            if (SquareSum > Tolerance)
            {
                double Scale = 1.0 / System.Math.Sqrt(SquareSum);
                X *= Scale;
                Y *= Scale;
                Z *= Scale;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether this instance is normalized.
        /// </summary>
        /// <returns><c>true</c> if this instance is normalized; otherwise, <c>false</c>.</returns>
        public bool IsNormalized()
        {
            return System.Math.Abs(1.0 - SizeSquared()) < UnrealConstants.ThreshVectorNormalized;
        }
    }
}
