using MultiXIVLauncher.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public PresetEditView(Preset preset, PresetsView parent)
        {
            InitializeComponent();
            currentPreset = preset ?? throw new ArgumentNullException(nameof(preset));
            parentView = parent ?? throw new ArgumentNullException(nameof(parent));

            TxtPresetName.Text = currentPreset.Name;
        }

        /// <summary>
        /// Opens the selection window to copy configuration from an existing character.
        /// </summary>
        private void CopyFromCharacter_Click(object sender, RoutedEventArgs e)
        {
            var selectWindow = new PresetEditSelectCharacterWindow
            {
                Owner = Application.Current.MainWindow
            };

            if (selectWindow.ShowDialog() == true && selectWindow.SelectedCharacter != null)
            {
                try
                {
                    ApplyChanges(); // ensure preset folder path is correct and exists

                    string sourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Characters", selectWindow.SelectedCharacter.Name);
                    string targetDir = currentPreset.FolderPath;

                    if (!Directory.Exists(sourceDir))
                    {
                        MessageBox.Show($"The directory for character \"{selectWindow.SelectedCharacter.Name}\" was not found.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir);

                    CopyDirectory(sourceDir, targetDir, overwrite: true);

                    MessageBox.Show($"Configuration copied successfully from \"{selectWindow.SelectedCharacter.Name}\" to preset \"{currentPreset.Name}\".",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while copying files:\n{ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Opens the preset folder in Windows Explorer. Creates it if it doesn't exist.
        /// </summary>
        private void OpenPresetFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplyChanges(); // ensures path exists
                Process.Start("explorer.exe", currentPreset.FolderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to open preset folder: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Applies changes to the preset: update name and RENAME folder if needed.
        /// </summary>
        private void ApplyChanges()
        {
            string newDisplayName = TxtPresetName.Text.Trim();
            if (string.IsNullOrEmpty(newDisplayName))
                return;

            string presetsBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets");

            // Old state
            string oldDisplayName = currentPreset.Name ?? "";
            string oldPath = currentPreset.FolderPath ?? "";

            // New normalized folder name
            string normalized = SanitizeFolderName(newDisplayName);
            string newPath = Path.Combine(presetsBase, normalized);

            // Ensure base directory exists
            if (!Directory.Exists(presetsBase))
                Directory.CreateDirectory(presetsBase);

            // If path changed, try to move the directory
            if (!string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    bool oldExists = !string.IsNullOrWhiteSpace(oldPath) && Directory.Exists(oldPath);
                    bool newExists = Directory.Exists(newPath);

                    if (oldExists && !newExists)
                    {
                        Directory.Move(oldPath, newPath);
                    }
                    else if (!oldExists && !newExists)
                    {
                        Directory.CreateDirectory(newPath);
                    }
                    else if (oldExists && newExists)
                    {
                        // Conflict: merge old into new (overwrite), then delete old
                        CopyDirectory(oldPath, newPath, overwrite: true);
                        TryDeleteDirectorySafe(oldPath);
                    }
                    // else (!oldExists && newExists): nothing to do, just reuse existing new folder
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to rename preset folder:\n{ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    // If rename failed, at least ensure newPath exists and continue
                    if (!Directory.Exists(newPath))
                    {
                        try { Directory.CreateDirectory(newPath); } catch { /* ignore */ }
                    }
                }
            }
            else
            {
                // Path unchanged: make sure it exists
                if (!Directory.Exists(newPath))
                    Directory.CreateDirectory(newPath);
            }

            // Update model (in-memory)
            currentPreset.Name = newDisplayName;
            currentPreset.FolderPath = newPath;
        }

        private void DownloadPreset_Click(object sender, RoutedEventArgs e)
        {
            var downloadWindow = new PresetDownloadWindow(currentPreset)
            {
                Owner = Application.Current.MainWindow
            };
            downloadWindow.ShowDialog();
        }


        private static string SanitizeFolderName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Preset";

            // Replace spaces with underscores
            string s = input.Replace(' ', '_').Trim();

            // Remove/replace invalid file name chars
            var invalid = Path.GetInvalidFileNameChars();
            s = new string(s.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());

            // Avoid empty name after sanitization
            return string.IsNullOrWhiteSpace(s) ? "Preset" : s;
        }

        private static void TryDeleteDirectorySafe(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch
            {
                // swallow — not critical
            }
        }

        /// <summary>
        /// Recursively copies a directory and its contents to a target location.
        /// </summary>
        private static void CopyDirectory(string sourceDir, string destDir, bool overwrite)
        {
            Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string targetFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, targetFile, overwrite);
            }

            foreach (string folder in Directory.GetDirectories(sourceDir))
            {
                string targetFolder = Path.Combine(destDir, Path.GetFileName(folder));
                CopyDirectory(folder, targetFolder, overwrite);
            }
        }

        /// <summary>
        /// Returns to the presets list view and refreshes its display.
        /// </summary>
        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges();                 // Update in-memory preset and filesystem
            parentView.RefreshList();       // Refresh existing list UI
            ((LauncherWindow)Application.Current.MainWindow).SetPage(parentView); // Navigate back to the SAME instance
        }
    }
}
