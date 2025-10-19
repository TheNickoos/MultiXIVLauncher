using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace MultiXIVLauncher
{
    public partial class SettingsWindow : Window
    {
        private readonly string configPath;
        private Config config;

        public SettingsWindow()
        {
            InitializeComponent();
            InitializePresetView();

            configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

            InitializeLanguageComboBox();

            LoadConfig();
            ApplyConfigToUI();
            LoadPresets();

            if (string.IsNullOrEmpty(InputLauncherTextbox.Text))
            {
                string detected = DetectLauncherFromRegistry();
                if (!string.IsNullOrEmpty(detected))
                {
                    InputLauncherTextbox.Text = detected;
                    config.Launcher.Path = detected;
                    config.Save(configPath);
                }
            }

            Closing += (s, e) => config.Save(configPath);

            InputLauncherTextbox.TextChanged += (s, e) =>
            {
                string path = InputLauncherTextbox.Text?.Trim() ?? "";
                config.Launcher.Path = path;
                ValidateLauncherPath(path);
                config.Save(configPath);
            };

            LangageLauncherComboBox.SelectionChanged += (s, e) =>
            {
                if (LangageLauncherComboBox.SelectedItem is ComboBoxItem sel)
                {
                    config.Launcher.Language = sel.Tag?.ToString() ?? "en";
                    config.Save(configPath);
                }
            };

            BrowseForLauncherButton.Click += BrowseForLauncherButton_Click;

            PresetAddButton.Click += PresetAddButton_Click;
            PresetDeleteButton.Click += PresetDeleteButton_Click;
            PresetExploreButton.Click += PresetExploreButton_Click;
            PresetListBox.SelectionChanged += PresetListBox_SelectionChanged;

            PresetNameTextBox.TextChanged += (s, e) =>
            {
                if (PresetListBox.SelectedItem is ListBoxItem selectedItem)
                {
                    int id = (int)selectedItem.Tag;
                    var preset = config.Presets.Find(p => p.Id == id);
                    if (preset != null)
                    {
                        preset.Name = PresetNameTextBox.Text.Trim();
                        selectedItem.Content = preset.Name;
                        config.Save(configPath);
                    }
                }
            };
        }

        private void InitializeLanguageComboBox()
        {
            var langs = new List<(string Code, string Label)>
            {
                ("en", "English"),
                // futurs langages : ("fr", "Français"), etc.
            };

            LangageLauncherComboBox.Items.Clear();
            foreach (var (code, label) in langs)
            {
                var item = new ComboBoxItem
                {
                    Content = label,
                    Tag = code
                };
                LangageLauncherComboBox.Items.Add(item);
            }
        }

        private void LoadConfig()
        {
            config = Config.Load(configPath);

            if (config == null)
            {
                var result = MessageBox.Show(
                    "Configuration file is corrupted. Reset to default?",
                    "Error",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                    config = new Config();
                else
                    Close();
            }
        }

        private void ApplyConfigToUI()
        {
            if (config?.Launcher == null)
                return;

            InputLauncherTextbox.Text = config.Launcher.Path ?? "";

            string langCode = config.Launcher.Language ?? "en";
            foreach (var item in LangageLauncherComboBox.Items)
            {
                if (item is ComboBoxItem comboItem && (comboItem.Tag?.ToString() ?? "") == langCode)
                {
                    LangageLauncherComboBox.SelectedItem = comboItem;
                    break;
                }
            }
        }

        private void ValidateLauncherPath(string path)
        {
            if (!File.Exists(path))
                InputLauncherTextbox.BorderBrush = System.Windows.Media.Brushes.Red;
            else
                InputLauncherTextbox.BorderBrush = System.Windows.Media.Brushes.Green;
        }

        private void BrowseForLauncherButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "XIVLauncher.exe|XIVLauncher.exe",
                Title = "Select XIVLauncher.exe"
            };

            if (dlg.ShowDialog() == true)
            {
                InputLauncherTextbox.Text = dlg.FileName;
            }
        }

        private string DetectLauncherFromRegistry()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\XIVLauncher"))
                {
                    if (key != null)
                    {
                        object installLocation = key.GetValue("InstallLocation");
                        if (installLocation != null)
                        {
                            string path = Path.Combine(installLocation.ToString(), "XIVLauncher.exe");
                            if (File.Exists(path))
                                return path;
                        }
                    }
                }
            }
            catch
            {
                // Ignore if error
            }
            return null;
        }

        private void InitializePresetView()
        {
            PresetAddButton.Visibility = Visibility.Visible;
            PresetGrid.Visibility = Visibility.Collapsed;
        }

        private void LoadPresets()
        {
            PresetListBox.Items.Clear();

            if (config?.Presets == null) return;

            foreach (var preset in config.Presets)
            {
                var item = new ListBoxItem
                {
                    Content = preset.Name,
                    Tag = preset.Id
                };
                PresetListBox.Items.Add(item);
            }
        }

        private void PresetAddButton_Click(object sender, RoutedEventArgs e)
        {
            PresetAddButton.Visibility = Visibility.Collapsed;
            PresetGrid.Visibility = Visibility.Visible;
            PresetNameLabel.Visibility = Visibility.Visible;
            PresetNameTextBox.Visibility = Visibility.Visible;
            RectangleEdit.Visibility = Visibility.Visible;
            PresetSaveButton.Visibility = Visibility.Visible;
            PresetDeleteButton.Visibility = Visibility.Collapsed;
            PresetExploreButton.Visibility = Visibility.Collapsed;

            int newId = config.Presets.Count > 0 ? config.Presets[config.Presets.Count - 1].Id + 1 : 1;
            var newPreset = new Preset
            {
                Id = newId,
                Name = "New Preset"
            };

            string presetsRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets");
            if (!Directory.Exists(presetsRoot))
                Directory.CreateDirectory(presetsRoot);

            string folderName = "Preset_" + newPreset.Id;
            newPreset.FolderPath = Path.Combine(presetsRoot, folderName);
            if (!Directory.Exists(newPreset.FolderPath))
                Directory.CreateDirectory(newPreset.FolderPath);

            config.Presets.Add(newPreset);
            config.Save(configPath);

            var item = new ListBoxItem
            {
                Content = newPreset.Name,
                Tag = newPreset.Id
            };
            PresetListBox.Items.Add(item);
            PresetListBox.SelectedItem = item;

            PresetNameTextBox.Text = newPreset.Name;
        }


        private void PresetDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (PresetListBox.SelectedItem is ListBoxItem selectedItem)
            {
                int id = (int)selectedItem.Tag;
                var preset = config.Presets.Find(p => p.Id == id);
                if (preset != null)
                {
                    if (!string.IsNullOrEmpty(preset.FolderPath) && Directory.Exists(preset.FolderPath))
                    {
                        try
                        {
                            Directory.Delete(preset.FolderPath, true); // true pour suppression récursive
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Unable to delete preset's folder : {ex.Message}",
                                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    config.Presets.Remove(preset);
                    PresetListBox.Items.Remove(selectedItem);
                    config.Save(configPath);
                }
            }

            PresetGrid.Visibility = Visibility.Collapsed;
            PresetAddButton.Visibility = Visibility.Visible;
        }


        private void PresetExploreButton_Click(object sender, RoutedEventArgs e)
        {
            if (PresetListBox.SelectedItem is ListBoxItem selectedItem)
            {
                int id = (int)selectedItem.Tag;
                var preset = config.Presets.Find(p => p.Id == id);
                if (preset != null && Directory.Exists(preset.FolderPath))
                {
                    Process.Start("explorer.exe", preset.FolderPath);
                }
            }
        }

        private void PresetListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PresetListBox.SelectedItem is ListBoxItem selectedItem)
            {
                int id = (int)selectedItem.Tag;
                var preset = config.Presets.Find(p => p.Id == id);
                if (preset != null)
                {
                    PresetGrid.Visibility = Visibility.Visible;
                    PresetNameLabel.Visibility = Visibility.Visible;
                    PresetNameTextBox.Visibility = Visibility.Visible;
                    RectangleEdit.Visibility = Visibility.Visible;
                    PresetSaveButton.Visibility = Visibility.Visible;
                    PresetDeleteButton.Visibility = Visibility.Visible;
                    PresetExploreButton.Visibility = Visibility.Visible;

                    PresetNameTextBox.Text = preset.Name;
                }
            }
            else
            {
                PresetGrid.Visibility = Visibility.Collapsed;
                PresetAddButton.Visibility = Visibility.Visible;
            }
        }
    }
}
