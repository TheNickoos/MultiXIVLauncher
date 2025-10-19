using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MultiXIVLauncher
{
    public static class CharacterLaunchMonitor
    {
        public static async Task WaitForFFXIVWindowAsync(int launcherPid, Window loadingWindow, CancellationToken token)
        {
            try
            {
                var existingFFXIV = Process.GetProcessesByName("ffxiv_dx11")
                                           .Concat(Process.GetProcessesByName("ffxiv"))
                                           .Select(p => p.Id)
                                           .ToList();

                DateTime start = DateTime.Now;
                int timeoutSec = 90;

                while (!token.IsCancellationRequested)
                {
                    var newFFXIV = Process.GetProcessesByName("ffxiv_dx11")
                                          .Concat(Process.GetProcessesByName("ffxiv"))
                                          .Where(p => !existingFFXIV.Contains(p.Id))
                                          .ToList();

                    if (newFFXIV.Any())
                    {
                        loadingWindow.Dispatcher.Invoke(() => loadingWindow.Close());
                        return;
                    }

                    if ((DateTime.Now - start).TotalSeconds > timeoutSec)
                    {
                        MessageBox.Show("Timeout: FFXIV did not start within the expected delay.",
                                        "Timeout",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                        loadingWindow.Dispatcher.Invoke(() => loadingWindow.Close());
                        return;
                    }

                    await Task.Delay(2000, token);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Monitoring error: {ex.Message}",
                                "Monitor Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                loadingWindow.Dispatcher.Invoke(() => loadingWindow.Close());
            }
        }
    }
}
