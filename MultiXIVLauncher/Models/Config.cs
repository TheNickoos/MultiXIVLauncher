using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MultiXIVLauncher.Models
{
    public class Config
    {
        /// <summary>
        /// Version number of the configuration file.
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Global launcher settings.
        /// </summary>
        public LauncherConfig Launcher { get; set; } = new LauncherConfig();

        /// <summary>
        /// List of all presets saved in the configuration.
        /// </summary>
        public List<Preset> Presets { get; set; } = new List<Preset>();

        /// <summary>
        /// List of all groups defined in the configuration.
        /// </summary>
        public List<Group> Groups { get; set; } = new List<Group>();

        /// <summary>
        /// List of all characters created in the launcher.
        /// </summary>
        public List<Character> Characters { get; set; } = new List<Character>();
    }
}
