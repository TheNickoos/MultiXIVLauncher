using MultiXIVLauncher.Services;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;

namespace MultiXIVLauncher
{
    /// <summary>
    /// Main entry point for the application.
    /// Handles startup, configuration loading, and language initialization.
    /// </summary>
    public partial class App : Application
    {
#if DEBUG
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
#endif
        /// <summary>
        /// Invoked when the application starts.
        /// Initializes error handling, loads user configuration, applies localization settings,
        /// and opens the main launcher window.
        /// </summary>
        /// <param name="e">Startup event arguments.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
#if DEBUG
            AllocConsole();
#endif
            ErrorManager.Initialize();
            ConfigManager.Load();
            LanguageManager.Initialize(ConfigManager.Current.Launcher.Language);
            
            new LauncherWindow().Show();
            Logger.Info("Application startup complete.");

        }
    }
}
