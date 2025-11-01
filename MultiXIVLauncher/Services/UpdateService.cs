using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;                  // MessageBox + Application
using Newtonsoft.Json;

namespace MultiXIVLauncher.Services
{
    /// <summary>
    /// Result of an update check flow.
    /// </summary>
    public enum UpdateResult
    {
        /// <summary>No update is available (or a recoverable error happened while checking).</summary>
        NoUpdate,
        /// <summary>User declined the update or the update could not proceed.</summary>
        Skipped,
        /// <summary>The external Updater executable was launched and the app should not continue initialization.</summary>
        LaunchedUpdater
    }

    /// <summary>
    /// Handles update discovery (via remote manifest), download integrity checks,
    /// and launching the external updater while cleanly shutting down the app.
    /// </summary>
    public sealed class UpdateService
    {
        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        /// <summary>
        /// Optional hook invoked right before launching the updater to release resources
        /// (stop timers/watchers, close file handles, <c>Logger.Shutdown()</c>, etc.).
        /// </summary>
        public Action PrepareForUpdate { get; set; }

        /// <summary>
        /// Remote manifest schema describing the latest available version and payload.
        /// </summary>
        public sealed class UpdateInfo
        {
            /// <summary>Semantic version string (e.g., "1.4.2").</summary>
            public string version { get; set; }
            /// <summary>Direct URL to the update ZIP archive.</summary>
            public string url { get; set; }
            /// <summary>Expected SHA-256 hash of the ZIP file (hex, case-insensitive). If empty, the check is skipped.</summary>
            public string sha256 { get; set; }
            /// <summary>Release notes to display to the user.</summary>
            public string releaseNotes { get; set; }
        }

        /// <summary>
        /// Checks the remote manifest, prompts the user, downloads and verifies the update,
        /// and launches the external updater if appropriate.
        /// </summary>
        /// <param name="manifestUrl">Absolute URL to a JSON manifest describing the latest release.</param>
        /// <returns>
        /// <see cref="UpdateResult.LaunchedUpdater"/> if the updater was started and the app should stop UI initialization;
        /// <see cref="UpdateResult.Skipped"/> if the user declined or the flow couldn’t proceed;
        /// <see cref="UpdateResult.NoUpdate"/> if nothing newer is available or a transient error occurred while checking.
        /// </returns>
        public async Task<UpdateResult> CheckAndOfferAsync(Uri manifestUrl)
        {
            UpdateInfo info;
            try
            {
                var json = await _http.GetStringAsync(manifestUrl).ConfigureAwait(false);
                info = JsonConvert.DeserializeObject<UpdateInfo>(json);
            }
            catch
            {
                // Silent failure (offline/HTTP error): treat as "no update" to avoid blocking startup.
                return UpdateResult.NoUpdate;
            }

            if (!Version.TryParse(info?.version, out var remote))
                return UpdateResult.NoUpdate;

            var local = GetCurrentVersion();
            if (remote <= local)
                return UpdateResult.NoUpdate;

            // Ask user consent on the UI thread.
            var userWantsUpdate = await Application.Current.Dispatcher.InvokeAsync(() =>
                MessageBox.Show(
                    $"New version {remote} is available.\nRelease notes:\n{info.releaseNotes}\n\nInstall now?",
                    "Update",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information) == MessageBoxResult.Yes);

            if (!userWantsUpdate)
                return UpdateResult.Skipped;

            var zipPath = await DownloadAsync(info.url).ConfigureAwait(false);
            if (zipPath == null)
                return UpdateResult.Skipped;

            if (!await VerifySha256Async(zipPath, info.sha256).ConfigureAwait(false))
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    MessageBox.Show("Downloaded file integrity check failed (invalid SHA-256).",
                        "Update", MessageBoxButton.OK, MessageBoxImage.Error));
                return UpdateResult.Skipped;
            }

            LaunchUpdater(zipPath);
            return UpdateResult.LaunchedUpdater; // Signal App.OnStartup to NOT create the main window.
        }

        /// <summary>
        /// Gets the current assembly version used for update comparison.
        /// </summary>
        private Version GetCurrentVersion()
            => typeof(UpdateService).Assembly.GetName().Version ?? new Version(0, 0, 0, 0);

        /// <summary>
        /// Downloads the update payload to a temporary ZIP file.
        /// </summary>
        /// <param name="url">Direct URL to the ZIP archive.</param>
        /// <returns>Absolute temp file path if successful; otherwise <c>null</c>.</returns>
        private async Task<string> DownloadAsync(string url)
        {
            try
            {
                var tmp = Path.Combine(Path.GetTempPath(), "MultiXIVLauncher.update.zip");
                using (var resp = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                {
                    resp.EnsureSuccessStatusCode();
                    using var fs = File.Create(tmp);
                    await resp.Content.CopyToAsync(fs).ConfigureAwait(false);
                }
                return tmp;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Verifies the SHA-256 checksum of a file against the provided expected hex string.
        /// </summary>
        /// <param name="file">Absolute file path.</param>
        /// <param name="expectedHex">Expected SHA-256 in hexadecimal (case-insensitive). If null/empty, verification is skipped.</param>
        /// <returns><c>true</c> if the checksum matches or verification is skipped; otherwise <c>false</c>.</returns>
        private async Task<bool> VerifySha256Async(string file, string expectedHex)
        {
            if (string.IsNullOrWhiteSpace(expectedHex)) return true; // Non-strict mode if no checksum provided.
            try
            {
                using var algo = SHA256.Create();
                await using var fs = File.OpenRead(file);
                var hash = await algo.ComputeHashAsync(fs).ConfigureAwait(false);
                var hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                return string.Equals(hex, expectedHex.ToLowerInvariant(), StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Quotes a path for command-line usage, trimming any trailing backslash to avoid escaping the closing quote.
        /// </summary>
        private static string Q(string s) => "\"" + s.TrimEnd('\\') + "\"";

        /// <summary>
        /// Launches the external Updater executable and terminates the current application.
        /// </summary>
        /// <param name="zipPath">Absolute path to the downloaded update ZIP.</param>
        private void LaunchUpdater(string zipPath)
        {
            // Allow the host app to release resources before replacement.
            try { PrepareForUpdate?.Invoke(); } catch { /* best-effort */ }

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var updaterExe = Path.Combine(baseDir, "Updater.exe");
            if (!File.Exists(updaterExe))
            {
                // Show on UI thread to avoid cross-thread surprises.
                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show("Updater.exe not found.", "Update",
                        MessageBoxButton.OK, MessageBoxImage.Error));
                return;
            }

            var exeToRelaunch = Path.Combine(baseDir, "MultiXIVLauncher.exe");

            var psi = new ProcessStartInfo
            {
                FileName = updaterExe,
                Arguments = $"{Process.GetCurrentProcess().Id} {Q(baseDir)} {Q(zipPath)} {Q(exeToRelaunch)}",
                UseShellExecute = true // detach cleanly; avoid inheritable handle redirections
            };

            Process.Start(psi);

            // Close the app on the UI thread so the main window never opens during update.
            Application.Current.Dispatcher.Invoke(() =>
            {
                try { Application.Current.Shutdown(); } catch { }
            });

            // Safety net in case something still keeps the process alive.
            Environment.Exit(0);
        }
    }
}
