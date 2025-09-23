
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeneSort.UI.Models;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace GeneSort.UI.ViewModels
{
    public partial class ExperimentSelectionVm : ObservableObject
    {
        [ObservableProperty]
        private string? rootFolder;

        [ObservableProperty]
        private ObservableCollection<ExperimentInfoViewModel>? experiments = new();

        [ObservableProperty]
        private ExperimentInfoViewModel? selectedExperiment;

        partial void OnRootFolderChanged(string? value)
        {
            LoadExperiments();
        }

        private void LoadExperiments()
        {
            Experiments?.Clear();
            if (string.IsNullOrEmpty(RootFolder))
            {
                return;
            }

            try
            {
                var directories = Directory.GetDirectories(RootFolder);
                foreach (var dir in directories.OrderBy(d => Path.GetFileName(d)))
                {
                    Experiments.Add(new ExperimentInfoViewModel
                    {
                        Name = Path.GetFileName(dir),
                        FullPath = dir,
                        Description = string.Empty // TODO: Load from metadata if available
                    });
                }
            }

            catch (Exception)
            {
                // Handle directory access errors if needed
            }
        }

        [RelayCommand]
        private void BrowseProjectFolder()
        {
            var dialog = new VistaFolderBrowserDialog();

            dialog.UseDescriptionForTitle = true;
            dialog.Description = "Select Project folder";

            if (dialog.ShowDialog() == true)
            {
                RootFolder = dialog.SelectedPath;
            }
        }
    }
}