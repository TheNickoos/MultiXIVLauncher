using MultiXIVLauncher.Services;
using System;
using System.Windows;

namespace MultiXIVLauncher
{
    public partial class App : Application
    {
#if DEBUG
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
#endif
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
#if DEBUG
            AllocConsole();
#endif

#if !DEBUG
            try
            {
                var updater = new UpdateService
                {
                    PrepareForUpdate = () =>
                    {

                    }
                };

                var result = await updater.CheckAndOfferAsync(
                    new Uri("https://raw.githubusercontent.com/TheNickoos/MultiXIVUpdater/main/latest.json"));

                if (result == UpdateResult.LaunchedUpdater)
                    return;
            }
            catch { /* ignore in prod */ }
#endif

            ErrorManager.Initialize();
            ConfigManager.Load();
            LanguageManager.Initialize(ConfigManager.Current.Launcher.Language);

            new LauncherWindow().Show();
            Logger.Info("Application startup complete.");
        }
    }
}
