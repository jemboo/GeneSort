using CommunityToolkit.Mvvm.ComponentModel;
using GeneSort.Project;
using System.Collections.ObjectModel;

namespace GeneSort.UI.ViewModels
{
    public partial class WorkspaceViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _description;

        public ObservableCollection<Dictionary<string, string>> RunParametersData { get; } 
                        = new ObservableCollection<Dictionary<string, string>>();

        public IReadOnlyList<string> ParameterKeys { get; private set; } = new List<string>();

        public WorkspaceViewModel(project workspace)
        {
            if (workspace == null) throw new ArgumentNullException(nameof(workspace));

            Name = workspace.Name;
            Description = workspace.Description;
            ParameterKeys = workspace.ParameterKeys.ToList();

            foreach (var rp in workspace.RunParametersArray)
            {
                var dict = new Dictionary<string, string>();
                foreach (var kvp in rp.ParamMap)
                {
                    dict[kvp.Key] = kvp.Value;
                }
                RunParametersData.Add(dict);
            }
        }
    }
}
