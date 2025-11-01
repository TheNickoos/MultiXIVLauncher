using System;
using System.Windows;

namespace MultiXIVLauncher.Updater
{
    /// <summary>
    /// WPF application entry point for the standalone updater UI.
    /// Validates command-line arguments and opens the update window.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Called when the updater starts. Expects exactly four arguments:
        /// <c>&lt;pid&gt; &lt;appDir&gt; &lt;zipPath&gt; &lt;exeToRelaunch&gt;</c>.
        /// If arguments are missing, shows an error and exits with code 2.
        /// </summary>
        /// <param name="e">Startup event args carrying the command-line parameters.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Args: <pid> <appDir> <zipPath> <exeToRelaunch>
            if (e.Args.Length < 4)
            {
                MessageBox.Show(
                    $"Missing arguments ({e.Args.Length}/4).\n" +
                    "Usage:\nUpdater.exe <pid> <appDir> <zipPath> <exeToRelaunch>",
                    "Updater",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Console.WriteLine($"Missing arguments ({e.Args.Length}/4).");
                Shutdown(2);
                return;
            }

            var win = new MainWindow(e.Args[0], e.Args[1], e.Args[2], e.Args[3]);
            MainWindow = win;
            win.Show();
        }
    }
}
