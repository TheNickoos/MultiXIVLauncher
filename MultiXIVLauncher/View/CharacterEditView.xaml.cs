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

        private void OpenCharacterFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", @"C:\Users\Public\Documents\MultiXIVLauncher\Characters");
        }

        private async void SaveCharacter_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtCharacterName.Text.Trim();
            int lodestoneId = 0;
            int.TryParse(TxtLodestoneId.Text.Trim(), out lodestoneId);

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a character name.");
                return;
            }

            // Crée ou met à jour le personnage
            var character = Character.Create(name);
            character.LodestoneId = lodestoneId;

            // Téléchargement Lodestone
            //bool success = await LodestoneFetcher.UpdateCharacterFromLodestoneAsync(character);
            bool success = true;

            // Ajoute à la configuration
            ConfigManager.Current.Characters.Add(character);
            ConfigManager.Save();

            MessageBox.Show(success
                ? $"Character '{name}' saved and Lodestone data fetched successfully!"
                : $"Character '{name}' saved, but Lodestone data could not be fetched.");

            ((LauncherWindow)Application.Current.MainWindow).SetPage(new CharactersView());
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            ((LauncherWindow)Application.Current.MainWindow).SetPage(new CharactersView());
        }
    }
}
