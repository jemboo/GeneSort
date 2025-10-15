using System.Windows;
using System.Windows.Controls;

namespace GeneSort.UI.Behaviors
{
    public static class TreeViewBehavior
    {

        #region

        public static readonly DependencyProperty BindableSelectedItemProperty =
            DependencyProperty.RegisterAttached("BindableSelectedItem", typeof(bool), typeof(TreeViewBehavior),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBindableSelectedItemChanged));

        public static object? GetBindableSelectedItem(DependencyObject obj) => (object?)obj.GetValue(BindableSelectedItemProperty);

        public static void SetBindableSelectedItem(DependencyObject obj, object? value) => obj.SetValue(BindableSelectedItemProperty, value);

        #endregion


        #region

        public static readonly DependencyProperty SelectionActionProperty =
            DependencyProperty.RegisterAttached("SelectionAction", typeof(Action<object>), typeof(TreeViewBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBindableSelectedItemChanged));
        public static Action<object>? GetSelectionAction(DependencyObject obj)
            => (Action<object>?)obj.GetValue(SelectionActionProperty);

        public static void SetSelectionAction(DependencyObject obj, Action<object>? value)
            => obj.SetValue(SelectionActionProperty, value);
        #endregion



        private static void OnBindableSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TreeView treeView)
            {
                var qua = treeView.DataContext as ViewModels.ProjectVm;
                //Unsubscribe temporarily to avoid recursion
                treeView.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
                treeView.SelectedItemChanged += OnTreeViewSelectedItemChanged;
            }
        }

        private static void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object?> e)
        {
            if (sender is TreeView treeView)
            {
                var selectionAction = GetSelectionAction(treeView);
                if (selectionAction != null)
                {
                    selectionAction(e.NewValue);
                }
            }
        }
    }
}