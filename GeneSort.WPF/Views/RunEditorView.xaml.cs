using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using GeneSort.FileDb.V1;
using GeneSort.WPF.ViewModels;

namespace GeneSort.WPF.Views;

public partial class RunEditorView : UserControl
{
    private RunEditorViewModel Model => (RunEditorViewModel)DataContext;
    public RunEditorView()
    {
        InitializeComponent();
        RunSelection.RunChosen += async tile =>
        {
            if (!DiscardChanges()) return;
            RunSelection.IsEnabled = false;
            try { await Model.ChooseRunAsync(tile); }
            catch (Exception error) { MessageDialog.Show(Window.GetWindow(this), error.GetBaseException().Message, "Open run", MessageBoxButton.OK, MessageBoxImage.Error); }
            finally { RunSelection.IsEnabled = true; }
        };
        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is INotifyPropertyChanged oldModel)
                PropertyChangedEventManager.RemoveHandler(oldModel, SetupAvailabilityChanged, nameof(RunEditorViewModel.HasSetup));
            if (e.NewValue is INotifyPropertyChanged newModel)
                PropertyChangedEventManager.AddHandler(newModel, SetupAvailabilityChanged, nameof(RunEditorViewModel.HasSetup));
        };
    }
    private void SetupAvailabilityChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!Model.HasSetup && SetupParametersTab.IsSelected)
            RunParametersTab.IsSelected = true;
    }
    private bool DiscardChanges() => !Model.HasUnsavedChanges || MessageDialog.Show(Window.GetWindow(this),
        "Discard the unsaved run definition changes?", "Run editor", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    private async void ChooseRun_Click(object sender, RoutedEventArgs e) => await Model.BeginSelectionAsync();
    private void ReturnToEditor_Click(object sender, RoutedEventArgs e) => Model.CancelSelection();

    private void New_Click(object sender, RoutedEventArgs e) { if (DiscardChanges()) Model.New(); }
    private void Clone_Click(object sender, RoutedEventArgs e) => Model.Clone();
    private void Validate_Click(object sender, RoutedEventArgs e) => Model.Validate();
    private void CopyErrors_Click(object sender, RoutedEventArgs e)
    {
        if (!Model.HasErrors) return;
        try { Clipboard.SetText(Model.ErrorMessages); }
        catch (System.Runtime.InteropServices.ExternalException)
        {
            Model.Status = "The clipboard is busy. Try Copy errors again, or select the error text and press Ctrl+C.";
        }
    }
    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Choose the folder containing your Sortopia projects" };
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
        if (!Model.CanSave) return;
        var configured = Model.Validate();
        if (configured is null) return;
        await PerformAsync(async () =>
        {
            var path = RunDefinitionFiles.pathFor(Model.ProjectsFolder, configured);
            var overwrite = File.Exists(path);
            if (overwrite && MessageDialog.Show(Window.GetWindow(this), $"Replace this run definition?\n\n{path}",
                    "Save run", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            await Model.SaveAsync(configured, overwrite);
        });
    }
    private async void RefreshParameters_Click(object sender, RoutedEventArgs e)
    {
        await PerformAsync(() => Model.LoadRunParametersAsync());
    }
    private void CancelPhase_Click(object sender, RoutedEventArgs e) => Model.CancelPhase();

    private async void RunSetup_Click(object sender, RoutedEventArgs e)
    {
        if (Model.CanRunSetup) await Model.RunSetupAsync();
    }
    private async void RunExecution_Click(object sender, RoutedEventArgs e)
    {
        if (Model.CanRunExecution) await Model.RunExecutionAsync();
    }
    private async void GenerateSummaryReports_Click(object sender, RoutedEventArgs e)
    {
        if (Model.CanGenerateSummaryReports) await Model.GenerateSummaryReportsAsync();
    }
    private async void RefreshReports_Click(object sender, RoutedEventArgs e)
    {
        if (Model.CanRefreshSummaryReports) await Model.LoadSummaryReportsAsync();
    }
    private async void Generate_Click(object sender, RoutedEventArgs e)
    {
        await PerformAsync(async () =>
        {
            var prepared = await Model.PrepareGenerationAsync();
            if (prepared is null) return;
            var replace = prepared.RequiresReplacement;
            if (replace && MessageDialog.Show(Window.GetWindow(this),
                    $"Save this definition and replace all {prepared.ExistingFileCount:N0} existing RunParameter files for '{prepared.Run.RunName}' with the {prepared.FileCount:N0} prepared files?\n\n" +
                    $"Old parameter files, including replicas outside the new range, will be removed.\n\n{prepared.ParameterFolder}",
                    "Generate RunParameters", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                Model.Status = "Generation cancelled. Existing files were kept.";
                return;
            }
            await Model.GenerateParametersAsync(prepared, replace);
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
