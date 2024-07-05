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

partial struct FRotator : IEquatable<FRotator>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FRotator"/> struct.
    /// </summary>
    /// <param name="pitch">The pitch.</param>
    /// <param name="yaw">The yaw.</param>
    /// <param name="roll">The roll.</param>
    public FRotator(double pitch, double yaw, double roll)
    {
        Pitch = pitch;
        Yaw = yaw;
        Roll = roll;
    }

    /// <summary>
    /// Equals the specified r.
    /// </summary>
    /// <param name="otherRotator">The r.</param>
    /// <returns><c>true</c> if equal, <c>false</c> otherwise.</returns>
    public bool Equals(FRotator otherRotator)
    {
        // same with unreal engine C++
        // so disable this warning
        // ReSharper disable CompareOfFloatsByEqualityOperator
        return Pitch == otherRotator.Pitch && Yaw == otherRotator.Yaw && Roll == otherRotator.Roll;
        // ReSharper restore CompareOfFloatsByEqualityOperator
    }

    /// <summary>
    /// Determines whether [contains na n].
    /// </summary>
    /// <returns><c>true</c> if [contains na n]; otherwise, <c>false</c>.</returns>
    public bool ContainsNaN()
    {
        return !double.IsFinite(Pitch) || !double.IsNaN(Yaw) || !double.IsNaN(Roll);
    }

    /// <summary>
    /// Gets the forward vector.
    /// </summary>
    /// <returns>FVector.</returns>
    public FVector GetForwardVector()
    {
        return UKismetMathLibrary.GetForwardVector(this);
    }

    /// <summary>
    /// Gets up vector.
    /// </summary>
    /// <returns>FVector.</returns>
    public FVector GetUpVector()
    {
        return UKismetMathLibrary.GetUpVector(this);
    }

    /// <summary>
    /// Gets the right vector.
    /// </summary>
    /// <returns>FVector.</returns>
    public FVector GetRightVector()
    {
        return UKismetMathLibrary.GetRightVector(this);
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        return $"P={Pitch:F3} Y={Yaw:F3} R={Roll:F3}";
    }

    /// <summary>
    /// Implements the + operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static FRotator operator +(FRotator left, FRotator right)
    {
        return new FRotator(left.Pitch + right.Pitch, left.Yaw + right.Yaw, left.Roll + right.Roll);
    }

    /// <summary>
    /// Implements the - operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static FRotator operator -(FRotator left, FRotator right)
    {
        return new FRotator(left.Pitch - right.Pitch, left.Yaw - right.Yaw, left.Roll - right.Roll);
    }

    /// <summary>
    /// Implements the == operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(FRotator left, FRotator right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Implements the != operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(FRotator left, FRotator right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is FRotator otherRotator && Equals(otherRotator);
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Pitch, Yaw, Roll);
    }

    /// <summary>
    /// Determines whether [is nearly zero] [the specified tolerance].
    /// </summary>
    /// <param name="tolerance">The tolerance.</param>
    /// <returns><c>true</c> if [is nearly zero] [the specified tolerance]; otherwise, <c>false</c>.</returns>
    public bool IsNearlyZero(double tolerance = UnrealConstants.KindaSmallNumber)
    {
        return Math.Abs(Pitch) <= tolerance && Math.Abs(Yaw) <= tolerance && Math.Abs(Roll) <= tolerance;
    }

    /// <summary>
    /// Determines whether [is nearly equal] [the specified other].
    /// </summary>
    /// <param name="other">The other.</param>
    /// <returns><c>true</c> if [is nearly equal] [the specified other]; otherwise, <c>false</c>.</returns>
    public bool IsNearlyEqual(FRotator other)
    {
        return (other - this).IsNearlyZero();
    }

}