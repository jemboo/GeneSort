using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using GeneSort.Project.V1.Parameters;
using RunParameters = GeneSort.Project.V1.Parameters.parameterSet<GeneSort.Project.V1.Parameters.runParameterKind>;

namespace GeneSort.WPF.ViewModels;

public sealed class RunParameterItem
{
    public int Number { get; }
    public IReadOnlyDictionary<string, string> Values { get; }
    public string?[] Cells { get; }

    internal RunParameterItem(int number, RunParameters parameters, string[] keys)
    {
        Number = number;
        Values = parameters.ParamMap.ToDictionary(pair => pair.Key, pair => pair.Value);
        Cells = keys.Select(key => Values.GetValueOrDefault(key)).ToArray();
    }
}

/// A reusable, read-only collection view; sorting never changes the saved parameters.
public sealed class RunParameterListViewModel : INotifyPropertyChanged
{
    public string[] Keys { get; private set; } = [];
    public ListCollectionView Items { get; private set; } = new(new List<RunParameterItem>());
    public string Summary { get; private set; } = "Generate parameters or refresh saved files to view this run.";
    public string? SortKey { get; private set; }
    public bool Descending { get; private set; }
    public int Count => Items.Count;
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Changed(string property) => PropertyChanged?.Invoke(this, new(property));

    public void Clear(string message = "Refresh saved files to view this run.", bool preserveSort = false)
    {
        Keys = [];
        Items = new(new List<RunParameterItem>());
        Summary = message;
        if (!preserveSort) { SortKey = null; Descending = false; }
        NotifyRows();
    }

    public void SetParameters(IEnumerable<RunParameters> parameters)
    {
        var data = parameters.ToArray();
        string[] leading = [ParamKeys.replKey, ParamKeys.mutationModKey, ParamKeys.sourceMutationModKey,
            ParamKeys.partitionCountKey, ParamKeys.generationCurrentKey, ParamKeys.runNameKey];
        var keys = data.SelectMany(row => row.ParamMap.Keys).Distinct(StringComparer.Ordinal).ToArray();
        Keys = leading.Where(keys.Contains).Concat(keys.Except(leading).Order(StringComparer.Ordinal)).ToArray();
        Items = new(data.Select((row, index) => new RunParameterItem(index + 1, row, Keys)).ToList());
        var replicas = data.Select(row => row.GetRepl()?.Value).Distinct().Count();
        Summary = data.Length == 0 ? "No saved RunParameter files for this run."
            : $"{data.Length:N0} saved RunParameters across {replicas:N0} replica(s). Click a column heading to sort; click again to reverse.";
        var previousKey = SortKey;
        var previousDirection = Descending;
        SortKey = null;
        Descending = false;
        NotifyRows();
        if (Keys.Length > 0)
        {
            var key = previousKey is not null && Keys.Contains(previousKey) ? previousKey : Keys[0];
            ApplySort(key, key == previousKey && previousDirection);
        }
    }

    private void NotifyRows()
    {
        foreach (var property in new[] { nameof(Keys), nameof(Items), nameof(Count), nameof(Summary), nameof(SortKey), nameof(Descending) })
            Changed(property);
    }

    public void Sort(string key)
    {
        if (Keys.Contains(key)) ApplySort(key, SortKey == key && !Descending);
    }

    private void ApplySort(string key, bool descending)
    {
        var index = Array.IndexOf(Keys, key);
        var values = Items.SourceCollection.Cast<RunParameterItem>().Select(row => row.Cells[index])
            .Where(value => !string.IsNullOrEmpty(value)).ToArray();
        // Infer one comparison type for the entire column to keep ordering transitive.
        // Decimal preserves large integer seeds; double also handles very small/large exponents.
        var decimals = values.All(value => decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            && (number != 0 || double.Parse(value!, CultureInfo.InvariantCulture) == 0));
        var numbers = !decimals && values.All(value => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) && double.IsFinite(number));
        Items.CustomSort = new ValueComparer(index, descending, decimals, numbers);
        SortKey = key;
        Descending = descending;
        Changed(nameof(SortKey)); Changed(nameof(Descending));
    }

    private sealed class ValueComparer(int index, bool descending, bool decimals, bool numbers) : IComparer
    {
        public int Compare(object? x, object? y)
        {
            var left = (RunParameterItem)x!;
            var right = (RunParameterItem)y!;
            var a = left.Cells[index];
            var b = right.Cells[index];
            var missingA = string.IsNullOrEmpty(a);
            var missingB = string.IsNullOrEmpty(b);
            if (missingA != missingB) return missingA ? 1 : -1; // Missing values stay last in both directions.
            var result = missingA ? 0 : decimals
                ? decimal.Parse(a!, NumberStyles.Float, CultureInfo.InvariantCulture).CompareTo(decimal.Parse(b!, NumberStyles.Float, CultureInfo.InvariantCulture))
                : numbers ? double.Parse(a!, CultureInfo.InvariantCulture).CompareTo(double.Parse(b!, CultureInfo.InvariantCulture))
                : StringComparer.OrdinalIgnoreCase.Compare(a, b);
            return result == 0 ? left.Number.CompareTo(right.Number) : descending ? -Math.Sign(result) : Math.Sign(result);
        }
    }
}
