using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Xml;

namespace MultiXIVLauncher
{
    public partial class App : Application
    {
        private string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!File.Exists(configPath))
            {
                var defaultConfig = new Config();
                string json = JsonConvert.SerializeObject(defaultConfig, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(configPath, json);
            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
