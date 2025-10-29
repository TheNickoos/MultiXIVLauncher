using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using MultiXIVLauncher.Models;

namespace MultiXIVLauncher.View
{
    /// <summary>
    /// Edits a temporary Character instance (from CharactersView).
    /// Changes are kept in memory and only persisted when the header Save is used.
    /// </summary>
    public partial class CharacterEditView : UserControl
    {
        private readonly Character CurrentCharacter;
        private readonly CharactersView ParentView;

        /// <summary>
        /// Construct with the temporary character and the parent CharactersView.
        /// </summary>
        public CharacterEditView(Character tempCharacter, CharactersView parentView)
        {
            InitializeComponent();

            CurrentCharacter = tempCharacter;
            ParentView = parentView;

            // Populate UI from temp character
            TxtCharacterName.Text = CurrentCharacter.Name ?? string.Empty;
            TxtLodestoneId.Text = CurrentCharacter.LodestoneId.ToString();
        }

        /// <summary>
        /// Update temp character name on each change (like GroupEditView does).
        /// </summary>
        private void TxtCharacterName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentCharacter == null)
                return;

            CurrentCharacter.Name = TxtCharacterName.Text?.Trim() ?? string.Empty;
        }


        /// <summary>
        /// Update temp character LodestoneId when the field loses focus (avoids spam parsing).
        /// </summary>
        private void TxtLodestoneId_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CurrentCharacter == null)
                return;

            if (int.TryParse(TxtLodestoneId.Text?.Trim(), out int lodestoneId))
                CurrentCharacter.LodestoneId = lodestoneId;
            else
                CurrentCharacter.LodestoneId = 0;
        }


        /// <summary>
        /// Open the characters folder.
        /// </summary>
        private void OpenCharacterFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", @"C:\Users\Public\Documents\MultiXIVLauncher\Characters");
            }
            catch
            {
                MessageBox.Show("Unable to open character directory.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Back to the list without saving to disk (like Groups).
        /// The temporary list already contains the changes; we just refresh the UI.
        /// </summary>
        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            ParentView.RefreshCharacterList();
            ((LauncherWindow)Application.Current.MainWindow).SetPage(ParentView);
        }
    }
}
