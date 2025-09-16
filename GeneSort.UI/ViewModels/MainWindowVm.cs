using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeneSort.UI.Models;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.IO;

namespace GeneSort.UI.ViewModels
{

    namespace GeneSort.UI.ViewModels
    {
        public partial class MainWindowVm : ObservableObject
        {
            [ObservableProperty]
            private ObservableCollection<TabViewModel> tabs = new();

            [ObservableProperty]
            private ProjectsViewModel projects = new();

            public MainWindowVm()
            {
                var projectsTab = new TabViewModel
                {
                    Header = "Projects",
                    ContentVm = Projects
                };
                Tabs.Add(projectsTab);
            }

            [RelayCommand]
            private void OpenExperiment(string? experimentName)
            {
                if (string.IsNullOrEmpty(experimentName))
                {
                    return;
                }

                var expVm = new ExperimentViewModel { ExperimentName = experimentName };
                var expTab = new TabViewModel
                {
                    Header = experimentName,
                    ContentVm = expVm
                };
                Tabs.Add(expTab);
            }
        }
    }

}