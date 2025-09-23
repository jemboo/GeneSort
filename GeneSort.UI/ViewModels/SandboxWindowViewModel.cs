using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GeneSort.UI.ViewModels
{
    public partial class SandboxWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string testText = "Hello from Sandbox!";

        [RelayCommand]
        private void ShowMessage()
        {
            System.Windows.MessageBox.Show(TestText, "Sandbox Message");
        }
    }
}