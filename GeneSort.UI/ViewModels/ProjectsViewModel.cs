using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeneSort.UI.Models;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.IO;

namespace GeneSort.UI.ViewModels
{
    public partial class ProjectsViewModel : ObservableObject
    {
        [ObservableProperty]
        private ProjectModel? project = new();

        [ObservableProperty]
        private string? projectFolder;

        [ObservableProperty]
        private ObservableCollection<string>? experiments = new();

        [ObservableProperty]
        private string? selectedExperiment;

        partial void OnProjectFolderChanged(string? value)
        {
            LoadExperiments();
        }

        private void LoadExperiments()
        {
            Experiments?.Clear();
            if (string.IsNullOrEmpty(ProjectFolder))
            {
                return;
            }

            try
            {
                var directories = Directory.GetDirectories(ProjectFolder);
                foreach (var dir in directories)
                {
                    Experiments.Add(Path.GetFileName(dir));
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
                project.ProjectFolder = dialog.SelectedPath;
                ProjectFolder = dialog.SelectedPath;
            }
        }
    }
}