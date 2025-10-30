using MultiXIVLauncher.Models;
using MultiXIVLauncher.Services;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.TextFormatting;

namespace MultiXIVLauncher.View
{
    /// <summary>
    /// Modal window allowing the user to select a character to copy configuration from.
    /// </summary>
    public partial class PresetEditSelectCharacterWindow : Window
    {
        /// <summary>
        /// Gets the selected character after validation.
        /// </summary>
        public Character SelectedCharacter { get; private set; }

        public PresetEditSelectCharacterWindow()
        {
            InitializeComponent();
            LoadCharacters();
        }

        /// <summary>
        /// Loads all existing characters from the configuration.
        /// </summary>
        private void LoadCharacters()
        {
            CharacterList.Items.Clear();

            List<Character> characters = ConfigManager.Current.Characters;
            if (characters == null || characters.Count == 0)
            {
                CharacterList.Items.Add("No characters available");
                CharacterList.IsEnabled = false;
                return;
            }

            foreach (var character in characters)
            {
                CharacterList.Items.Add(character.Name);
            }
        }

        private void Validate_Click(object sender, RoutedEventArgs e)
        {
            if (CharacterList.SelectedItem == null)
            {
                MessageBox.Show("Please select a character.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string selectedName = CharacterList.SelectedItem.ToString();
            SelectedCharacter = ConfigManager.Current.Characters.Find(c => c.Name == selectedName);

            if (SelectedCharacter == null)
            {
                MessageBox.Show("An error occurred while retrieving the character.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
