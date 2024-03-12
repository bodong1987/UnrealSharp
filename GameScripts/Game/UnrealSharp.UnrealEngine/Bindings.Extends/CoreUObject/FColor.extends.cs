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
    partial struct FColor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FColor" /> struct.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <param name="a">a.</param>
        public FColor(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FColor" /> struct.
        /// </summary>
        /// <param name="color">The color.</param>
        public FColor(uint color)
        {
            // only support little endian...
            B = (byte)(color >> 24);
            G = (byte)(color >> 16);
            R = (byte)(color >> 8);
            A = (byte)(color >> 0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FColor" /> struct.
        /// </summary>
        /// <param name="linearColor">Color of the linear.</param>
        public FColor(FLinearColor linearColor)
        {
            B = (byte)(linearColor.B * 255);
            G = (byte)(linearColor.G * 255);
            R = (byte)(linearColor.R * 255);
            A = (byte)(linearColor.A * 255);
        }

        /// <summary>
        /// Converts to dwcolor.
        /// </summary>
        /// <returns>System.UInt32.</returns>
        public uint ToDWColor()
        {
            return ToPackedBGRA();
        }

        /// <summary>
        /// Converts to packedargb.
        /// </summary>
        /// <returns>System.UInt32.</returns>
        public uint ToPackedARGB()
        {
            return ((uint)A << 24) | ((uint)R << 16) | ((uint)G << 8) | ((uint)B << 0);
        }

        /// <summary>
        /// Converts to packagedabgr.
        /// </summary>
        /// <returns>System.UInt32.</returns>
        public uint ToPackagedABGR()
        {
            return ((uint)A << 24) | ((uint)B << 16) | ((uint)G << 8) | ((uint)R << 0);
        }

        /// <summary>
        /// Converts to packedrgba.
        /// </summary>
        /// <returns>System.UInt32.</returns>
        public uint ToPackedRGBA()
        {
            return ((uint)R << 24) | ((uint)G << 16) | ((uint)B << 8) | ((uint)A << 0);
        }

        /// <summary>
        /// Converts to packedbgra.
        /// </summary>
        /// <returns>System.UInt32.</returns>
        public uint ToPackedBGRA()
        {
            return ((uint)B << 24) | ((uint)G << 16) | ((uint)R << 8) | ((uint)A << 0);
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="c1">The c1.</param>
        /// <param name="c2">The c2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(FColor c1, FColor c2)
        {
            return c1.R == c2.R && c1.G == c2.G && c1.B == c2.B && c1.A == c2.A;
        }

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="c1">The c1.</param>
        /// <param name="c2">The c2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(FColor c1, FColor c2)
        {
            return !(c1 == c2);
        }

        /// <summary>
        /// Implements the + operator.
        /// </summary>
        /// <param name="c1">The c1.</param>
        /// <param name="c2">The c2.</param>
        /// <returns>The result of the operator.</returns>
        public static FColor operator +(FColor c1, FColor c2)
        {
            return new FColor(
                (byte)Math.Min((int)c1.R + (int)c2.R, 255),
                (byte)Math.Min((int)c1.G + (int)c2.G, 255),
                (byte)Math.Min((int)c1.B + (int)c2.B, 255),
                (byte)Math.Min((int)c1.A + (int)c2.A, 255));
        }


        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is FColor c)
            {
                return this == c;
            }

            return false;
        }

        /// <summary>
        /// Froms the hexadecimal.
        /// </summary>
        /// <param name="hexString">The hexadecimal string.</param>
        /// <returns>FColor.</returns>
        public static FColor FromHex(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
            {
                return new FColor(0, 0, 0, 0);
            }

            int startIndex = hexString[0] == '#' ? 1 : 0;

            if (hexString.Length == 3 + startIndex)
            {
                byte r = Convert.ToByte(hexString.Substring(startIndex, 1), 16);
                byte g = Convert.ToByte(hexString.Substring(startIndex + 1, 1), 16);
                byte b = Convert.ToByte(hexString.Substring(startIndex + 2, 1), 16);

                return new FColor((byte)((r << 4) + r), (byte)((g << 4) + g), (byte)((b << 4) + b));
            }

            if (hexString.Length == 6 + startIndex)
            {
                byte r = Convert.ToByte(hexString.Substring(startIndex, 2), 16);
                byte g = Convert.ToByte(hexString.Substring(startIndex + 2, 2), 16);
                byte b = Convert.ToByte(hexString.Substring(startIndex + 4, 2), 16);

                return new FColor(r, g, b);
            }

            if (hexString.Length == 8 + startIndex)
            {
                byte r = Convert.ToByte(hexString.Substring(startIndex, 2), 16);
                byte g = Convert.ToByte(hexString.Substring(startIndex + 2, 2), 16);
                byte b = Convert.ToByte(hexString.Substring(startIndex + 4, 2), 16);
                byte a = Convert.ToByte(hexString.Substring(startIndex + 6, 2), 16);

                return new FColor(r, g, b, a);
            }

            return new FColor(0, 0, 0, 0);
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("(R={0},G={1},B={2},A={3})", R, G, B, A);
        }

        /// <summary>
        /// Converts to hex.
        /// </summary>
        /// <returns>System.String.</returns>
        public string ToHex()
        {
            return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", R, G, B, A);
        }

        /// <summary>
        /// Converts to linearcolor.
        /// </summary>
        /// <param name="bSRGB">if set to <c>true</c> [b SRGB].</param>
        /// <returns>FLinearColor.</returns>
        public FLinearColor ToLinearColor(bool bSRGB = false)
        {
            float FloatR = R / 255.0f;
            float FloatG = G / 255.0f;
            float FloatB = B / 255.0f;
            float FloatA = A / 255.0f;

            if (bSRGB)
            {
                FloatR = FloatR <= 0.04045f ? FloatR / 12.92f : (float)Math.Pow((FloatR + 0.055f) / 1.055f, 2.4f);
                FloatG = FloatG <= 0.04045f ? FloatG / 12.92f : (float)Math.Pow((FloatG + 0.055f) / 1.055f, 2.4f);
                FloatB = FloatB <= 0.04045f ? FloatB / 12.92f : (float)Math.Pow((FloatB + 0.055f) / 1.055f, 2.4f);
            }

            return new FLinearColor(FloatR, FloatG, FloatB, FloatA);
        }

        #region Global Colors
        /// <summary>
        /// The white
        /// </summary>
        public static readonly FColor White = new FColor(255, 255, 255);
        /// <summary>
        /// The black
        /// </summary>
        public static readonly FColor Black = new FColor(0, 0, 0);
        /// <summary>
        /// The transparent
        /// </summary>
        public static readonly FColor Transparent = new FColor(0, 0, 0, 0);
        /// <summary>
        /// The red
        /// </summary>
        public static readonly FColor Red = new FColor(255, 0, 0);
        /// <summary>
        /// The green
        /// </summary>
        public static readonly FColor Green = new FColor(0, 255, 0);
        /// <summary>
        /// The blue
        /// </summary>
        public static readonly FColor Blue = new FColor(0, 0, 255);
        /// <summary>
        /// The yellow
        /// </summary>
        public static readonly FColor Yellow = new FColor(255, 255, 0);
        /// <summary>
        /// The cyan
        /// </summary>
        public static readonly FColor Cyan = new FColor(0, 255, 255);
        /// <summary>
        /// The magenta
        /// </summary>
        public static readonly FColor Magenta = new FColor(255, 0, 255);
        /// <summary>
        /// The orange
        /// </summary>
        public static readonly FColor Orange = new FColor(243, 156, 18);
        /// <summary>
        /// The purple
        /// </summary>
        public static readonly FColor Purple = new FColor(169, 7, 228);
        /// <summary>
        /// The turquoise
        /// </summary>
        public static readonly FColor Turquoise = new FColor(26, 188, 156);
        /// <summary>
        /// The silver
        /// </summary>
        public static readonly FColor Silver = new FColor(189, 195, 199);
        /// <summary>
        /// The emerald
        /// </summary>
        public static readonly FColor Emerald = new FColor(46, 204, 113);
        #endregion
    }
}
