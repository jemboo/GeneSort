using GeneSort.Project;
using GeneSort.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GeneSort.UI.Views
{
    public partial class WorkspaceDisplayControl : UserControl
    {
        public WorkspaceDisplayControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty WorkspaceProperty =
            DependencyProperty.Register(
                nameof(Workspace),
                typeof(Workspace),
                typeof(WorkspaceDisplayControl),
                new PropertyMetadata(null, OnWorkspaceChanged));

        public workspace Workspace
        {
            get => (workspace)GetValue(WorkspaceProperty);
            set => SetValue(WorkspaceProperty, value);
        }

        private static void OnWorkspaceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WorkspaceDisplayControl control)
            {
                control.DataContext = e.NewValue != null
                    ? new WorkspaceViewModel((workspace)e.NewValue)
                    : null;

                if (control.DataContext != null && e.NewValue != null)
                {
                    var vm = (WorkspaceViewModel)control.DataContext;
                    control.ParametersDataGrid.Columns.Clear();

                    foreach (string key in vm.ParameterKeys)
                    {
                        var column = new DataGridTextColumn
                        {
                            Header = key,
                            Binding = new Binding($"[{key}]"),
                            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                        };
                        control.ParametersDataGrid.Columns.Add(column);
                    }
                }
            }
        }

    }
}