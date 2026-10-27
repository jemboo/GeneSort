using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using GeneSort.WPF.Settings;

namespace GeneSort.WPF.Views;

/// Retains the user's column layout while the table's rows and available fields change.
internal sealed class GridViewColumnLayout(string rowNumberProperty, Action<string> sort)
{
    // Keep the columns themselves so widths (including automatic sizing) survive reloads.
    private readonly List<GridViewColumn> layout =
        [new() { Header = "#", Width = 45, DisplayMemberBinding = new Binding(rowNumberProperty) }];
    private GridViewColumnCollection? columns;
    private TableLayoutSettings? settings;
    private readonly HashSet<GridViewColumn> hidden = [];
    private string[] keys = [];
    private ContextMenu? chooser;

    public void Reset()
    {
        CloseChooser();
        columns?.Clear();
        layout.Clear();
        hidden.Clear();
        layout.Add(CreateColumn(null));
        if (settings is not null) settings.Columns.Clear();
    }

    public void Capture()
    {
        if (columns is null) return;
        // Update only the visible slots; temporarily absent columns retain their places.
        var visible = columns.ToHashSet();
        var order = new Queue<GridViewColumn>(columns);
        for (var index = 0; index < layout.Count; index++)
            if (visible.Contains(layout[index])) layout[index] = order.Dequeue();

        if (settings is not null)
            settings.Columns = layout.Select(column => new ColumnLayoutSetting(
                (column.Header as GridViewColumnHeader)?.Tag as string,
                double.IsNaN(column.Width) ? null : column.Width, hidden.Contains(column))).ToList();
    }

    public void Apply(GridViewColumnCollection columns, string[] keys, TableLayoutSettings? settings)
    {
        CloseChooser();
        Capture();
        if (!ReferenceEquals(this.settings, settings))
        {
            this.settings = settings;
            layout.Clear();
            hidden.Clear();
            foreach (var saved in settings?.Columns ?? [])
            {
                var column = CreateColumn(saved.Key);
                column.Width = saved.Width ?? double.NaN;
                layout.Add(column);
                if (saved.Hidden) hidden.Add(column);
            }
            if (!layout.Any(column => column.Header is not GridViewColumnHeader)) layout.Insert(0, CreateColumn(null));
        }
        this.columns = columns;
        this.keys = keys;
        columns.Clear();
        for (var index = 0; index < keys.Length; index++)
        {
            var key = keys[index];
            var column = layout.Find(column => column.Header is GridViewColumnHeader header && Equals(header.Tag, key));
            if (column is null)
            {
                column = CreateColumn(key);
                layout.Add(column);
            }
            // The backing schema can change independently of the displayed column order.
            column.DisplayMemberBinding = new Binding($"Cells[{index}]");
        }
        ShowVisibleColumns();
    }

    private bool IsAvailable(GridViewColumn column) =>
        column.Header is not GridViewColumnHeader { Tag: string key } || keys.Contains(key);

    private void ShowVisibleColumns()
    {
        if (columns is null) return;
        columns.Clear();
        foreach (var column in layout.Where(column => IsAvailable(column) && !hidden.Contains(column)))
            columns.Add(column);
    }

    public void CloseChooser()
    {
        if (chooser is null) return;
        chooser.IsOpen = false;
        if (chooser.PlacementTarget is Button button && button.ContextMenu == chooser) button.ContextMenu = null;
        chooser = null;
    }

    public void OpenChooser(Button button, Action changed)
    {
        CloseChooser();
        Capture();
        chooser = new ContextMenu { PlacementTarget = button, Placement = PlacementMode.Bottom, MinWidth = 200, MaxHeight = 420 };
        ScrollViewer.SetVerticalScrollBarVisibility(chooser, ScrollBarVisibility.Auto);
        foreach (var column in layout.Where(IsAvailable))
        {
            var key = (column.Header as GridViewColumnHeader)?.Tag as string;
            var item = new MenuItem { Header = key ?? "#", Tag = key, IsCheckable = true, IsChecked = !hidden.Contains(column), StaysOpenOnClick = true };
            item.Click += (_, _) =>
            {
                Capture();
                if (item.IsChecked) hidden.Remove(column); else hidden.Add(column);
                ShowVisibleColumns();
                Capture();
                changed();
            };
            chooser.Items.Add(item);
        }
        button.ContextMenu = chooser;
        chooser.IsOpen = true;
    }

    private GridViewColumn CreateColumn(string? key)
    {
        if (key is null) return new() { Header = "#", Width = 45, DisplayMemberBinding = new Binding(rowNumberProperty) };
        var header = new GridViewColumnHeader { Content = key, Tag = key, ToolTip = $"Sort by {key}" };
        header.Click += (_, _) => sort(key);
        return new() { Header = header, Width = Math.Clamp(key.Length * 7 + 32, 110, 260) };
    }
}
