using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MultiXIVLauncher
{
    public partial class PresetDownloadWindow : Window
    {
        private const string PresetRepoUrl = "https://raw.githubusercontent.com/TheNickoos/MultiXIVPresets/main/presets.json";
        private List<PresetInfo> presets = new List<PresetInfo>();
        private PresetInfo selectedPreset;
        private readonly SettingsWindow parentWindow;

        public PresetDownloadWindow(SettingsWindow parent)
        {
            InitializeComponent();
            parentWindow = parent;
            LoadPresetList();
        }

        private async void LoadPresetList()
        {
            try
            {
                PresetListBox.Items.Clear();
                PresetListBox.Items.Add("Loading presets...");

                string json = await DownloadStringAsync(PresetRepoUrl);
                presets = JsonConvert.DeserializeObject<List<PresetInfo>>(json);

                PresetListBox.Items.Clear();

                if (presets == null || presets.Count == 0)
                {
                    PresetListBox.Items.Add("No presets available.");
                    return;
                }

                foreach (var p in presets)
                    PresetListBox.Items.Add(p.Name);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load presets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<string> DownloadStringAsync(string url)
        {
            using (var client = new WebClient())
                return await client.DownloadStringTaskAsync(url);
        }

        private void PresetListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PresetListBox.SelectedIndex == -1)
                return;

            var presetName = PresetListBox.SelectedItem.ToString();
            selectedPreset = presets.Find(p => p.Name == presetName);

            if (selectedPreset == null)
                return;

            PresetNameText.Text = selectedPreset.Name;
            PresetAuthorText.Text = selectedPreset.Author;
            PresetVersionText.Text = selectedPreset.Version;
            PresetDescriptionText.Text = selectedPreset.Description;

            PresetPluginsList.Items.Clear();
            if (selectedPreset.Plugins != null && selectedPreset.Plugins.Count > 0)
            {
                foreach (var plugin in selectedPreset.Plugins)
                    PresetPluginsList.Items.Add(plugin);
            }
            else
            {
                PresetPluginsList.Items.Add("No plugin list available.");
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPreset == null)
            {
                MessageBox.Show("Please select a preset first.", "No Preset Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                DownloadButton.IsEnabled = false;
                DownloadButton.Content = "Downloading...";
                DownloadProgress.Value = 0;
                DownloadProgress.Visibility = Visibility.Visible;

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                string safeName = selectedPreset.Name;
                foreach (char c in Path.GetInvalidFileNameChars())
                    safeName = safeName.Replace(c, '_');
                safeName = safeName.Replace(" ", "_");

                string presetDir = Path.Combine(baseDir, "Presets", safeName);
                string zipPath = Path.Combine(presetDir, "preset.zip");

                if (Directory.Exists(presetDir))
                    Directory.Delete(presetDir, true);
                Directory.CreateDirectory(presetDir);

                await DownloadFileWithProgress(selectedPreset.DownloadUrl, zipPath);

                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        string destPath = Path.Combine(presetDir, entry.FullName);
                        string dir = Path.GetDirectoryName(destPath);
                        if (!string.IsNullOrEmpty(dir))
                            Directory.CreateDirectory(dir);

                        if (!string.IsNullOrEmpty(entry.Name))
                            entry.ExtractToFile(destPath, true);
                    }
                }

                File.Delete(zipPath);

                string configPath = Path.Combine(baseDir, "config.json");
                var config = Config.Load(configPath) ?? new Config();
                if (config.Presets == null) config.Presets = new List<Preset>();

                int newId = config.Presets.Count > 0
                    ? config.Presets[config.Presets.Count - 1].Id + 1
                    : 1;

                               
                var newPreset = new Preset
                {
                    Id = parentWindow.config.Presets.Count > 0
                        ? parentWindow.config.Presets[parentWindow.config.Presets.Count - 1].Id + 1
                        : 1,
                    Name = selectedPreset.Name,
                    FolderPath = presetDir
                };

                parentWindow.config.Presets.Add(newPreset);
                parentWindow.config.Save(parentWindow.configPath);

                
                parentWindow.LoadPresets();


                MessageBox.Show(
                    "Preset \"" + selectedPreset.Name + "\" successfully downloaded, extracted and added to your presets list.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while downloading preset: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DownloadButton.IsEnabled = true;
                DownloadButton.Content = "Download this preset";
                DownloadProgress.Visibility = Visibility.Collapsed;
            }
        }

        private async Task DownloadFileWithProgress(string url, string destination)
        {
            using (var client = new WebClient())
            {
                client.DownloadProgressChanged += (s, e) =>
                {
                    DownloadProgress.Value = e.ProgressPercentage;
                };
                await client.DownloadFileTaskAsync(new Uri(url), destination);
            }
        }
    }

    public class PresetInfo
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string DownloadUrl { get; set; }
        public List<string> Plugins { get; set; } = new List<string>();
    }
}
