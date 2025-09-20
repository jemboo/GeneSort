using System.Windows;
using System.Windows.Controls;

namespace GeneSort.UI.Behaviors
{
    public static class TreeViewBehavior
    {
        public static readonly DependencyProperty BindableSelectedItemProperty =
            DependencyProperty.RegisterAttached("BindableSelectedItem", typeof(object), typeof(TreeViewBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBindableSelectedItemChanged));

        public static object? GetBindableSelectedItem(DependencyObject obj) => (object?)obj.GetValue(BindableSelectedItemProperty);

        public static void SetBindableSelectedItem(DependencyObject obj, object? value) => obj.SetValue(BindableSelectedItemProperty, value);

        private static void OnBindableSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TreeView treeView)
            {
                // Unsubscribe temporarily to avoid recursion 
              //  treeView.SelectedItemChanged -= OnTreeViewSelectedItemChanged;

                // Subscribe
              //  treeView.SelectedItemChanged += OnTreeViewSelectedItemChanged;
            }
        }

        private static void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object?> e)
        {
            if (sender is TreeView treeView)
            {
                SetBindableSelectedItem(treeView, e.NewValue);
            }
        }
    }
}