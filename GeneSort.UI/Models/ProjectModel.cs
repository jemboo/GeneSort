using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace GeneSort.UI.Models
{
    public partial class ProjectModel : ObservableObject
    {
        [ObservableProperty]
        private string? projectFolder;
    }
}