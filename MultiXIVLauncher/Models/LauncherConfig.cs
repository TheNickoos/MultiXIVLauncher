namespace MultiXIVLauncher.Models
{
    public class LauncherConfig
    {
        /// <summary>
        /// Path to the XIVLauncher executable.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Current language used by the launcher interface.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the beta access key used to connect to Dalamud’s testing environment.
        /// </summary>
        public string DalamudBetaKey { get; set; }

        /// <summary>
        /// Gets or sets the selected Dalamud beta channel or build type (e.g. “stg”, “canary”).
        /// </summary>
        public string DalamudBetaKind { get; set; }
        public LauncherConfig()
        {
            Path = "";
            Language = "en";
            DalamudBetaKey = "";
            DalamudBetaKind = "";
        }
    }
}
