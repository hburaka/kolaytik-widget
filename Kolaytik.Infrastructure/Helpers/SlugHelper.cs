using System.Text.RegularExpressions;

namespace Kolaytik.Infrastructure.Helpers;

internal static partial class SlugHelper
{
    internal static string ToSlug(string name)
    {
        var s = name.ToLowerInvariant();
        s = s.Replace('ı', 'i').Replace('ğ', 'g').Replace('ü', 'u')
             .Replace('ş', 's').Replace('ö', 'o').Replace('ç', 'c')
             .Replace('İ', 'i').Replace('Ğ', 'g').Replace('Ü', 'u')
             .Replace('Ş', 's').Replace('Ö', 'o').Replace('Ç', 'c');
        s = NonAlphanumericRegex().Replace(s, "");
        s = MultiSpaceRegex().Replace(s, "-");
        s = MultiDashRegex().Replace(s, "-").Trim('-');
        return s.Length > 80 ? s[..80].TrimEnd('-') : s;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultiSpaceRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultiDashRegex();
}
