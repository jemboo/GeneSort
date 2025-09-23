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
            private ExperimentSelectionVm experimentSelectionVm = new();

            public MainWindowVm()
            {
                var projectsTab = new TabViewModel
                {
                    Header = "Projects",
                    ContentVm = ExperimentSelectionVm
                };
                Tabs.Add(projectsTab);
            }

            [RelayCommand]
            private void OpenExperiment(ExperimentInfoViewModel? experimentInfo)
            {
                if (experimentInfo == null || string.IsNullOrEmpty(ExperimentSelectionVm.RootFolder))
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