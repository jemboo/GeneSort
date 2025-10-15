using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GeneSort.UI.ViewModels
{
    public partial class ProjectInfoVm : ObservableObject
    {
        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string fullPath = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;
    }
}

