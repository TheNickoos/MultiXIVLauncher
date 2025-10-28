using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using MultiXIVLauncher.Views.Headers;
using MultiXIVLauncher.Utils.Interfaces;
using MultiXIVLauncher.Services;

namespace MultiXIVLauncher.Views
{
    /// <summary>
    /// Interaction logic for the settings view.
    /// Allows users to configure launcher paths, language preferences, and other global options.
    /// </summary>
    public partial class SettingsView : UserControl, ISavableView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsView"/> class
        /// and sets the header content.
        /// </summary>
        public SettingsView()
        {
            InitializeComponent();
            ((LauncherWindow)Application.Current.MainWindow).SetHeaderContent(new SettingsHeader());

            // Chargement initial des données depuis la config
            XIVLauncherPath.Text = ConfigManager.Current.Launcher.Path;
            DalamudBetaKey.Text = ConfigManager.Current.Launcher.DalamudBetaKey;
            DalamudBetaKind.Text = ConfigManager.Current.Launcher.DalamudBetaKind;

            foreach (ComboBoxItem item in LanguageSelect.Items)
            {
                if ((string)item.Tag == ConfigManager.Current.Launcher.Language)
                {
                    LanguageSelect.SelectedItem = item;
                    break;
                }
            }
        }

        /// <summary>
        /// Opens a file dialog for the user to select the XIVLauncher executable.
        /// </summary>
        private void BrowseXIVLauncher(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "XIVLauncher executable|XIVLauncher.exe",
                Title = "Select your XIVLauncher.exe file"
            };

            if (dialog.ShowDialog() == true)
                XIVLauncherPath.Text = dialog.FileName;
        }

        /// <summary>
        /// Saves the current settings into ConfigManager.
        /// </summary>
        public void Save()
        {
            // Sauvegarde des valeurs dans la configuration
            ConfigManager.Current.Launcher.Path = XIVLauncherPath.Text;
            ConfigManager.Current.Launcher.DalamudBetaKey = DalamudBetaKey.Text;
            ConfigManager.Current.Launcher.DalamudBetaKind = DalamudBetaKind.Text;

            if (LanguageSelect.SelectedItem is ComboBoxItem selected)
                ConfigManager.Current.Launcher.Language = selected.Tag.ToString();

            // Écriture du fichier
            ConfigManager.Save();
        }
    }
}
