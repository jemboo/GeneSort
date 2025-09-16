using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace GeneSort.UI.ViewModels
{
    public partial class TabViewModel : ObservableObject
    {
        [ObservableProperty]
        private string header;

        [ObservableProperty]
        private object contentVm;
    }
}