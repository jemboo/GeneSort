
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeneSort.UI.Models;
using System.Collections.ObjectModel;
using System.IO;

namespace GeneSort.UI.ViewModels
{
    public partial class ExperimentViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool canOpenSelectedFile;

        [ObservableProperty]
        private string experimentName;

        [ObservableProperty]
        private string? experimentPath;

        [ObservableProperty]
        private ExperimentDirectoryItem? root;

        [ObservableProperty]
        private ExperimentDirectoryItem? selectedFileItem;

        [ObservableProperty]
        private ObservableCollection<TabViewModel> fileTabs = new();

        [ObservableProperty]
        private TabViewModel? selectedFileTab;

        partial void OnExperimentPathChanged(string? value)
        {
            if (string.IsNullOrEmpty(value)) return;

            Root = LoadDirectory(value);
            FileTabs.Clear();
        }

        [RelayCommand]
        private void OpenSelectedFile()
        {
            if (SelectedFileItem == null || SelectedFileItem.IsDirectory) return;

            var path = SelectedFileItem.FullPath;
            var existingTab = FileTabs.FirstOrDefault(t => t.ContentVm is FileViewerViewModel fv && fv.FilePath == path);
            if (existingTab != null)
            {
                SelectedFileTab = existingTab;
            }
            else
            {
                var fvVm = new FileViewerViewModel { FilePath = path };
                var newTab = new TabViewModel
                {
                    Header = SelectedFileItem.Name,
                    ContentVm = fvVm
                };
                FileTabs.Add(newTab);
                SelectedFileTab = newTab;
            }
        }


        [RelayCommand]
        private void CloseFileTab(TabViewModel tab)
        {
            FileTabs.Remove(tab);
        }

        private ExperimentDirectoryItem LoadDirectory(string path)
        {
            var item = new ExperimentDirectoryItem
            {
                Name = Path.GetFileName(path),
                FullPath = path,
                IsDirectory = true
            };

            try
            {
                // Load subdirectories first, sorted
                var subDirs = Directory.GetDirectories(path).OrderBy(d => Path.GetFileName(d));
                foreach (var subDir in subDirs)
                {
                    item.Children.Add(LoadDirectory(subDir));
                }

                // Then files, sorted
                var files = Directory.GetFiles(path).OrderBy(f => Path.GetFileName(f));
                foreach (var file in files)
                {
                    item.Children.Add(new ExperimentDirectoryItem
                    {
                        Name = Path.GetFileName(file),
                        FullPath = file,
                        IsDirectory = false
                    });
                }
            }
            catch (Exception)
            {
                // Ignore errors like access denied
            }

            return item;
        }
    }
}


