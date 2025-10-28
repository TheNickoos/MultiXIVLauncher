using MultiXIVLauncher.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiXIVLauncher.Models
{
    public class Character
    {
        /// <summary>
        /// Internal unique identifier of the character in the configuration.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Character name displayed in the launcher.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Lodestone ID (FFXIV) used to retrieve the character profile or portrait.
        /// </summary>
        public int LodestoneId { get; set; }

        /// <summary>
        /// ID of the preset linked to this character (optional).
        /// </summary>
        public int PresetId { get; set; }

        /// <summary>
        /// List of group IDs the character belongs to.
        /// </summary>
        public List<int> GroupIds { get; set; }

        /// <summary>
        /// Current character class or job (e.g. Bard, Paladin).
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// Server where the character is located.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Current character level.
        /// </summary>
        public int Level { get; set; }

#pragma warning disable CS8618
        public Character()
#pragma warning restore CS8618
        {
            Name = "";
            GroupIds = new List<int>();
        }

        /// <summary>
        /// Creates a new character instance with an automatically generated unique ID.
        /// </summary>
        /// <param name="name">Name of the new character.</param>
        /// <returns>A new instance of <see cref="Character"/> with a unique ID.</returns>
        public static Character Create(string name)
        {
            return new Character
            {
                Id = GenerateNextId(),
                Name = name,
                LodestoneId = 0,
                PresetId = 0,
                GroupIds = new List<int>()
            };
        }

        /// <summary>
        /// Generates a new positive integer ID based on existing characters in the configuration.
        /// </summary>
        private static int GenerateNextId()
        {
            if (ConfigManager.Current?.Characters == null || ConfigManager.Current.Characters.Count == 0)
                return 1;

            return ConfigManager.Current.Characters.Max(c => c.Id) + 1;
        }
    }
}
