using MultiXIVLauncher.Models;
using MultiXIVLauncher.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MultiXIVLauncher.Services
{
    public static class CharacterLauncher
    {
        private static readonly string[] GameProcessNames = { "ffxiv_dx11", "ffxiv" };

        /// <summary>
        /// Lance XIVLauncher dans un environnement "profil personnage" et attend l'apparition du process FFXIV.
        /// Option B: la fermeture de la LoadingWindow annule le lancement via CancellationToken.
        /// </summary>
        public static async Task LaunchCharacterAsync(
            Character character,
            Config config,
            Window ownerWindow,                // ex: MainWindow
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            if (ownerWindow == null) throw new ArgumentNullException(nameof(ownerWindow));
            if (character == null) throw new ArgumentNullException(nameof(character));

            if (config?.Launcher?.Path == null || !File.Exists(config.Launcher.Path))
                throw new FileNotFoundException(Properties.Resources.XIVLauncherNotFoundException);

            // Racine = dossier de l’application (exigence V2)
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string charRoot = Path.Combine(baseDir, "Characters", character.Id.ToString());
            string appDataDir = Path.Combine(charRoot, "AppData");
            string documentsDir = Path.Combine(charRoot, "Documents");
            string ffxivDocs = Path.Combine(documentsDir, "My Games", "FINAL FANTASY XIV - A Realm Reborn");

            Directory.CreateDirectory(appDataDir);
            Directory.CreateDirectory(ffxivDocs);

            // Baseline: PIDs des jeux déjà présents pour les exclure.
            var baseline = SnapshotGamePids();

            var psi = new ProcessStartInfo
            {
                FileName = config.Launcher.Path,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(config.Launcher.Path) ?? baseDir
            };

            // Variables d’environnement pour "sandboxer" le profil
            psi.EnvironmentVariables["USERPROFILE"] = charRoot;
            psi.EnvironmentVariables["HOMEPATH"] = charRoot;
            psi.EnvironmentVariables["HOMEDRIVE"] = Path.GetPathRoot(charRoot) ?? "C:\\";
            psi.EnvironmentVariables["APPDATA"] = appDataDir;
            psi.EnvironmentVariables["LOCALAPPDATA"] = appDataDir;
            psi.EnvironmentVariables["DOCUMENTS"] = documentsDir;

            ToggleInteractivity(ownerWindow, false);

            LoadingWindow loading = null;
            CancellationTokenSource cts = null;
            try
            {
                // Token combiné : annulation externe + annulation via fermeture de la LoadingWindow
                cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var effectiveToken = cts.Token;

                loading = new LoadingWindow { Owner = ownerWindow, CancellationSource = cts };
                loading.Show();

                // Lancement de XIVLauncher
                var launcher = Process.Start(psi);
                if (launcher == null)
                    throw new InvalidOperationException("Impossible de démarrer XIVLauncher.");

                var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(90);

                // Attend l’apparition d’un NOUVEAU process FFXIV (dx11 ou non)
                var gameProc = await WaitForGameStartAsync(
                    baseline,
                    effectiveTimeout,
                    progress: title => loading.SetStatus(title),
                    cancellationToken: effectiveToken);

                // Enregistrer le process pour usage ultérieur (si tu utilises la registry)
                CharacterProcessRegistry.Track(character.Id.ToString(), gameProc);

                // Optionnel : attendre la fenêtre principale du jeu
                // await CharacterLaunchMonitor.WaitForFFXIVWindowAsync(gameProc.Id, loading, effectiveToken);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show(Properties.Resources.ActionCanceled, Properties.Resources.MonitoringError,
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.MonitoringErrorMessage, ex.Message),
                    Properties.Resources.MonitoringError,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                try { loading?.Close(); } catch { /* ignore */ }
                ToggleInteractivity(ownerWindow, true);
                cts?.Dispose();
            }
        }

        /// <summary>
        /// Attend l’apparition d’un nouveau process FFXIV par rapport à une baseline de PIDs.
        /// </summary>
        private static async Task<Process> WaitForGameStartAsync(
            HashSet<int> baselinePids,
            TimeSpan timeout,
            Action<string> progress = null,
            CancellationToken cancellationToken = default)
        {
            var start = DateTime.UtcNow;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Cherche un nouveau process du jeu non présent dans la baseline
                var current = GameProcessNames
                    .SelectMany(Process.GetProcessesByName)
                    .Where(p => !baselinePids.Contains(p.Id))
                    .ToList();

                // Optionnel: filtrer ceux qui ont un MainWindowHandle non nul (indique une fenêtre créée)
                var candidate = current.FirstOrDefault(p =>
                {
                    try { return p.MainWindowHandle != IntPtr.Zero || !p.HasExited; } catch { return false; }
                });

                if (candidate != null)
                    return candidate;

                if (DateTime.UtcNow - start > timeout)
                    throw new TimeoutException("Le lancement du jeu a dépassé le délai imparti.");

                progress?.Invoke(Properties.Resources.LauncherStatus_WaitingForGame); // ex: "Démarrage du jeu…"
                await Task.Delay(1500, cancellationToken);
            }
        }

        private static HashSet<int> SnapshotGamePids()
        {
            return new HashSet<int>(
                GameProcessNames
                    .SelectMany(Process.GetProcessesByName)
                    .Select(p => p.Id));
        }

        /// <summary>
        /// Tente d’appeler SetLauncherInteractivity(bool) si dispo, sinon bascule IsEnabled.
        /// </summary>
        private static void ToggleInteractivity(Window owner, bool enable)
        {
            if (owner == null) return;

            try
            {
                var mi = owner.GetType().GetMethod(
                    "SetLauncherInteractivity",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    binder: null,
                    types: new[] { typeof(bool) },
                    modifiers: null);

                if (mi != null)
                {
                    if (owner.Dispatcher.CheckAccess())
                        mi.Invoke(owner, new object[] { enable });
                    else
                        owner.Dispatcher.Invoke(() => mi.Invoke(owner, new object[] { enable }));
                    return;
                }
            }
            catch
            {
                // fallback
            }

            if (owner.Dispatcher.CheckAccess())
                owner.IsEnabled = enable;
            else
                owner.Dispatcher.Invoke(() => owner.IsEnabled = enable);
        }
    }
}
