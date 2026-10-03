using GeneSort.WPF.ViewModels;
using System.Windows;

namespace GeneSort.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}
