using MultiXIVLauncher.Models;
using MultiXIVLauncher.Services;
using System.Windows;

namespace MultiXIVLauncher.Views
{
    /// <summary>
    /// Interaction logic for the window that allows users to add a character to a group.
    /// </summary>
    public partial class AddMemberWindow : Window
    {
        /// <summary>
        /// Gets the character selected by the user.
        /// </summary>
        public Character SelectedCharacter { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddMemberWindow"/> class
        /// and populates the character selection list.
        /// </summary>
        public AddMemberWindow()
        {
            InitializeComponent();

            CharacterList.Items.Add("Lyna Jade");
            CharacterList.Items.Add("Miles Brenner");
            CharacterList.Items.Add("Tigris Paw");
            CharacterList.Items.Add("Rikku Akane");
        }

        /// <summary>
        /// Validates the user’s selection and closes the window with a positive result.
        /// Displays a warning message if no character is selected.
        /// </summary>
        private void Validate_Click(object sender, RoutedEventArgs e)
        {
            if (CharacterList.SelectedItem is string name)
            {
                SelectedCharacter = new Character { Name = name };
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
