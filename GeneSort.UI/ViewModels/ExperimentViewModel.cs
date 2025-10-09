
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

        [ObservableProperty]
        public Action<object> selectionAction;


        public ExperimentViewModel()
        {
            selectionAction = _selectionAction;
        }   

        private Action<object> _selectionAction
        {
            get
            {
                return obj =>
                {
                    SelectedFileItem = obj as ExperimentDirectoryItem;
                    CanOpenSelectedFile = SelectedFileItem != null && !SelectedFileItem.IsDirectory;
                };
            }
        }


        partial void OnExperimentPathChanged(string? value)
        {
            if (string.IsNullOrEmpty(value)) return;

            Root = LoadDirectory(value);
            FileTabs.Clear();
        }


        [RelayCommand]
        private async Task OpenSelectedFile()
        {
            if (SelectedFileItem == null || SelectedFileItem.IsDirectory) return;

            var path = SelectedFileItem.FullPath;
            var fileName = SelectedFileItem.Name;

            // Check if tab already exists
            var existingTab = FileTabs.FirstOrDefault(t =>
                (t.ContentVm is FileViewerViewModel fv && fv.FilePath == path) ||
                (t.ContentVm is WorkspaceParamsVm wv && wv.FilePath == path));

            if (existingTab != null)
            {
                SelectedFileTab = existingTab;
                return;
            }

            // Determine if this is a WorkspaceDto file based on extension or content
            // You might want to adjust this logic based on your file naming convention
            if (IsWorkspaceFile(path, fileName))
            {
                // Create WorkspaceView tab
                var workspaceVm = new WorkspaceParamsVm();
                var newTab = new TabViewModel
                {
                    Header = fileName,
                    ContentVm = workspaceVm
                };

                FileTabs.Add(newTab);
                SelectedFileTab = newTab;

                // Load the workspace data asynchronously
                await workspaceVm.LoadWorkspaceAsync(path);
            }
            else
            {
                // Create regular FileViewer tab
                var fvVm = new FileViewerViewModel { FilePath = path };
                var newTab = new TabViewModel
                {
                    Header = fileName,
                    ContentVm = fvVm
                };
                FileTabs.Add(newTab);
                SelectedFileTab = newTab;
            }
        }

        private bool IsWorkspaceFile(string filePath, string fileName)
        {
            // Implement your logic to determine if this is a WorkspaceDto file
            // This could be based on file extension, naming convention, or file content inspection

            // Example: Check file extension
            return Path.GetExtension(filePath).ToLowerInvariant() == ".workspace" ||
                   fileName.ToLowerInvariant().Contains("workspace") ||
                   fileName.ToLowerInvariant().EndsWith(".msgpack");

            // Alternative: You could try to deserialize and catch exceptions
            // try
            // {
            //     var bytes = File.ReadAllBytes(filePath);
            //     MessagePackSerializer.Deserialize<dynamic>(bytes);
            //     return true; // If deserialization succeeds, assume it's a workspace file
            // }
            // catch
            // {
            //     return false;
            // }
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


