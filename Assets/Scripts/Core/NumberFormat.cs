namespace TapMinies.Core
{
    /// <summary>
    /// Idle games reach absurd numbers quickly; raw digits stop being readable
    /// around six figures. Short-form keeps values scannable at a glance.
    /// </summary>
    public static class NumberFormat
    {
        private static readonly string[] Suffixes = { "", "K", "M", "B", "T", "aa", "ab", "ac" };

        public static string Short(long value)
        {
            if (value < 1000) return value.ToString();

            double v = value;
            int tier = 0;
            while (v >= 1000d && tier < Suffixes.Length - 1)
            {
                v /= 1000d;
                tier++;
            }

            // Keep one decimal below 100 so 1.2K reads differently from 1.9K,
            // then drop it once the integer part alone carries the magnitude.
            return v < 100d
                ? $"{v:0.#}{Suffixes[tier]}"
                : $"{v:0}{Suffixes[tier]}";
        }
    }
}
