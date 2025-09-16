
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;

namespace GeneSort.UI.ViewModels
{
    public partial class FileViewerViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? filePath;

        [ObservableProperty]
        private string? fileContent;

        partial void OnFilePathChanged(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                FileContent = string.Empty;
                return;
            }

            try
            {
                FileContent = File.ReadAllText(value);
            }
            catch (Exception)
            {
                FileContent = "Unable to read file content.";
            }
        }
    }
}