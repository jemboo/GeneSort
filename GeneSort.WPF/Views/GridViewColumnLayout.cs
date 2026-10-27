using System.Windows.Controls;
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
                double.IsNaN(column.Width) ? null : column.Width)).ToList();
    }

    public void Apply(GridViewColumnCollection columns, string[] keys, TableLayoutSettings? settings)
    {
        Capture();
        if (!ReferenceEquals(this.settings, settings))
        {
            this.settings = settings;
            layout.Clear();
            foreach (var saved in settings?.Columns ?? [])
            {
                var column = CreateColumn(saved.Key);
                column.Width = saved.Width ?? double.NaN;
                layout.Add(column);
            }
            if (!layout.Any(column => column.Header is not GridViewColumnHeader)) layout.Insert(0, CreateColumn(null));
        }
        this.columns = columns;
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
        foreach (var column in layout)
            if (column.Header is not GridViewColumnHeader { Tag: string key } || keys.Contains(key))
                columns.Add(column);
    }

    private GridViewColumn CreateColumn(string? key)
    {
        if (key is null) return new() { Header = "#", Width = 45, DisplayMemberBinding = new Binding(rowNumberProperty) };
        var header = new GridViewColumnHeader { Content = key, Tag = key, ToolTip = $"Sort by {key}" };
        header.Click += (_, _) => sort(key);
        return new() { Header = header, Width = Math.Clamp(key.Length * 7 + 32, 110, 260) };
    }
}
