using MultiXIVLauncher.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MultiXIVLauncher.View
{
    /// <summary>
    /// Interaction logic for the preset editor view.
    /// Allows the user to modify a preset temporarily before final save.
    /// </summary>
    public partial class PresetEditView : UserControl
    {
        private readonly Preset currentPreset;
        private readonly PresetsView parentView;

        /// <summary>
        /// Initializes a new instance of the <see cref="PresetEditView"/> class for editing an existing preset.
        /// </summary>
        /// <param name="preset">The preset to edit (from the temporary list).</param>
        /// <param name="parent">The parent PresetsView instance to refresh on return.</param>
        public PresetEditView(Preset preset, PresetsView parent)
        {
            InitializeComponent();
            currentPreset = preset ?? throw new ArgumentNullException(nameof(preset));
            parentView = parent ?? throw new ArgumentNullException(nameof(parent));

            TxtPresetName.Text = currentPreset.Name;
        }

        private void CopyFromCharacter_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Feature coming soon: copy settings from an existing character!");
        }

        private void OpenPresetFolder_Click(object sender, RoutedEventArgs e)
        {
            string presetPath = currentPreset.FolderPath;

            if (Directory.Exists(presetPath))
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
        /// Applies changes made in the UI to the current preset object (in-memory only).
        /// </summary>
        private void ApplyChanges()
        {
            string newName = TxtPresetName.Text.Trim();

            if (!string.IsNullOrEmpty(newName))
            {
                currentPreset.Name = newName;
                currentPreset.FolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets", newName);
            }
        }

        /// <summary>
        /// Returns to the presets list view and refreshes its display.
        /// </summary>
        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges();                 // Update in-memory preset
            parentView.RefreshList();       // Refresh existing list UI
            ((LauncherWindow)Application.Current.MainWindow).SetPage(parentView); // Navigate back to the SAME instance
        }
    }
}
