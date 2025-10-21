using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;

namespace MultiXIVLauncher
{
    public partial class App : Application
    {
        private readonly string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!File.Exists(configPath))
            {
                var defaultConfig = new Config();
                string json = JsonConvert.SerializeObject(defaultConfig, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(configPath, json);
            }

            var config = Config.Load(configPath);

            if (config?.Launcher?.Language != null)
                ApplyLanguage(config.Launcher.Language);
            else
                ApplyLanguage("en");

            base.OnStartup(e);

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private static void ApplyLanguage(string langCode)
        {
            try
            {
                var culture = new CultureInfo(langCode);
                Thread.CurrentThread.CurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;

                MultiXIVLauncher.Properties.Resources.Culture = culture;

                // Empty cache of ResourceManager
                var field = typeof(Properties.Resources)
                    .GetField("resourceMan", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                field?.SetValue(null, null);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error applying language: " + ex.Message);
            }
        }

    }
}
