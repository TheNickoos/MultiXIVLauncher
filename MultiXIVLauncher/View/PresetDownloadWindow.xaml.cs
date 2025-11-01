using MultiXIVLauncher.Models;
using MultiXIVLauncher.Services;
using MultiXIVLauncher.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MultiXIVLauncher.View
{
    public partial class PresetDownloadWindow : Window
    {
        private readonly Preset targetPreset;

        private class RemotePreset
        {
            public string Name { get; set; } = "";
            public string Author { get; set; } = "";
            public string Description { get; set; } = "";
            public string Version { get; set; } = "";
            public string DownloadUrl { get; set; } = "";
            public List<string> Plugins { get; set; } = new();
        }

        public PresetDownloadWindow(Preset preset)
        {
            InitializeComponent();
            targetPreset = preset ?? throw new ArgumentNullException(nameof(preset));

            _ = LoadPresetsAsync();
        }

        private async Task LoadPresetsAsync()
        {
            string url = "https://raw.githubusercontent.com/TheNickoos/MultiXIVPresets/main/presets.json";

            try
            {
                using var client = new WebClient();
                string json = await client.DownloadStringTaskAsync(url);
                var presets = JsonSerializer.Deserialize<List<RemotePreset>>(json);

                if (presets == null || presets.Count == 0)
                {
                    MessageBox.Show("No downloadable presets found.", "Information",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                foreach (var preset in presets)
                    AddPresetCard(preset);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load presets list:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddPresetCard(RemotePreset preset)
        {
            Border card = new()
            {
                Style = (Style)FindResource("CardBorder"),
                Margin = new Thickness(0, 0, 0, 12),
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(185, 147, 255),
                    BlurRadius = 20,
                    ShadowDepth = 0,
                    Opacity = 0.7
                },
                Padding = new Thickness(10)
            };

            StackPanel panel = new() { Orientation = Orientation.Vertical };

            panel.Children.Add(new TextBlock
            {
                Text = $"{preset.Name} (v{preset.Version})",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("ClrTextPrimary")
            });

            panel.Children.Add(new TextBlock
            {
                Text = $"By {preset.Author}",
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 2, 0, 4)
            });

            panel.Children.Add(new TextBlock
            {
                Text = preset.Description,
                TextWrapping = TextWrapping.Wrap,
                Foreground = (Brush)FindResource("ClrTextSecondary"),
                Margin = new Thickness(0, 0, 0, 6)
            });

            if (preset.Plugins.Count > 0)
            {
                panel.Children.Add(new TextBlock
                {
                    Text = $"Plugins: {string.Join(", ", preset.Plugins)}",
                    Foreground = (Brush)FindResource("ClrTextSecondary"),
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(0, 0, 0, 8)
                });
            }

            ProgressBar progressBar = new()
            {
                Height = 8,
                Margin = new Thickness(0, 6, 0, 6),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Visibility = Visibility.Collapsed
            };

            Button downloadButton = new()
            {
                Content = "Download",
                Height = 30,
                Width = 120,
                Style = (Style)FindResource("RoundedButton"),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            downloadButton.Click += async (s, e) =>
            {
                progressBar.Visibility = Visibility.Visible;
                downloadButton.IsEnabled = false;

                await DownloadPresetAsync(preset, progressBar);

                downloadButton.Content = "Installed";
                await Task.Delay(700);

                // Sécurisé : pas d'exception si la fenêtre n'est pas en modal
                SetDialogResultSafe(true);
            };

            panel.Children.Add(progressBar);
            panel.Children.Add(downloadButton);
            card.Child = panel;
            PresetsListPanel.Children.Add(card);
        }

        private async Task DownloadPresetAsync(RemotePreset preset, ProgressBar progressBar)
        {
            string tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            string presetZip = Path.Combine(tempDir, $"{preset.Name}.zip");
            string extractDir = Path.Combine(tempDir, preset.Name.Replace(' ', '_'));

            try
            {
                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);

                if (File.Exists(presetZip))
                    File.Delete(presetZip);

                using (var client = new WebClient())
                {
                    client.DownloadProgressChanged += (s, e) =>
                    {
                        progressBar.Value = e.ProgressPercentage;
                    };
                    await client.DownloadFileTaskAsync(new Uri(preset.DownloadUrl), presetZip);
                }

                progressBar.Value = 100;

                if (Directory.Exists(extractDir))
                    Directory.Delete(extractDir, true);

                ZipFile.ExtractToDirectory(presetZip, extractDir);

                // Copy to target preset directory
                if (!Directory.Exists(targetPreset.FolderPath))
                    Directory.CreateDirectory(targetPreset.FolderPath);

                CopyDirectory(extractDir, targetPreset.FolderPath, true);

                // Clean up temp files
                File.Delete(presetZip);
                Directory.Delete(extractDir, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while downloading or installing preset:\n{ex.Message}",
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

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            // Sécurisé : supporte ShowDialog() et Show()
            SetDialogResultSafe(false);
        }

        /// <summary>
        /// Définit DialogResult en toute sécurité (ShowDialog) sinon ferme proprement (Show),
        /// tout en garantissant l’exécution sur le thread UI.
        /// </summary>
        private void SetDialogResultSafe(bool? result)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SetDialogResultSafe(result));
                return;
            }

            if (!IsLoaded || !IsVisible)
            {
                try { Close(); } catch { /* ignore */ }
                return;
            }

            try
            {
                if (result.HasValue)
                    DialogResult = result; // OK si ouverte via ShowDialog()
                else
                    Close();
            }
            catch (InvalidOperationException)
            {
                // Pas en modal (ouverte via Show()) -> on ferme sans DialogResult
                try { Close(); } catch { /* ignore */ }
            }
        }
    }
}
