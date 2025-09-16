using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GeneSort.UI.Models
{
    public partial class ProjectModel : ObservableObject
    {
        [ObservableProperty]
        private string? projectFolder;
    }
}