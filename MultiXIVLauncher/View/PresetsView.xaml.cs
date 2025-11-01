using MultiXIVLauncher.Models;
using MultiXIVLauncher.Services;
using MultiXIVLauncher.Utils;
using MultiXIVLauncher.Utils.Interfaces;
using MultiXIVLauncher.View.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MultiXIVLauncher.View
{
    /// <summary>
    /// Interaction logic for the presets management view.
    /// Allows the user to create, edit, and delete presets dynamically with animations.
    /// Changes are applied only to a temporary list until saved.
    /// </summary>
    public partial class PresetsView : UserControl, ISavableView
    {
        private bool isAddingPreset = false;

        /// <summary>
        /// Temporary copy of presets to edit safely before saving.
        /// </summary>
        private readonly List<Preset> tempPresets = [];

        public PresetsView()
        {
            InitializeComponent();
            ((LauncherWindow)Application.Current.MainWindow).SetHeaderContent(new SettingsHeader());

            // Copy presets from config to temporary list
            foreach (var preset in ConfigManager.Current.Presets)
                tempPresets.Add(new Preset
                {
                    Id = preset.Id,
                    Name = preset.Name,
                    FolderPath = preset.FolderPath
                });

            // Display all presets
            LoadPresets();
        }

        public void RefreshList() => LoadPresets();

        private void LoadPresets()
        {
            PresetListPanel.Children.Clear();
            foreach (var preset in tempPresets)
                AddPresetCard(preset, false);
        }

        private void BtnAddPreset_Click(object sender, RoutedEventArgs e)
        {
            if (!isAddingPreset)
            {
                isAddingPreset = true;
                BtnAddPreset.Visibility = Visibility.Collapsed;
                ShowElement(TxtPresetName);
                ShowElement(BtnValidate);
            }
        }

        private void BtnValidate_Click(object sender, RoutedEventArgs e)
        {
            if (!isAddingPreset) return;

            string newPresetName = TxtPresetName.Text.Trim();
            if (!string.IsNullOrEmpty(newPresetName))
            {
                string presetsBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets");
                if (!Directory.Exists(presetsBase))
                    Directory.CreateDirectory(presetsBase);

                string normalized = SanitizeFolderName(newPresetName);
                string presetDir = Path.Combine(presetsBase, normalized);

                var newPreset = new Preset
                {
                    Id = NextPresetId(),
                    Name = newPresetName,
                    FolderPath = presetDir
                };

                // Add to temporary + UI
                tempPresets.Add(newPreset);
                AddPresetCard(newPreset);

                // Add to global config (keep in sync)
                ConfigManager.Current.Presets.Add(newPreset);

                // Ensure folder exists
                if (!Directory.Exists(presetDir))
                    Directory.CreateDirectory(presetDir);

                ConfigManager.Save();
            }

            isAddingPreset = false;
            HideElement(TxtPresetName);
            HideElement(BtnValidate);
            TxtPresetName.Text = string.Empty;
            BtnAddPreset.Visibility = Visibility.Visible;
        }

        private int NextPresetId()
        {
            int max1 = tempPresets.Count == 0 ? 0 : tempPresets.Max(p => p.Id);
            int max2 = ConfigManager.Current.Presets.Count == 0 ? 0 : ConfigManager.Current.Presets.Max(p => p.Id);
            return Math.Max(max1, max2) + 1;
        }

        private static string SanitizeFolderName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Preset";
            string s = input.Replace(' ', '_').Trim();
            var invalid = Path.GetInvalidFileNameChars();
            s = new string(s.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
            return string.IsNullOrWhiteSpace(s) ? "Preset" : s;
        }

        private void AddPresetCard(Preset preset, bool animate = true)
        {
            Border card = new()
            {
                Style = (Style)FindResource("CardBorder"),
                Opacity = 0,
                RenderTransform = new TranslateTransform(0, 30),
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(185, 147, 255),
                    BlurRadius = 25,
                    ShadowDepth = 0,
                    Opacity = 0.9
                }
            };

            Grid content = new();
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameText = new TextBlock
            {
                Text = preset.Name,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("ClrTextPrimary"),
                VerticalAlignment = VerticalAlignment.Center
            };

            StackPanel actions = new()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var editButton = new Button
            {
                Content = "Edit",
                Height = 30,
                Margin = new Thickness(0, 0, 8, 0),
                Style = (Style)FindResource("OutlineButton")
            };
            editButton.Click += (s, e) =>
            {
                var editView = new PresetEditView(preset, this);
                ((LauncherWindow)Application.Current.MainWindow).SetPage(editView);
            };

            var deleteButton = new Button
            {
                Content = "Delete",
                Height = 30,
                Style = (Style)FindResource("OutlineButton")
            };
            deleteButton.Click += (s, e) =>
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the preset \"{preset.Name}\" and its folder?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                try
                {
                    if (!string.IsNullOrWhiteSpace(preset.FolderPath) && Directory.Exists(preset.FolderPath))
                        Directory.Delete(preset.FolderPath, true);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to delete preset folder: {ex.Message}");
                    MessageBox.Show($"An error occurred while deleting the preset folder:\n{ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                tempPresets.Remove(preset);
                ConfigManager.Current.Presets.RemoveAll(p => p.Id == preset.Id);
                UIAnimationHelper.AnimateRemoval(card, () => PresetListPanel.Children.Remove(card));
                ConfigManager.Save();
            };

            actions.Children.Add(editButton);
            actions.Children.Add(deleteButton);

            content.Children.Add(nameText);
            content.Children.Add(actions);
            Grid.SetColumn(actions, 1);
            card.Child = content;

            PresetListPanel.Children.Add(card);

            if (animate) UIAnimationHelper.AnimateAppearance(card);
            else card.Opacity = 1;
        }

        /// <summary>
        /// Saves all temporary presets to the configuration and ensures their directories exist (normalized).
        /// </summary>
        public void Save()
        {
            try
            {
                string presetsBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets");
                if (!Directory.Exists(presetsBasePath))
                    Directory.CreateDirectory(presetsBasePath);

                // Sync config with temp + ensure normalized paths
                ConfigManager.Current.Presets.Clear();

                foreach (var p in tempPresets)
                {
                    string normalized = SanitizeFolderName(p.Name ?? "Preset");
                    string desiredPath = Path.Combine(presetsBasePath, normalized);

                    // Rename/move folder if necessary
                    if (!string.Equals(p.FolderPath, desiredPath, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            bool oldExists = !string.IsNullOrWhiteSpace(p.FolderPath) && Directory.Exists(p.FolderPath);
                            bool newExists = Directory.Exists(desiredPath);

                            if (oldExists && !newExists)
                                Directory.Move(p.FolderPath, desiredPath);
                            else if (!oldExists && !newExists)
                                Directory.CreateDirectory(desiredPath);
                            else if (oldExists && newExists)
                            {
                                // Merge then delete old
                                CopyDirectory(p.FolderPath!, desiredPath, overwrite: true);
                                TryDeleteDirectorySafe(p.FolderPath!);
                            }

                            p.FolderPath = desiredPath;
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Error renaming preset folder: " + ex.Message);
                            if (!Directory.Exists(desiredPath))
                                Directory.CreateDirectory(desiredPath);
                            p.FolderPath = desiredPath;
                        }
                    }
                    else
                    {
                        if (!Directory.Exists(desiredPath))
                            Directory.CreateDirectory(desiredPath);
                    }

                    ConfigManager.Current.Presets.Add(p);
                }

                ConfigManager.Save();
                Logger.Info("Presets saved and directories normalized successfully.");
            }
            catch (Exception ex)
            {
                Logger.Error("Error while saving presets: " + ex.Message);
                MessageBox.Show("An error occurred while saving preset folders.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

        private static void TryDeleteDirectorySafe(string path)
        {
            try { if (Directory.Exists(path)) Directory.Delete(path, true); }
            catch { /* ignore */ }
        }

        private static void ShowElement(UIElement element)
        {
            element.Visibility = Visibility.Visible;
            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(150)));
            element.BeginAnimation(OpacityProperty, fadeIn);
        }

        private static void HideElement(UIElement element)
        {
            var fadeOut = new System.Windows.Media.Animation.DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(150)));
            fadeOut.Completed += (s, _) => element.Visibility = Visibility.Collapsed;
            element.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void BtnDownloadPreset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var downloadWindow = new PresetDownloadWindow(null)
                {
                    Owner = Application.Current.MainWindow
                };

                downloadWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open preset download window:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

    }
}
