using MultiXIVLauncher.Models;
using MultiXIVLauncher.Services;
using MultiXIVLauncher.Utils;
using MultiXIVLauncher.View.Headers;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            UIHelper.SetHeaderButtonsVisible(false);

            CurrentCharacter = tempCharacter;
            ParentView = parentView;

            // Populate UI from temp character
            TxtCharacterName.Text = CurrentCharacter.Name ?? string.Empty;
            TxtLodestoneId.Text = CurrentCharacter.LodestoneId.ToString();

            // Fill preset list and select the correct one
            PopulatePresets();
            SelectCurrentPreset();
        }

        /// <summary>
        /// Populates the ComboBox with all available presets.
        /// </summary>
        private void PopulatePresets()
        {
            CmbPreset.Items.Clear();
            CmbPreset.Items.Add(new ComboBoxItem { Content = "Aucun", Tag = 0 });

            if (ConfigManager.Current?.Presets != null)
            {
                foreach (var p in ConfigManager.Current.Presets)
                    CmbPreset.Items.Add(new ComboBoxItem { Content = p.Name, Tag = p.Id });
            }

            CmbPreset.SelectionChanged -= CmbPreset_SelectionChanged;
            CmbPreset.SelectionChanged += CmbPreset_SelectionChanged;
        }

        /// <summary>
        /// Selects the current preset in the ComboBox based on the character's PresetId.
        /// </summary>
        private void SelectCurrentPreset()
        {
            int pid = CurrentCharacter.PresetId;
            foreach (ComboBoxItem item in CmbPreset.Items)
            {
                if ((int)item.Tag == pid)
                {
                    CmbPreset.SelectedItem = item;
                    return;
                }
            }
            CmbPreset.SelectedIndex = 0; // default: none
        }

        /// <summary>
        /// Updates the character's PresetId when the user selects a preset.
        /// </summary>
        private void CmbPreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentCharacter == null)
                return;

            if (CmbPreset.SelectedItem is ComboBoxItem sel && sel.Tag is int id)
                CurrentCharacter.PresetId = id;
        }

        /// <summary>
        /// Updates the temp character name on each change.
        /// </summary>
        private void TxtCharacterName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentCharacter == null)
                return;

            CurrentCharacter.Name = TxtCharacterName.Text?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Updates the temp character LodestoneId when field loses focus.
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
        /// Opens the folder of the current character in Explorer.
        /// </summary>
        private void OpenCharacterFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Base path = application root
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                // Character folder: Characters\<CharacterId>
                string characterDir = System.IO.Path.Combine(baseDir, "Characters", CurrentCharacter.Id.ToString());

                // Create it if it doesn't exist yet (optional but handy)
                if (!System.IO.Directory.Exists(characterDir))
                    System.IO.Directory.CreateDirectory(characterDir);

                // Open it in Explorer
                Process.Start("explorer.exe", characterDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open the character directory.\n\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// Returns to CharactersView without saving to disk.
        /// </summary>
        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            ParentView.RefreshCharacterList();
            UIHelper.SetHeaderButtonsVisible(true);

            ((LauncherWindow)Application.Current.MainWindow).SetPage(ParentView);
        }
    }
}
