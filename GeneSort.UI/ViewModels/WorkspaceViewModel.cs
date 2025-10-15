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

        public WorkspaceViewModel(project project)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));

            Name = project.Name;
            Description = project.Description;
            ParameterKeys = project.ParameterKeys.ToList();

            foreach (var rp in project.RunParametersArray)
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
