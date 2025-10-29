using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;
using MultiXIVLauncher.Properties;

namespace MultiXIVLauncher.Services
{
    /// <summary>
    /// Handles all localization and language management for the application.
    /// Provides dynamic translation loading and updates UI text when the language changes.
    /// </summary>
    public static class LanguageManager
    {
        private static ResourceManager? _resourceManager;
        private static CultureInfo? _currentCulture;

        /// <summary>
        /// Gets the current language code (e.g. "en", "fr", "de").
        /// </summary>
        public static string CurrentLanguage { get; private set; } = "en";

        /// <summary>
        /// Triggered when the active language changes.
        /// </summary>
        public static event Action? LanguageChanged;

        /// <summary>
        /// Initializes the language system and applies the specified language.
        /// </summary>
        public static void Initialize(string langCode)
        {
            Apply(langCode);
            Logger.Info("LanguageManager initialized with language: " + langCode);
        }

        /// <summary>
        /// Changes the active language and refreshes all UI bindings.
        /// </summary>
        public static void ChangeLanguage(string langCode)
        {
            Apply(langCode);
            Logger.Info("Language changed to: " + langCode);

            if (LanguageChanged != null)
                LanguageChanged.Invoke();
        }

        /// <summary>
        /// Applies the given language code and sets up thread and resource culture contexts.
        /// </summary>
        private static void Apply(string langCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(langCode))
                    langCode = "en";

                CurrentLanguage = langCode;
                _currentCulture = new CultureInfo(langCode);

                Thread.CurrentThread.CurrentCulture = _currentCulture;
                Thread.CurrentThread.CurrentUICulture = _currentCulture;

                Resources.Culture = _currentCulture;

                _resourceManager = new ResourceManager("MultiXIVLauncher.Properties.Resources", Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Logger.Error("Error applying language: " + ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a translated string for the specified key.
        /// Returns the key name in brackets if the translation is missing.
        /// </summary>
        public static string T(string key)
        {
            try
            {
                if (_resourceManager == null)
                    _resourceManager = new ResourceManager("MultiXIVLauncher.Properties.Resources", Assembly.GetExecutingAssembly());

                string? value = _resourceManager.GetString(key, _currentCulture);
                if (!string.IsNullOrEmpty(value))
                    return value;

                Logger.Warn("Missing translation key: " + key);
                return "[" + key + "]";
            }
            catch (Exception ex)
            {
                Logger.Error("Error reading translation for key '" + key + "': " + ex.Message);
                return "[" + key + "]";
            }
        }
    }
}
