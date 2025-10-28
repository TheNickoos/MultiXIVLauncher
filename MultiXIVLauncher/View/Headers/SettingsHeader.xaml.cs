using System.Windows;
using System.Windows.Controls;
using MultiXIVLauncher.Utils.Interfaces;

namespace MultiXIVLauncher.Views.Headers
{
    /// <summary>
    /// Header displayed at the top of the Settings view.
    /// Provides navigation back to the main launcher view and allows contextual saving.
    /// </summary>
    public partial class SettingsHeader : UserControl
    {
        public SettingsHeader()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Saves the content of the current view if it supports saving.
        /// </summary>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (LauncherWindow)Application.Current.MainWindow;

            if (mainWindow.MainContent.Content is ISavableView savableView)
            {
                savableView.Save();
                MessageBox.Show("Changes saved successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("This view does not support saving.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Returns the user to the main view of the launcher.
        /// </summary>
        private void ReturnToMain(object sender, RoutedEventArgs e)
        {
            ((LauncherWindow)Application.Current.MainWindow).MainContent.Content = new Views.MainView();
        }
    }
}
