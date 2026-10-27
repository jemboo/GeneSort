using System.Configuration;
using System.IO;
using System.Windows;
using GeneSort.WPF.Settings;

namespace GeneSort.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static TableLayoutStore? TableLayouts { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            TableLayouts = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GeneSort", "table-layouts.json"));
            base.OnStartup(e);
        }
    }

}
