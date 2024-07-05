namespace UnrealSharp.Utils.Misc;

/// <summary>
/// Class ProgressCalculator.
/// Tool class used to quickly calculate the current progress as a percentage[0 - 100]
/// </summary>
public static class ProgressCalculator
{
    /// <summary>
    /// Calculates the progress.
    /// </summary>
    /// <param name="count">The count.</param>
    /// <param name="totalCount">The total count.</param>
    /// <param name="rangeMin">The range minimum.</param>
    /// <param name="rangeMax">The range maximum.</param>
    /// <returns>System.Int32.</returns>
    public static int CalcProgress(int count, int totalCount, int rangeMin = 0, int rangeMax = 100)
    {
        if (totalCount == 0)
        {
            return rangeMax;
        }

        var percent = count * 100 / totalCount;

        var total = rangeMax - rangeMin;
        var value = (int)(percent / 100.0f * total) + rangeMin;

        return value;
    }
}