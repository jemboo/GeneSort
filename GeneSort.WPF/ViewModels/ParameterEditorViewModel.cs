using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using GeneSort.Project.V1.Editing;
using GeneSort.Project.V1.Parameters;

namespace GeneSort.WPF.ViewModels;

public sealed class ParameterValue : INotifyPropertyChanged
{
    private string text;
    private readonly string key;
    private readonly parameterDescription? definition;

    public ParameterValue(string key, string text, parameterDescription? definition)
    {
        this.key = key;
        this.text = text;
        this.definition = definition;
    }

    public parameterValueKind Kind => definition?.ValueKind ?? parameterValueKind.Text;
    public string[] Choices => definition?.Choices ?? [];
    public string Text
    {
        get => text;
        set
        {
            if (text == value) return;
            text = value;
            Changed(); Changed(nameof(IsValid)); Changed(nameof(Error));
        }
    }
    public string Error
    {
        get
        {
            var result = ParameterValidation.validateValue(key, Text);
            return result.IsError ? result.ErrorValue : "";
        }
    }
    public bool IsValid => Error.Length == 0;
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Changed([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}

/// A row's identity and editor kind are fixed for its lifetime. Only values can be edited.
public sealed class ParameterRow : INotifyPropertyChanged
{
    private readonly HashSet<ParameterValue> observedValues = [];
    private string inheritedValues = "";
    public string Key { get; }
    public parameterDescription? Definition { get; }
    public string Description => Definition?.Description ?? "Unsupported parameter. Remove it before saving this workflow.";
    public ObservableCollection<ParameterValue> Values { get; } = [];
    public string InheritedValues => inheritedValues;
    public bool HasInheritedValues => inheritedValues.Length > 0;
    public string InputHint
    {
        get
        {
            if (Definition is null) return "Unsupported parameter";
            var minimum = Definition.Minimum?.Value.ToString("G", CultureInfo.InvariantCulture);
            var maximum = Definition.Maximum?.Value.ToString("G", CultureInfo.InvariantCulture);
            return Definition.ValueKind switch
            {
                parameterValueKind.Integer => $"Whole numbers · minimum {minimum}",
                parameterValueKind.Number when maximum is not null => $"Decimal numbers · {minimum} to {maximum}",
                parameterValueKind.Number => $"Decimal numbers · minimum {minimum}",
                parameterValueKind.Choice => "Choose one or more options",
                _ => "One label per field"
            };
        }
    }
    public string Error
    {
        get
        {
            if (Values.Count == 0) return "Enter at least one value.";
            var canonical = new HashSet<string>(StringComparer.Ordinal);
            var errors = new List<string>();
            foreach (var value in Values)
            {
                var result = ParameterValidation.validateValue(Key, value.Text);
                if (result.IsError) errors.Add(result.ErrorValue);
                else if (!canonical.Add(result.ResultValue)) errors.Add($"Value '{value.Text}' is duplicated.");
            }
            return string.Join("\n", errors.Distinct());
        }
    }
    public bool HasErrors => Error.Length > 0;
    public bool CanAddValue => Definition?.ValueKind != parameterValueKind.Choice
        || Definition.Choices.Any(choice => Values.All(value => value.Text != choice));

    public ParameterRow(string key, IEnumerable<string> values)
    {
        Key = key;
        Definition = RunEditor.parameters().FirstOrDefault(spec => spec.Key == key);
        Values.CollectionChanged += (_, _) =>
        {
            foreach (var value in observedValues) value.PropertyChanged -= ValueChanged;
            observedValues.Clear();
            foreach (var value in Values)
                if (observedValues.Add(value)) value.PropertyChanged += ValueChanged;
            ChangedValues();
        };
        foreach (var value in values) Values.Add(new ParameterValue(Key, value, Definition));
    }

    public void AddValue()
    {
        if (!CanAddValue) return;
        var text = Definition?.Choices.FirstOrDefault(choice => Values.All(value => value.Text != choice)) ?? "";
        Values.Add(new ParameterValue(Key, text, Definition));
    }
    public parameterDraft Snapshot() => parameterDraft.create(Key, Values.Select(value => value.Text).ToArray());
    public void SetInheritedValues(string[]? values)
    {
        var display = values is null ? "No setup value" : "Inherited: " + string.Join(", ", values.Select(value => $"“{value}”"));
        if (display == inheritedValues) return;
        inheritedValues = display;
        Changed(nameof(InheritedValues)); Changed(nameof(HasInheritedValues));
    }
    private void ValueChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(ParameterValue.Text)) ChangedValues();
    }
    private void ChangedValues()
    {
        Changed(nameof(Values)); Changed(nameof(Error)); Changed(nameof(HasErrors)); Changed(nameof(CanAddValue));
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Changed([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}

/// The add panel owns an uncommitted row, so selecting a different parameter cannot alter an existing row.
public sealed class ParameterSectionViewModel : INotifyPropertyChanged
{
    private readonly parameterDescription[] allowed;
    private readonly Func<string, string[]?>? inherited;
    private readonly Dictionary<string, string[]> defaults = RunEditor.newDraft().SetupParameters.ToDictionary(row => row.Key, row => row.Values);
    private parameterDescription? selectedParameter;
    private ParameterRow? pendingRow;
    private bool isAdding;

    public string Title { get; }
    public string AddLabel => inherited is null ? "Add parameter" : "Add override";
    public string Help => inherited is null
        ? "Names stay fixed. Edit the values, or remove a parameter to choose another."
        : "Setup values are inherited. Add an override to replace them for execution.";
    public ObservableCollection<ParameterRow> Rows { get; } = [];
    public parameterDescription[] AvailableParameters => allowed.Where(spec => Rows.All(row => row.Key != spec.Key)).ToArray();
    public bool HasAvailableParameters => AvailableParameters.Length > 0;
    public bool IsAdding { get => isAdding; private set { isAdding = value; Changed(); } }
    public parameterDescription? SelectedParameter
    {
        get => selectedParameter;
        set
        {
            var selection = AvailableParameters.FirstOrDefault(spec => spec.Key == value?.Key);
            if (ReferenceEquals(selectedParameter, selection)) return;
            selectedParameter = selection;
            SetPending(selection is null ? null : new ParameterRow(selection.Key,
                inherited?.Invoke(selection.Key) ?? defaults.GetValueOrDefault(selection.Key) ?? [""]));
            Changed();
        }
    }
    public ParameterRow? PendingRow => pendingRow;
    public bool CanAdd => IsAdding && PendingRow is { HasErrors: false }
        && AvailableParameters.Any(spec => spec.Key == PendingRow.Key);

    public ParameterSectionViewModel(string title, parameterDescription[] allowed, Func<string, string[]?>? inherited = null)
    {
        Title = title;
        this.allowed = allowed;
        this.inherited = inherited;
        Rows.CollectionChanged += (_, _) =>
        {
            Changed(nameof(AvailableParameters)); Changed(nameof(HasAvailableParameters)); Changed(nameof(CanAdd));
        };
    }
    public void BeginAdd()
    {
        if (!HasAvailableParameters) return;
        CancelAdd();
        IsAdding = true;
        Changed(nameof(CanAdd));
    }
    public void CancelAdd()
    {
        SelectedParameter = null;
        SetPending(null);
        IsAdding = false;
        Changed(nameof(CanAdd));
    }
    public bool Add()
    {
        if (!CanAdd) return false;
        var row = PendingRow!;
        CancelAdd();
        Rows.Add(row);
        return true;
    }
    public void RefreshInheritance()
    {
        if (inherited is null) return;
        foreach (var row in Rows) row.SetInheritedValues(inherited(row.Key));
        PendingRow?.SetInheritedValues(inherited(PendingRow.Key));
    }
    private void SetPending(ParameterRow? row)
    {
        if (pendingRow is not null) pendingRow.PropertyChanged -= PendingChanged;
        pendingRow = row;
        if (pendingRow is not null) pendingRow.PropertyChanged += PendingChanged;
        RefreshInheritance();
        Changed(nameof(PendingRow)); Changed(nameof(CanAdd));
    }
    private void PendingChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(ParameterRow.Values)) Changed(nameof(CanAdd));
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Changed([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}
