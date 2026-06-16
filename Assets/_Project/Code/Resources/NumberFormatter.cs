namespace Project.Resources
{
    public static class NumberFormatter
    {
        private const int Decimals = 1;
        private static readonly string[] s_suffixes = { "", "k", "m", "b", "t" };

        public static string Abbreviate(this float value)
        {
            var tier = 0;
            var shrunkenBeast = value;

            while (shrunkenBeast >= 1000f && tier < s_suffixes.Length - 1)
            {
                shrunkenBeast /= 1000f;
                tier++;
            }

            if (tier == 0) return shrunkenBeast.ToString("f0");

            return shrunkenBeast.ToString("f" + Decimals) + s_suffixes[tier];
        }
    }
}
