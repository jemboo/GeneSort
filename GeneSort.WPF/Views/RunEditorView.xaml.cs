using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using GeneSort.FileDb.V1;
using GeneSort.WPF.ViewModels;

namespace GeneSort.WPF.Views;

public partial class RunEditorView : UserControl
{
    private RunEditorViewModel Model => (RunEditorViewModel)DataContext;
    public RunEditorView() => InitializeComponent();
    private bool DiscardChanges() => !Model.IsDirty || MessageBox.Show(Window.GetWindow(this),
        "Discard the unsaved run definition changes?", "Run editor", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    private void New_Click(object sender, RoutedEventArgs e) { if (DiscardChanges()) Model.New(); }
    private void Validate_Click(object sender, RoutedEventArgs e) => Model.Validate();
    private void AddSetup_Click(object sender, RoutedEventArgs e) => Model.SetupParameters.Add(new());
    private void AddExecution_Click(object sender, RoutedEventArgs e) => Model.ExecutionParameters.Add(new());
    private void RemoveSetup_Click(object sender, RoutedEventArgs e) { if (SetupGrid.SelectedItem is ParameterRow row) Model.SetupParameters.Remove(row); }
    private void RemoveExecution_Click(object sender, RoutedEventArgs e) { if (ExecutionGrid.SelectedItem is ParameterRow row) Model.ExecutionParameters.Remove(row); }
    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Choose the folder containing your GeneSort projects" };
        if (dialog.ShowDialog(Window.GetWindow(this)) == true) Model.ProjectsFolder = dialog.FolderName;
    }
    private async void Open_Click(object sender, RoutedEventArgs e)
    {
        if (!DiscardChanges()) return;
        var dialog = new OpenFileDialog { Title = "Open a saved run definition", Filter = "Run definitions (Run_*.msgpack)|Run_*.msgpack|MessagePack files|*.msgpack" };
        if (dialog.ShowDialog(Window.GetWindow(this)) != true) return;
        await PerformAsync(() => Model.OpenAsync(dialog.FileName));
    }
    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        var configured = Model.Validate();
        if (configured is null) return;
        await PerformAsync(async () =>
        {
            var path = RunDefinitionFiles.pathFor(Model.ProjectsFolder, configured);
            var overwrite = File.Exists(path);
            if (overwrite && MessageBox.Show(Window.GetWindow(this), $"Replace this run definition?\n\n{path}",
                    "Save run", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            await Model.SaveAsync(configured, overwrite);
        });
    }
    private async Task PerformAsync(Func<Task> action)
    {
        Model.IsBusy = true;
        try { await action(); }
        catch (Exception error)
        {
            Model.Status = $"Could not complete the operation: {error.GetBaseException().Message}";
        }
        finally { Model.IsBusy = false; }
    }
}
