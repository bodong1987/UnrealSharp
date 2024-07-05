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

public partial struct FInputActionValue
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>The value.</value>
    public FVector Value { get; private set; } = new();

    /// <summary>
    /// Gets the type of the value.
    /// </summary>
    /// <value>The type of the value.</value>
    public EInputActionValueType ValueType { get; private set; } = EInputActionValueType.Boolean;

    /// <summary>
    /// Initializes a new instance of the <see cref="FInputActionValue"/> struct.
    /// </summary>
    /// <param name="value">if set to <c>true</c> [value].</param>
    public FInputActionValue(bool value)
    {
        Value = new FVector(value ? 1 : 0, 0, 0);
        ValueType = EInputActionValueType.Boolean;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FInputActionValue"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public FInputActionValue(double value)
    {
        Value = new FVector(value, 0, 0);
        ValueType = EInputActionValueType.Axis1D;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FInputActionValue"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public FInputActionValue(FVector2D value)
    {
        Value = new FVector(value.X, value.Y, 0);
        ValueType = EInputActionValueType.Axis2D;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FInputActionValue"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public FInputActionValue(FVector value)
    {
        Value = value;
        ValueType = EInputActionValueType.Axis3D;
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        Value = new FVector(0, 0, 0);
    }

    /// <summary>
    /// Gets the type of the value.
    /// </summary>
    /// <returns>EInputActionValueType.</returns>
    public EInputActionValueType GetValueType()
    {
        return ValueType;
    }

    /// <summary>
    /// Gets the boolean.
    /// </summary>
    public bool GetBoolean()
    {
        return IsNonZero();
    }

    /// <summary>
    /// Gets the axis1 d.
    /// </summary>
    /// <returns>System.Double.</returns>
    public double GetAxis1D()
    {
        return Value.X;
    }

    /// <summary>
    /// Gets the axis2 d.
    /// </summary>
    /// <returns>FVector2D.</returns>
    public FVector2D GetAxis2D()
    {
        return new FVector2D { X = Value.X, Y = Value.Y };
    }

    /// <summary>
    /// Gets the axis3 d.
    /// </summary>
    /// <returns>FVector.</returns>
    public FVector GetAxis3D()
    {
        return Value;
    }

    /// <summary>
    /// Determines whether [is nonzero] [the specified tolerance].
    /// </summary>
    /// <param name="tolerance">The tolerance.</param>
    /// <returns><c>true</c> if [is nonzero] [the specified tolerance]; otherwise, <c>false</c>.</returns>
    public bool IsNonZero(double tolerance = UnrealConstants.KindaSmallNumber)
    {
        return Value.SizeSquared() >= tolerance * tolerance;
    }

    /// <summary>
    /// Implements the + operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static FInputActionValue operator +(FInputActionValue left, FInputActionValue right)
    {
        var value = new FInputActionValue
        {
            Value = left.Value + right.Value,
            ValueType = (EInputActionValueType)Math.Max((byte)left.ValueType, (byte)right.ValueType)
        };

        return value;
    }

    /// <summary>
    /// Implements the + operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The result of the operator.</returns>
    public static FInputActionValue operator +(FInputActionValue left, float scalar)
    {
        var value = new FInputActionValue
        {
            Value = left.Value * scalar
        };

        return value;
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public override string ToString()
    {
        return GetValueType() switch
        {
            EInputActionValueType.Boolean => IsNonZero() ? "true" : "false",
            EInputActionValueType.Axis1D => $"{Value.X:F3}",
            EInputActionValueType.Axis2D => $"X={Value.X:F3} Y={Value.Y:F3}",
            EInputActionValueType.Axis3D => $"X={Value.X:F3} Y={Value.Y:F3} Z={Value.Z:F3}",
            _ => throw new NotImplementedException()
        };
    }
}