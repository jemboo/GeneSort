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
            private int selectedTabIndex;

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
            private void OpenExperiment(ExperimentInfoViewModel? experimentInfo)
            {
                if (experimentInfo == null || string.IsNullOrEmpty(Projects.ProjectFolder))
                {
                    return;
                }

                var expVm = new ExperimentViewModel
                {
                    ExperimentName = experimentInfo.Name,
                    ExperimentPath = experimentInfo.FullPath
                };
                var expTab = new TabViewModel
                {
                    Header = experimentInfo.Name,
                    ContentVm = expVm
                };
                Tabs.Add(expTab);
                SelectedTabIndex = Tabs.Count - 1;
            }

            [RelayCommand]
            private void CloseTab(TabViewModel tab)
            {
                if (tab.Header != "Projects")
                {
                    Tabs.Remove(tab);
                }
            }
        }
    }

}