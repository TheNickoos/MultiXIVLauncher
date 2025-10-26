using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MultiXIVLauncher
{
    public static class CharacterLauncher
    {
        public static async void LaunchCharacter(Character character, Config config, MainWindow mainWindow)
        {
            try
            {
                if (mainWindow == null)
                    throw new ArgumentNullException(nameof(mainWindow));

                mainWindow.SetLauncherInteractivity(false);

                if (character == null)
                    throw new ArgumentNullException(nameof(character));

                if (config?.Launcher?.Path == null || !File.Exists(config.Launcher.Path))
                    throw new FileNotFoundException(Properties.Resources.XIVLauncherNotFoundException);

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string charDir = Path.Combine(baseDir, "Characters", $"Character_{character.Id}");
                string appDataDir = Path.Combine(charDir, "AppData");
                string documentsDir = Path.Combine(charDir, "Documents", "My Games", "FINAL FANTASY XIV - A Realm Reborn");

                Directory.CreateDirectory(appDataDir);
                Directory.CreateDirectory(documentsDir);

                var psi = new ProcessStartInfo
                {
                    FileName = config.Launcher.Path,
                    UseShellExecute = false
                };
                psi.EnvironmentVariables["APPDATA"] = appDataDir;
                psi.EnvironmentVariables["LOCALAPPDATA"] = appDataDir;
                psi.EnvironmentVariables["USERPROFILE"] = charDir;
                psi.EnvironmentVariables["HOMEPATH"] = charDir;
                psi.EnvironmentVariables["HOMEDRIVE"] = Path.GetPathRoot(charDir);
                psi.EnvironmentVariables["DOCUMENTS"] = Path.Combine(charDir, "Documents");

                var loadingWindow = new LoadingWindow { Owner = mainWindow };
                loadingWindow.Show();

                var process = Process.Start(psi);
                int launcherPid = process.Id;

                using (var cts = new CancellationTokenSource())
                {
                    await CharacterLaunchMonitor.WaitForFFXIVWindowAsync(launcherPid, loadingWindow, cts.Token);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.MonitoringErrorMessage, ex.Message),
                                Properties.Resources.MonitoringError,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            finally
            {
                mainWindow.SetLauncherInteractivity(true);
            }
        }
    }
}