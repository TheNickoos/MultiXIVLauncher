using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using MultiXIVLauncher.Updater.Helpers;

namespace MultiXIVLauncher.Updater
{
    /// <summary>
    /// Updater window that waits for the host app to close, extracts the payload with progress,
    /// and relaunches the main executable once the update completes.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int _pid;
        private readonly string _appDir;
        private readonly string _zipPath;
        private readonly string _exeToRelaunch;

        /// <summary>
        /// Creates the updater window with all required execution parameters.
        /// </summary>
        /// <param name="pid">PID of the running host process to wait for.</param>
        /// <param name="appDir">Application directory where files will be replaced.</param>
        /// <param name="zipPath">Absolute path to the downloaded update ZIP.</param>
        /// <param name="exeToRelaunch">Executable to relaunch after update.</param>
        public MainWindow(string pid, string appDir, string zipPath, string exeToRelaunch)
        {
            InitializeComponent();

            if (!int.TryParse(pid, out _pid)) _pid = -1;
            _appDir = appDir;
            _zipPath = zipPath;
            _exeToRelaunch = exeToRelaunch;

            Loaded += async (_, __) => await RunUpdateAsync();
        }

        /// <summary>
        /// Main update flow: wait for host exit, extract with progress, cleanup, relaunch.
        /// </summary>
        private async Task RunUpdateAsync()
        {
            try
            {
                SetStatus("Waiting for the application to exit…");
                await WaitForProcessToExitAsync(_pid, 15000);

                // Optional: check write permissions early (Program Files may require elevation).
                if (!IsWritable(_appDir))
                    Log("⚠ Target directory may require elevated privileges (Program Files?).");

                SetStatus("Analyzing package…");
                long totalBytes = ZipProgressExtractor.ComputeUncompressedSize(_zipPath);
                Log($"Estimated total uncompressed size: {totalBytes:N0} bytes");

                SetStatus("Extracting and replacing files…");
                var progress = new Progress<double>(p =>
                {
                    Progress.Value = p * 100.0;
                });

                // Extract directly into the app directory (host app should be closed).
                await Task.Run(() =>
                    ZipProgressExtractor.ExtractZipToDirectoryWithProgress(
                        _zipPath, _appDir,
                        overwrite: true,
                        progress: progress,
                        onEntry: s => Log(s)));

                SetStatus("Cleaning up…");
                TryDeleteFile(_zipPath);

                SetStatus("Relaunching application…");
                RelaunchApp(_exeToRelaunch, _appDir);

                SetStatus("Update completed ✔");
                Progress.Value = 100;
                CloseBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Progress.Value = 0;
                SetStatus("Update failed");
                Log("ERROR: " + ex.Message);
                Log(ex.ToString());
                WriteCrashLog(ex);
                CloseBtn.IsEnabled = true;
                MessageBox.Show(
                    "The update has failed.\nA crash log was written in %TEMP%.",
                    "Updater", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Updates the status text on the UI thread.
        /// </summary>
        private void SetStatus(string msg) => Dispatcher.Invoke(() => StatusText.Text = msg);

        /// <summary>
        /// Appends a log line to the on-screen log box.
        /// </summary>
        private void Log(string line)
        {
            Dispatcher.Invoke(() =>
            {
                LogBox.AppendText(line + Environment.NewLine);
                LogBox.ScrollToEnd();
            });
        }

        /// <summary>
        /// Waits for a process to exit up to a timeout. Continues even if still alive.
        /// </summary>
        private static async Task WaitForProcessToExitAsync(int pid, int timeoutMs)
        {
            if (pid <= 0) return;
            try
            {
                var proc = Process.GetProcessById(pid);
                if (!proc.HasExited)
                {
                    proc.WaitForExit(timeoutMs);
                    // If still alive, we continue; extraction will try to proceed.
                    // (Optional: attempt CloseMainWindow/Kill here if you want to force-close.)
                }
            }
            catch { /* already closed or inaccessible */ }
            await Task.Delay(250);
        }

        /// <summary>
        /// Quick write test to detect if the target directory is writable.
        /// </summary>
        private static bool IsWritable(string path)
        {
            try
            {
                var test = Path.Combine(path, ".write_test.tmp");
                File.WriteAllText(test, "ok");
                File.Delete(test);
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Best-effort deletion of a file.
        /// </summary>
        private static void TryDeleteFile(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }

        /// <summary>
        /// Relaunches the main application executable.
        /// </summary>
        private static void RelaunchApp(string exe, string workingDir)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                WorkingDirectory = workingDir,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        /// <summary>
        /// Writes a crash log to %TEMP% for troubleshooting.
        /// </summary>
        private static void WriteCrashLog(Exception ex)
        {
            try
            {
                var p = Path.Combine(Path.GetTempPath(), "mxivl_updater_error.txt");
                File.WriteAllText(p, DateTime.Now + Environment.NewLine + ex);
            }
            catch { }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
    }
}
