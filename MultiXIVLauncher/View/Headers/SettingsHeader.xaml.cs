using System.Windows;
using System.Windows.Controls;

namespace MultiXIVLauncher.Views.Headers
{
    public partial class SettingsHeader : UserControl
    {
        public SettingsHeader()
        {
            InitializeComponent();
        }

        private void ReturnToMain(object sender, RoutedEventArgs e)
        {
            ((LauncherWindow)Application.Current.MainWindow).MainContent.Content = new Views.MainView();
        }
    }
}
