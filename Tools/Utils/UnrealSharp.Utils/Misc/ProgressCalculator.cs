namespace UnrealSharp.Utils.Misc
{
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
        public static int CalcProgress(int count, int totalCount, int rangeMin, int rangeMax)
        {
            if (totalCount == 0)
            {
                return rangeMax;
            }

            int percent = count * 100 / totalCount;

            int total = rangeMax - rangeMin;
            int value = (int)(((float)percent / 100.0f) * total) + rangeMin;

            return value;
        }

        /// <summary>
        /// Calculates the progress.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="totalCount">The total count.</param>
        /// <returns>System.Int32.</returns>
        public static int CalcProgress(int count, int totalCount)
        {
            return CalcProgress(count, totalCount, 0, 100);
        }
    }
}
