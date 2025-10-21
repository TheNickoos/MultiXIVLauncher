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
        public static async Task LaunchCharacter(Character character, Config config, MainWindow mainWindow)
        {
            if (character == null)
            {
                MessageBox.Show(Properties.Resources.NoCharacter,
                    Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (config == null || config.Launcher == null || string.IsNullOrEmpty(config.Launcher.Path) || !File.Exists(config.Launcher.Path))
            {
                MessageBox.Show(Properties.Resources.XIVLauncherNotFoundException,
                    Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string characterDir = Path.Combine(baseDir, "Characters", $"Character_{character.Id}");

            if (!Directory.Exists(characterDir))
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.NoCharacterAssigned, character.Name),
                    Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                mainWindow.SetLauncherInteractivity(false);

                var loadingWindow = new LoadingWindow
                {
                    Owner = mainWindow
                };
                loadingWindow.Show();

                string xivLauncherPath = config.Launcher.Path;
                string args = $"--data \"{characterDir}\"";

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = xivLauncherPath,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(processStartInfo);

                if (process == null)
                    throw new Exception("Failed to start XIVLauncher process.");

                // Surveillance du démarrage
                bool launched = await WaitForGameStartAsync(process, TimeSpan.FromSeconds(15));
                loadingWindow.Close();

                if (!launched)
                {
                    MessageBox.Show(Properties.Resources.CharLaunchTimeout,
                        Properties.Resources.Timeout, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show(Properties.Resources.AllCharLaunched,
                        Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorLaunchingChar, ex.Message),
                    Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                mainWindow.SetLauncherInteractivity(true);
            }
        }

        private static async Task<bool> WaitForGameStartAsync(Process process, TimeSpan timeout)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                while (!process.HasExited && stopwatch.Elapsed < timeout)
                {
                    await Task.Delay(1000);

                    // Ici tu pourrais ajouter une vérification si FFXIV.exe est lancé
                    // (en cherchant le processus, par exemple)
                    var ffxiv = Process.GetProcessesByName("ffxiv_dx11");
                    if (ffxiv.Length > 0)
                        return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.MonitoringErrorMessage, ex.Message),
                    Properties.Resources.MonitoringError, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
