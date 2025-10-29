using MultiXIVLauncher.Models;
using MultiXIVLauncher.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MultiXIVLauncher.Views
{
    /// <summary>
    /// Window that allows the user to select a character to add to a group.
    /// Displays only characters not already part of the group.
    /// </summary>
    public partial class AddMemberWindow : Window
    {
        /// <summary>
        /// Gets the character selected by the user.
        /// </summary>
        public Character? SelectedCharacter { get; private set; }

        private readonly List<string> AvailableCharacterNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddMemberWindow"/> class,
        /// populating the character list dynamically from the provided names.
        /// </summary>
        /// <param name="availableCharacterNames">List of character names available for selection.</param>
        public AddMemberWindow(List<string> availableCharacterNames)
        {
            InitializeComponent();

            AvailableCharacterNames = availableCharacterNames ?? [];

            foreach (var name in AvailableCharacterNames)
                CharacterList.Items.Add(name);
        }

        /// <summary>
        /// Validates the user's selection and closes the window with a positive result.
        /// Shows a warning message if no character is selected.
        /// </summary>
        private void Validate_Click(object sender, RoutedEventArgs e)
        {
            if (CharacterList.SelectedItem is string name)
            {
                // Retrieve full character from ConfigManager (only for reference)
                var existingChar = ConfigManager.Current.Characters.FirstOrDefault(c => c.Name == name);

                // Create a simplified character if not found (shouldn't happen with proper sync)
                SelectedCharacter = existingChar ?? new Character { Name = name };

                DialogResult = true;
            }
            else
            {
                MessageBox.Show(
                    LanguageManager.T("Window_Member_Add_MessageBox_SelectChar"),
                    LanguageManager.T("Warning"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }
    }
}
