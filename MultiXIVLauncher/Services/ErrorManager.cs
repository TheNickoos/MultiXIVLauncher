using System;
using System.Threading.Tasks;
using System.Windows;
using MultiXIVLauncher.Services;

namespace MultiXIVLauncher.Services
{
    public static class ErrorManager
    {
        private static bool _initialized = false;

        /// <summary>
        /// Initializes global error handling for the entire application.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        /// <summary>
        /// Handles unhandled exceptions that occur on the main UI thread.
        /// </summary>
        private static void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogException("UI Thread crash", e.Exception);
            ShowErrorMessage(e.Exception);
            e.Handled = true;
        }

        /// <summary>
        /// Handles unhandled exceptions from non-UI threads.
        /// </summary>
        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                LogException("Non-UI Thread crash", ex);
            else
                Logger.Fatal("Unknown non-UI error occurred.");
        }


        /// <summary>
        /// Handles unobserved task exceptions (e.g. from async tasks).
        /// </summary>
        private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            LogException("Unobserved Task Exception", e.Exception);
            e.SetObserved();
        }

        /// <summary>
        /// Logs detailed information about an exception to the application logger.
        /// </summary>
        private static void LogException(string type, Exception ex)
        {
            string message = ex.Message + Environment.NewLine + ex.StackTrace;
            Logger.Fatal(type + ": " + message);
        }

        /// <summary>
        /// Displays a user-friendly error message in a MessageBox.
        /// Shows full stack trace in DEBUG mode.
        /// </summary>
        private static void ShowErrorMessage(Exception ex)
        {
#if DEBUG
            MessageBox.Show(
                "An unexpected error has occurred.\n\n" + ex.Message + "\n\n" + ex.StackTrace,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
#else
            MessageBox.Show(
                "An unexpected error has occurred.\n\n" + ex.Message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
#endif
        }
    }
}
