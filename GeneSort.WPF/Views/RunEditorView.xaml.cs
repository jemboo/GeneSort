using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
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
    private async void RefreshParameters_Click(object sender, RoutedEventArgs e)
    {
        await PerformAsync(() => Model.LoadRunParametersAsync());
    }
    private void SelectCompleted_Click(object sender, RoutedEventArgs e)
    {
        if (Model.IsReady) ProcessingParameters.SelectCompleted();
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
            await Model.GenerateParametersAsync(prepared, false);
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
