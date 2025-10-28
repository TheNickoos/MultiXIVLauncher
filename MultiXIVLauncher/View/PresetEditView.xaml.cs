using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MultiXIVLauncher.Views
{
    /// <summary>
    /// Interaction logic for the preset editor view.
    /// Allows the user to copy, open, and save presets for different configurations.
    /// </summary>
    public partial class PresetEditView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PresetEditView"/> class.
        /// </summary>
        public PresetEditView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Displays a placeholder message for the upcoming feature:
        /// copying settings from an existing character configuration.
        /// </summary>
        private void CopyFromCharacter_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Feature coming soon: copy settings from an existing character!");
        }

        /// <summary>
        /// Opens the preset folder in Windows Explorer if it exists.
        /// Displays an informational message if the folder is missing.
        /// </summary>
        private void OpenPresetFolder_Click(object sender, RoutedEventArgs e)
        {
            string presetPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets", TxtPresetName.Text);

            if (System.IO.Directory.Exists(presetPath))
            {
                Process.Start("explorer.exe", presetPath);
            }
            else
            {
                MessageBox.Show(
                    "The preset folder does not exist yet.",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        /// <summary>
        /// Displays a confirmation message when the preset is saved.
        /// </summary>
        private void SavePreset_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Preset \"{TxtPresetName.Text}\" saved successfully!");
        }

        /// <summary>
        /// Returns to the presets list view.
        /// </summary>
        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            ((LauncherWindow)Application.Current.MainWindow).SetPage(new PresetsView());
        }
    }
}
