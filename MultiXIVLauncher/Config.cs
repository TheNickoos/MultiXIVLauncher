using System.Collections.Generic;

namespace MultiXIVLauncher
{
    public class Config
    {
        public LauncherSettings Launcher { get; set; } = new LauncherSettings();
        public List<Preset> Presets { get; set; } = new List<Preset>();
        public List<Group> Groups { get; set; } = new List<Group>();
        public List<Character> Characters { get; set; } = new List<Character>();
        public string ConfigVersion { get; set; } = "1.0";
    }

    public class LauncherSettings
    {
        public string Path { get; set; } = "";
        public string Language { get; set; } = "en";
    }

    public class Preset
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string FolderPath { get; set; } = "";
    }

    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class Character
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int GroupId { get; set; }
    }
}
