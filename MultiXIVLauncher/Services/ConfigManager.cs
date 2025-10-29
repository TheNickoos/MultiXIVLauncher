using MultiXIVLauncher.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Xml;

namespace MultiXIVLauncher.Services
{
    public static class ConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        private static readonly object _lock = new();
        private static Config? _config;

        private const int CURRENT_VERSION = 2;

        public static Config Current => _config ?? throw new InvalidOperationException("Configuration not loaded.");

        /// <summary>
        /// Loads the configuration file from disk and applies upgrades if necessary.
        /// </summary>
        public static void Load()
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(ConfigPath))
                    {
                        _config = CreateDefaultConfig();
                        Save();
                        Logger.Info("No configuration file found. Default configuration created.");
                        return;
                    }

                    string json = File.ReadAllText(ConfigPath);
                    _config = JsonConvert.DeserializeObject<Config>(json) ?? CreateDefaultConfig();

                    bool upgraded = UpgradeIfNeeded();

                    if (upgraded)
                    {
                        Backup();
                        Save();
                        Logger.Info("Configuration file upgraded and saved successfully.");
                    }
                    else
                    {
                        Logger.Info("Configuration loaded successfully, no upgrade required.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to load configuration: " + ex.Message);
                    _config = CreateDefaultConfig();
                    Save();
                }
            }
        }

        /// <summary>
        /// Saves the current configuration to disk and creates a .bak backup.
        /// </summary>
        public static void Save()
        {
            lock (_lock)
            {
                try
                {
                    if (_config == null)
                    {
                        Logger.Warn("No configuration to save (ConfigManager.Current is null).");
                        return;
                    }

                    Backup();

                    string json = JsonConvert.SerializeObject(_config, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(ConfigPath, json);
                    Logger.Info("Configuration saved successfully.");
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to save configuration: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Reloads the configuration from disk, overwriting the current one.
        /// </summary>
        public static void Reload()
        {
            Logger.Info("Reloading configuration...");
            Load();
        }

        /// <summary>
        /// Creates a .bak backup before any write operation.
        /// </summary>
        private static void Backup()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string backupPath = ConfigPath + ".bak";
                    File.Copy(ConfigPath, backupPath, true);
                    Logger.Info("Backup created: " + backupPath);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to create configuration backup: " + ex.Message);
            }
        }

        /// <summary>
        /// Checks and updates the configuration if it is outdated or incomplete.
        /// </summary>
        private static bool UpgradeIfNeeded()
        {
            bool changed = false;

            try
            {
                if (_config == null)
                {
                    _config = CreateDefaultConfig();
                    Logger.Warn("Configuration was null. Default created.");
                    return true;
                }

                if (_config.Launcher == null)
                {
                    _config.Launcher = new LauncherConfig();
                    Logger.Warn("Launcher section missing. Created default instance.");
                    changed = true;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(_config.Launcher.Language))
                    {
                        _config.Launcher.Language = "en";
                        Logger.Warn("Launcher.Language missing. Set to 'en'.");
                        changed = true;
                    }

                    if (string.IsNullOrWhiteSpace(_config.Launcher.Path))
                    {
                        _config.Launcher.Path = "";
                        Logger.Warn("Launcher.Path missing. Set to empty string.");
                        changed = true;
                    }
                }

                if (_config.Presets == null)
                {
                    _config.Presets = [];
                    Logger.Warn("Presets list missing. Created empty list.");
                    changed = true;
                }

                if (_config.Groups == null)
                {
                    _config.Groups = [];
                    Logger.Warn("Groups list missing. Created empty list.");
                    changed = true;
                }

                if (_config.Characters == null)
                {
                    _config.Characters = [];
                    Logger.Warn("Characters list missing. Created empty list.");
                    changed = true;
                }

                if (_config.Version < CURRENT_VERSION)
                {
                    Logger.Info($"Upgrading configuration version from {_config.Version} to {CURRENT_VERSION}.");
                    _config.Version = CURRENT_VERSION;
                    changed = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during configuration upgrade: " + ex.Message);
            }

            return changed;
        }

        /// <summary>
        /// Creates a new default configuration.
        /// </summary>
        private static Config CreateDefaultConfig()
        {
            return new Config
            {
                Version = CURRENT_VERSION,
                Launcher = new LauncherConfig
                {
                    Language = "en",
                    Path = ""
                },
                Presets = [],
                Groups = [],
                Characters = []
            };
        }

        /// <summary>
        /// Returns the absolute path of the configuration file.
        /// </summary>
        public static string GetConfigPath() => ConfigPath;
    }
}
