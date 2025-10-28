namespace MultiXIVLauncher.Models
{
    public class Preset
    {
        /// <summary>
        /// Internal unique identifier of the preset in the configuration.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Display name of the preset.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Path to the folder containing the preset files.
        /// </summary>
        public string FolderPath { get; set; }

        public Preset()
        {
            Name = "";
            FolderPath = "";
        }
    }
}
