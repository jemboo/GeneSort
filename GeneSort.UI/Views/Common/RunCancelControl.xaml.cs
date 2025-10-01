using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GeneSort.UI.Views.Common
{
    /// <summary>
    /// Interaction logic for RunCancelControl.xaml
    /// </summary>
    public partial class RunCancelControl : UserControl
    {
        public RunCancelControl()
        {
            InitializeComponent();
        }

        public ICommand RunCommand
        {
            get => (ICommand)GetValue(RunCommandProperty);
            set => SetValue(RunCommandProperty, value);
        }
        public static readonly DependencyProperty RunCommandProperty =
            DependencyProperty.Register(nameof(RunCommand), typeof(ICommand), typeof(RunCancelControl));

        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }
        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(nameof(CancelCommand), typeof(ICommand), typeof(RunCancelControl));

        public bool IsRunning
        {
            get => (bool)GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }
        public static readonly DependencyProperty IsRunningProperty =
            DependencyProperty.Register(nameof(IsRunning), typeof(bool), typeof(RunCancelControl), new PropertyMetadata(false, OnIsRunningChanged));

        private static void OnIsRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Set a breakpoint on the next line to inspect changes
            var control = (RunCancelControl)d;
            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;
            // You can add logging or other logic here if needed, but this callback is invoked whenever the property value changes.
        }

        public Style RunButtonStyle
        {
            get => (Style)GetValue(RunButtonStyleProperty);
            set => SetValue(RunButtonStyleProperty, value);
        }
        public static readonly DependencyProperty RunButtonStyleProperty =
            DependencyProperty.Register(nameof(RunButtonStyle), typeof(Style), typeof(RunCancelControl));

        public Style DisabledRunButtonStyle
        {
            get => (Style)GetValue(DisabledRunButtonStyleProperty);
            set => SetValue(DisabledRunButtonStyleProperty, value);
        }
        public static readonly DependencyProperty DisabledRunButtonStyleProperty =
            DependencyProperty.Register(nameof(DisabledRunButtonStyle), typeof(Style), typeof(RunCancelControl));

        public Style CancelButtonStyle
        {
            get => (Style)GetValue(CancelButtonStyleProperty);
            set => SetValue(CancelButtonStyleProperty, value);
        }
        public static readonly DependencyProperty CancelButtonStyleProperty =
            DependencyProperty.Register(nameof(CancelButtonStyle), typeof(Style), typeof(RunCancelControl));
    }
}
