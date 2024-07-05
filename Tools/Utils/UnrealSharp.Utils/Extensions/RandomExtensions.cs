using System.Drawing;

namespace UnrealSharp.Utils.Extensions;

/// <summary>
/// Class RandomExtensions.
/// </summary>
public static class RandomExtensions
{
    /// <summary>
    /// Gets random <c>float</c> number within range.
    /// </summary>
    /// <param name="random">Current <see cref="System.Random"/>.</param>
    /// <param name="min">Minimum.</param>
    /// <param name="max">Maximum.</param>
    /// <returns>Random <c>float</c> number.</returns>
    public static float NextFloat(this Random random, float min, float max)
    {
        return Lerp(min, max, (float)random.NextDouble());
    }

    /// <summary>
    /// Gets random <c>double</c> number within range.
    /// </summary>
    /// <param name="random">Current <see cref="System.Random"/>.</param>
    /// <param name="min">Minimum.</param>
    /// <param name="max">Maximum.</param>
    /// <returns>Random <c>double</c> number.</returns>
    public static double NextDouble(this Random random, double min, double max)
    {
        return Lerp(min, max, random.NextDouble());
    }

    private static float Lerp(float from, float to, float amount)
    {
        return (1 - amount) * from + amount * to;
    }


    private static double Lerp(double from, double to, double amount)
    {
        return (1 - amount) * from + amount * to;
    }

    /// <summary>
    /// Gets random <c>long</c> number.
    /// </summary>
    /// <param name="random">Current <see cref="System.Random"/>.</param>
    /// <returns>Random <c>long</c> number.</returns>
    public static long NextLong(this Random random)
    {
        var buffer = new byte[sizeof(long)];
        random.NextBytes(buffer);
        return BitConverter.ToInt64(buffer, 0);
    }

    /// <summary>
    /// Gets random <c>long</c> number within range.
    /// </summary>
    /// <param name="random">Current <see cref="System.Random"/>.</param>
    /// <param name="min">Minimum.</param>
    /// <param name="max">Maximum.</param>
    /// <returns>Random <c>long</c> number.</returns>
    public static long NextLong(this Random random, long min, long max)
    {
        var buf = new byte[sizeof(long)];
        random.NextBytes(buf);
        var longRand = BitConverter.ToInt64(buf, 0);

        return Math.Abs(longRand % (max - min + 1)) + min;
    }

    /// <summary>
    /// Converts to byte.
    /// </summary>
    /// <param name="component">The component.</param>
    /// <returns>System.Byte.</returns>
    public static byte ToByte(float component)
    {
        var value = (int)(component * 255.0f);
        return ToByte(value);
    }

    /// <summary>
    /// Converts to byte.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>System.Byte.</returns>
    public static byte ToByte(int value)
    {
        return (byte)(value < 0 ? 0 : value > 255 ? 255 : value);
    }

    // ReSharper disable once InconsistentNaming
    private static Color FromFloatRGBA(float red, float green, float blue, float alpha = 1.0f)
    {
        return Color.FromArgb(ToByte(alpha), ToByte(red), ToByte(green), ToByte(blue));
    }

    /// <summary>
    /// Gets random opaque <see cref="Color"/>.
    /// </summary>
    /// <param name="random">Current <see cref="System.Random"/>.</param>
    /// <returns>Random <see cref="Color"/>.</returns>
    public static Color NextColor(this Random random)
    {
        return FromFloatRGBA(red: random.NextFloat(0.0f, 1.0f), green: random.NextFloat(0.0f, 1.0f), blue: random.NextFloat(0.0f, 1.0f), alpha: 1.0f);
    }

    /// <summary>
    /// Gets random opaque <see cref="Color"/>.
    /// </summary>
    /// <param name="random">Current <see cref="System.Random"/>.</param>
    /// <param name="minBrightness">Minimum brightness.</param>
    /// <param name="maxBrightness">Maximum brightness</param>
    /// <returns>Random <see cref="Color"/>.</returns>
    public static Color NextColor(this Random random, float minBrightness, float maxBrightness)
    {
        return FromFloatRGBA(red: random.NextFloat(minBrightness, maxBrightness), green: random.NextFloat(minBrightness, maxBrightness), blue: random.NextFloat(minBrightness, maxBrightness), alpha: 1.0f);
    }

    /// <summary>
    /// Gets random <see cref="Color"/>.
    /// </summary>
    /// <param name="random">Current <see cref="System.Random"/>.</param>   
    /// <param name="minBrightness">Minimum brightness.</param>
    /// <param name="maxBrightness">Maximum brightness</param>
    /// <param name="alpha">Alpha value.</param>
    /// <returns>Random <see cref="Color"/>.</returns>
    public static Color NextColor(this Random random, float minBrightness, float maxBrightness, float alpha)
    {
        return FromFloatRGBA(random.NextFloat(minBrightness, maxBrightness), random.NextFloat(minBrightness, maxBrightness), random.NextFloat(minBrightness, maxBrightness), alpha);
    }

    /// <summary>
    /// Gets random <see cref="Color"/>.
    /// </summary>
    /// <param name="random">Current <see cref="System.Random"/>.</param>
    /// <param name="minBrightness">Minimum brightness.</param>
    /// <param name="maxBrightness">Maximum brightness</param>
    /// <param name="minAlpha">Minimum alpha.</param>
    /// <param name="maxAlpha">Maximum alpha.</param>
    /// <returns>Random <see cref="Color"/>.</returns>
    public static Color NextColor(this Random random, float minBrightness, float maxBrightness, float minAlpha, float maxAlpha)
    {
        return FromFloatRGBA(random.NextFloat(minBrightness, maxBrightness), random.NextFloat(minBrightness, maxBrightness), random.NextFloat(minBrightness, maxBrightness), random.NextFloat(minAlpha, maxAlpha));
    }

    /// <summary>
    /// Gets random <see cref="Point"/>.
    /// </summary>
    /// <param name="random">Current <see cref="System.Random"/>.</param>
    /// <param name="min">Minimum.</param>
    /// <param name="max">Maximum.</param>
    /// <returns>Random <see cref="Point"/>.</returns>
    public static Point NextPoint(this Random random, Point min, Point max)
    {
        return new Point(random.Next(min.X, max.X), random.Next(min.Y, max.Y));
    }

    /// <summary>
    /// Gets random <see cref="System.TimeSpan"/>.
    /// </summary>
    /// <param name="random">Current <see cref="System.Random"/>.</param>
    /// <param name="min">Minimum.</param>
    /// <param name="max">Maximum.</param>
    /// <returns>Random <see cref="System.TimeSpan"/>.</returns>
    public static TimeSpan NextTime(this Random random, TimeSpan min, TimeSpan max)
    {
        return TimeSpan.FromTicks(random.NextLong(min.Ticks, max.Ticks));
    }
}