using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using MultiXIVLauncher.Views.Headers;

namespace MultiXIVLauncher.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();

            ((LauncherWindow)Application.Current.MainWindow).SetHeaderContent(new SettingsHeader());
        }

        private void BrowseXIVLauncher(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "XIVLauncher executable|XIVLauncher.exe",
                Title = "Sélectionnez votre fichier XIVLauncher.exe"
            };

            if (dialog.ShowDialog() == true)
            {
                XIVLauncherPath.Text = dialog.FileName;
            }
        }
    }
}
