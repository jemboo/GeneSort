using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

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
            private ProjectSelectionVm projectSelectionVm = new();

            public MainWindowVm()
            {
                var projectsTab = new TabViewModel
                {
                    Header = "Projects",
                    ContentVm = projectSelectionVm
                };
                Tabs.Add(projectsTab);
            }

            [RelayCommand]
            private void OpenExperiment(ProjectInfoVm? experimentInfo)
            {
                if (experimentInfo == null || string.IsNullOrEmpty(projectSelectionVm.RootFolder))
                {
                    return;
                }

                var expVm = new ProjectVm(
                    experimentName:experimentInfo.Name,
                    expperimentPath: experimentInfo.FullPath);

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