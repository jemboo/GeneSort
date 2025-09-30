using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GeneSort.UI.Views.Common
{
    public class RunCancelButton0 : Control
    {
        static RunCancelButton0()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RunCancelButton0), new FrameworkPropertyMetadata(typeof(RunCancelButton0)));
        }

        public ICommand RunCommand
        {
            get => (ICommand)GetValue(RunCommandProperty);
            set => SetValue(RunCommandProperty, value);
        }
        public static readonly DependencyProperty RunCommandProperty =
            DependencyProperty.Register(nameof(RunCommand), typeof(ICommand), typeof(RunCancelButton0));

        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }
        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(nameof(CancelCommand), typeof(ICommand), typeof(RunCancelButton0));

        public bool IsRunning
        {
            get => (bool)GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }
        public static readonly DependencyProperty IsRunningProperty =
            DependencyProperty.Register(nameof(IsRunning), typeof(bool), typeof(RunCancelButton0));

        public bool CanCancel
        {
            get => (bool)GetValue(CanCancelProperty);
            set => SetValue(CanCancelProperty, value);
        }
        public static readonly DependencyProperty CanCancelProperty =
            DependencyProperty.Register(nameof(CanCancel), typeof(bool), typeof(RunCancelButton0));

        public object RunContent
        {
            get => GetValue(RunContentProperty);
            set => SetValue(RunContentProperty, value);
        }
        public static readonly DependencyProperty RunContentProperty =
            DependencyProperty.Register(nameof(RunContent), typeof(object), typeof(RunCancelButton0));

        public object CancelContent
        {
            get => GetValue(CancelContentProperty);
            set => SetValue(CancelContentProperty, value);
        }
        public static readonly DependencyProperty CancelContentProperty =
            DependencyProperty.Register(nameof(CancelContent), typeof(object), typeof(RunCancelButton0));

        public Style RunButtonStyle
        {
            get => (Style)GetValue(RunButtonStyleProperty);
            set => SetValue(RunButtonStyleProperty, value);
        }
        public static readonly DependencyProperty RunButtonStyleProperty =
            DependencyProperty.Register(nameof(RunButtonStyle), typeof(Style), typeof(RunCancelButton0));

        public Style DisabledRunButtonStyle
        {
            get => (Style)GetValue(DisabledRunButtonStyleProperty);
            set => SetValue(DisabledRunButtonStyleProperty, value);
        }
        public static readonly DependencyProperty DisabledRunButtonStyleProperty =
            DependencyProperty.Register(nameof(DisabledRunButtonStyle), typeof(Style), typeof(RunCancelButton0));

        public Style CancelButtonStyle
        {
            get => (Style)GetValue(CancelButtonStyleProperty);
            set => SetValue(CancelButtonStyleProperty, value);
        }
        public static readonly DependencyProperty CancelButtonStyleProperty =
            DependencyProperty.Register(nameof(CancelButtonStyle), typeof(Style), typeof(RunCancelButton0));
    }
}