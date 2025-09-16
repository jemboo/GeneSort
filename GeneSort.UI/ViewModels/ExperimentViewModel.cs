using CommunityToolkit.Mvvm.ComponentModel;

namespace GeneSort.UI.ViewModels
{
    public partial class ExperimentViewModel : ObservableObject
    {
        [ObservableProperty]
        private string experimentName;
    }
}