using System;
using System.Diagnostics;
using System.IO;

namespace MultiXIVLauncher.Services
{
    /// <summary>
    /// Provides simple file and console logging for the application.
    /// Supports four log levels: Info, Warn, Error, and Fatal.
    /// </summary>
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MultiXIVLauncher.log");
        private static readonly object _lock = new object();

        /// <summary>
        /// Writes an informational message to the log.
        /// </summary>
        public static void Info(string message)
        {
            Write("Info", message);
        }

        /// <summary>
        /// Writes a warning message to the log.
        /// </summary>
        public static void Warn(string message)
        {
            Write("Warn", message);
        }

        /// <summary>
        /// Writes an error message to the log.
        /// </summary>
        public static void Error(string message)
        {
            Write("Error", message);
        }

        /// <summary>
        /// Writes a fatal (critical) message to the log.
        /// </summary>
        public static void Fatal(string message)
        {
            Write("Fatal", message);
        }

        /// <summary>
        /// Writes a formatted log entry to the log file and, in debug mode, to the console.
        /// </summary>
        private static void Write(string level, string message)
        {
            string line = string.Format("[{0:HH:mm}] [{1}] {2}", DateTime.Now, level, message);

#if DEBUG
            if (Environment.UserInteractive)
                Console.WriteLine(line);
            else
                Debug.WriteLine(line);
#endif

            try
            {
                lock (_lock)
                {
                    File.AppendAllText(LogFilePath, line + Environment.NewLine);
                }
            }
            catch
            {
                // Silently ignore logging failures.
            }
        }
    }
}
