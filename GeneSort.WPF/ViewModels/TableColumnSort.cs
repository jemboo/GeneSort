using System.Collections;
using System.Globalization;

namespace GeneSort.WPF.ViewModels;

/// One comparison type per column keeps sorting transitive; unavailable values stay last.
internal static class TableColumnSort
{
    public static IComparer Create<T>(IEnumerable<T> rows, Func<T, string?> value, Func<T, int> number, bool descending)
    {
        var values = rows.Select(value).Where(cell => !Missing(cell)).ToArray();
        var decimals = values.All(cell => decimal.TryParse(cell, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            && (parsed != 0 || double.Parse(cell!, CultureInfo.InvariantCulture) == 0));
        var numbers = !decimals && values.All(cell => double.TryParse(cell, NumberStyles.Float, CultureInfo.InvariantCulture, out _));
        return new Comparer<T>(value, number, descending, decimals, numbers);
    }

    private static bool Missing(string? value) => string.IsNullOrEmpty(value) || string.Equals(value, "NaN", StringComparison.OrdinalIgnoreCase);

    private sealed class Comparer<T>(Func<T, string?> value, Func<T, int> number, bool descending, bool decimals, bool numbers) : IComparer
    {
        public int Compare(object? x, object? y)
        {
            var left = (T)x!;
            var right = (T)y!;
            var a = value(left);
            var b = value(right);
            var missingA = Missing(a);
            var missingB = Missing(b);
            if (missingA != missingB) return missingA ? 1 : -1;
            var result = missingA ? 0 : decimals
                ? decimal.Parse(a!, NumberStyles.Float, CultureInfo.InvariantCulture).CompareTo(decimal.Parse(b!, NumberStyles.Float, CultureInfo.InvariantCulture))
                : numbers ? double.Parse(a!, CultureInfo.InvariantCulture).CompareTo(double.Parse(b!, CultureInfo.InvariantCulture))
                : StringComparer.OrdinalIgnoreCase.Compare(a, b);
            return result == 0 ? number(left).CompareTo(number(right)) : descending ? -Math.Sign(result) : Math.Sign(result);
        }
    }
}
