using System.Windows;

namespace MultiXIVLauncher.Views
{
    public partial class AddMemberWindow : Window
    {
        public Character SelectedCharacter { get; private set; }

        public AddMemberWindow()
        {
            InitializeComponent();

            CharacterList.Items.Add("Lyna Jade");
            CharacterList.Items.Add("Miles Brenner");
            CharacterList.Items.Add("Tigris Paw");
            CharacterList.Items.Add("Rikku Akane");
        }

        private void Validate_Click(object sender, RoutedEventArgs e)
        {
            if (CharacterList.SelectedItem is string name)
            {
                SelectedCharacter = new Character { Name = name };
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un personnage avant de valider.", "Attention",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
