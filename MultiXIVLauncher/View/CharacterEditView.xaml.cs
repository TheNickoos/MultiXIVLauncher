using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MultiXIVLauncher.Models;
using MultiXIVLauncher.Services;
using MultiXIVLauncher.Utils;

namespace MultiXIVLauncher.Views
{
    public partial class CharacterEditView : UserControl
    {
        public CharacterEditView()
        {
            InitializeComponent();
        }

        private Character? editingCharacter;

        public void LoadCharacter(Character character)
        {
            editingCharacter = character;
            TxtCharacterName.Text = character.Name;
            TxtLodestoneId.Text = character.LodestoneId.ToString();
        }

        private void OpenCharacterFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", @"C:\Users\Public\Documents\MultiXIVLauncher\Characters");
        }

        private async void SaveCharacter_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtCharacterName.Text.Trim();
            int.TryParse(TxtLodestoneId.Text.Trim(), out int lodestoneId);

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a character name.");
                return;
            }

            Character character;

            // 🔹 Si on édite un personnage existant
            if (editingCharacter != null)
            {
                character = editingCharacter;
                character.Name = name;
                character.LodestoneId = lodestoneId;
            }
            else
            {
                // 🔹 Sinon, création d'un nouveau personnage
                character = Character.Create(name);
                character.LodestoneId = lodestoneId;
                ConfigManager.Current.Characters.Add(character);
            }

            // --- Téléchargement Lodestone ---
            bool success = await LodestoneFetcher.UpdateCharacterFromLodestoneAsync(character);

            // --- Sauvegarde config ---
            ConfigManager.Save();

            ((LauncherWindow)Application.Current.MainWindow).SetPage(new CharactersView());
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            ((LauncherWindow)Application.Current.MainWindow).SetPage(new CharactersView());
        }
    }
}
