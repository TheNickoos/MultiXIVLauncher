using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace MultiXIVLauncher
{
    public class Config
    {
        public LauncherConfig Launcher { get; set; } = new LauncherConfig();
        public List<Preset> Presets { get; set; } = new List<Preset>();
        public List<Group> Groups { get; set; } = new List<Group>();
        public List<Character> Characters { get; set; } = new List<Character>();

        // Charge le JSON depuis le chemin donné, renvoie config par défaut si fichier manquant ou corrompu
        public static Config Load(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return new Config();

                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<Config>(json) ?? new Config();
            }
            catch
            {
                // Fichier corrompu -> retourne config vide
                return new Config();
            }
        }

        // Sauvegarde la config dans le chemin donné
        public void Save(string path)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }

    public class LauncherConfig
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
        public List<int> GroupIds { get; set; } = new List<int>();
    }
}
