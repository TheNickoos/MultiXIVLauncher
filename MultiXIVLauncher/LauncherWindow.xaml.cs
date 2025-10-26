using System.Windows;
using System.Windows.Controls;
using MultiXIVLauncher.Views;
using MultiXIVLauncher.Views.Headers;

namespace MultiXIVLauncher
{
    public partial class LauncherWindow : Window
    {
        public LauncherWindow()
        {
            InitializeComponent();
            MainContent.Content = new MainView();
        }

        public void SetHeaderContent(UIElement element)
        {
            HeaderDynamicArea.Content = element;
        }

        private void ShowSettings(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new SettingsView();
        }

        private void ShowCharacters(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new CharactersView();
        }

        private void ShowPresets(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new PresetsView();
        }

        private void ShowGroups(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new GroupsView();
        }

        private void OpenMogStation(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.mogstation.com");
        }

        private void OpenXIVISUP(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://xivisup.com/");
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            Close();
        }
        public void SetPage(UserControl view)
        {
            MainContent.Content = view;
        }

    }
}
