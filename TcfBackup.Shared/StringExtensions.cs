namespace TcfBackup.Shared
{
    public static class StringExtensions
    {
        private static readonly (long Unit, string Suffix)[] s_unitsSuffix = {
            (1L << 10, "KB"),
            (1L << 20, "MB"),
            (1L << 30, "GB"),
            (1L << 40, "TB"),
        };

        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        public static string GenerateRandomString(int length) => new (Enumerable.Repeat(Chars, length).Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
        
        public static string FormatBytes(double bytes, string? specSuffix = null)
        {
            foreach (var (unit, suffix) in s_unitsSuffix.OrderByDescending(us => us.Unit))
            {
                if (bytes >= unit)
                {
                    return $"{bytes / unit:0.00} {suffix + specSuffix}";
                }
            }

            return $"{bytes:0.00} {"B" + specSuffix}";
        }
    }
}