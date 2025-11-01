using MultiXIVLauncher.Models;
using MultiXIVLauncher.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace MultiXIVLauncher.View
{
    /// <summary>
    /// First-run onboarding to create a base Preset, a Character, then a Group.
    /// </summary>
    public partial class FirstRunWindow : Window, INotifyPropertyChanged
    {
        // INotifyPropertyChanged plumbing
        public event PropertyChangedEventHandler PropertyChanged;
        private void Raise(string name) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }

        // Bindings (texts could be wired to LanguageManager if tu préfères)
        public string TitleText => "Bienvenue • MultiXIVLauncher";
        public string WelcomeTitle => "Bienvenue !";
        public string WelcomeSubtitle => "Préparons votre premier preset, votre premier personnage et votre premier groupe.";
        public string Step1Title => "Étape 1 – Créer un preset";
        public string Step1Desc => "Vous pouvez télécharger un preset prêt à l’emploi (même source que la fenêtre de téléchargement des presets) ou créer un preset vide.";
        public string UseDownloadPresetText => "Télécharger un preset prêt";
        public string CreateEmptyPresetText => "Créer un preset vide";
        public string EmptyPresetNameLabel => "Nom du preset :";
        public string Step2Title => "Étape 2 – Créer un personnage";
        public string Step2Desc => "Saisissez le nom, le Lodestone ID et choisissez le preset utilisé par ce personnage.";
        public string CharacterNameLabel => "Nom du personnage :";
        public string LodestoneIdLabel => "Lodestone ID (FFXIV) :";
        public string CharacterPresetLabel => "Preset :";
        public string LaunchCharacterText => "Lancer le personnage";
        public string Step3Title => "Étape 3 – Créer un groupe";
        public string Step3Desc => "Donnez un nom à votre groupe et assignez-y votre personnage.";
        public string GroupNameLabel => "Nom du groupe :";
        public string GroupAssignLabel => "Personnage dans le groupe :";
        public string CancelText => "Annuler";
        public string BackText => "Retour";
        public string NextText => "Suivant";
        public string FinishText => "Terminer";
        public string CongratsTitle => "Tout est prêt !";
        public string CongratsDesc => "Votre preset, votre personnage et votre groupe ont été créés. Vous allez être redirigé vers l’accueil.";
        public string GoHomeText => "Aller à l’accueil";

        public string StepIndicator
        {
            get { return _stepIndicator; }
            set { _stepIndicator = value; Raise(nameof(StepIndicator)); }
        }
        private string _stepIndicator = "Étape 1/3";

        // Step state
        public bool UseDownloadPreset
        {
            get { return _useDownloadPreset; }
            set { _useDownloadPreset = value; if (value) UseEmptyPreset = false; Raise(nameof(UseDownloadPreset)); }
        }
        private bool _useDownloadPreset = true;

        public bool UseEmptyPreset
        {
            get { return _useEmptyPreset; }
            set { _useEmptyPreset = value; if (value) UseDownloadPreset = false; Raise(nameof(UseEmptyPreset)); }
        }
        private bool _useEmptyPreset = false;

        public string NewPresetName
        {
            get { return _newPresetName; }
            set { _newPresetName = value; Raise(nameof(NewPresetName)); }
        }
        private string _newPresetName = "Mon premier preset";

        public IList<string> PresetChoices
        {
            get { return _presetChoices; }
            set { _presetChoices = value; Raise(nameof(PresetChoices)); }
        }
        private IList<string> _presetChoices = new List<string>();

        public string SelectedPresetName
        {
            get { return _selectedPresetName; }
            set { _selectedPresetName = value; Raise(nameof(SelectedPresetName)); }
        }
        private string _selectedPresetName;

        public string CharacterName
        {
            get { return _characterName; }
            set { _characterName = value; Raise(nameof(CharacterName)); }
        }
        private string _characterName = "";

        public string LodestoneIdText
        {
            get { return _lodestoneIdText; }
            set { _lodestoneIdText = value; Raise(nameof(LodestoneIdText)); }
        }
        private string _lodestoneIdText = "";

        public IList<string> CharacterChoices
        {
            get { return _characterChoices; }
            set { _characterChoices = value; Raise(nameof(CharacterChoices)); }
        }
        private IList<string> _characterChoices = new List<string>();

        public string SelectedCharacterName
        {
            get { return _selectedCharacterName; }
            set { _selectedCharacterName = value; Raise(nameof(SelectedCharacterName)); }
        }
        private string _selectedCharacterName;

        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = value; Raise(nameof(GroupName)); }
        }
        private string _groupName = "Mon premier groupe";

        // Track created items for wiring
        private Preset _createdPreset;
        private Character _createdCharacter;

        public FirstRunWindow()
        {
            InitializeComponent();
            DataContext = this;
            RefreshPresetChoices();
            StepIndicator = "Étape 1/3";
        }

        // ----- UI Nav -----

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void NextFromPreset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UseDownloadPreset)
                {
                    // Ouvre la fenêtre de téléchargement existante (même JSON que PresetDownloadWindow)
                    // Le flux d’ajout réel dépend de ta fenêtre; ici on suppose qu’elle ajoute dans ConfigManager.Current.Presets.
                    var winType = Type.GetType("MultiXIVLauncher.View.PresetDownloadWindow");
                    if (winType != null)
                    {
                        var dlg = (Window)Activator.CreateInstance(winType);
                        dlg.Owner = this;
                        dlg.ShowDialog();
                    }

                    if (ConfigManager.Current.Presets == null || ConfigManager.Current.Presets.Count == 0)
                    {
                        MessageBox.Show(this, "Aucun preset n’a été téléchargé. Vous pouvez également créer un preset vide.",
                            "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    _createdPreset = ConfigManager.Current.Presets.Last();
                }
                else
                {
                    var name = (NewPresetName ?? "").Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        MessageBox.Show(this, "Veuillez entrer un nom de preset.", "Champ requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _createdPreset = new Preset
                    {
                        Id = NextPresetId(),
                        Name = name,
                    };

                    EnsureLists();
                    ConfigManager.Current.Presets.Add(_createdPreset);
                    ConfigManager.Save();
                }

                RefreshPresetChoices();
                SelectedPresetName = _createdPreset.Name;

                // Step switch
                StepPreset.Visibility = Visibility.Collapsed;
                StepCharacter.Visibility = Visibility.Visible;
                StepIndicator = "Étape 2/3";
            }
            catch (Exception ex)
            {
                Logger.Error("Preset step failed: " + ex.Message);
                MessageBox.Show(this, "Erreur lors de la création/téléchargement du preset.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToPreset_Click(object sender, RoutedEventArgs e)
        {
            StepCharacter.Visibility = Visibility.Collapsed;
            StepPreset.Visibility = Visibility.Visible;
            StepIndicator = "Étape 1/3";
        }

        private void NextFromCharacter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_createdPreset == null)
                {
                    MessageBox.Show(this, "Veuillez d’abord créer ou sélectionner un preset.", "Manquant", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var name = (CharacterName ?? "").Trim();
                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show(this, "Veuillez entrer un nom de personnage.", "Champ requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int lodestoneId = 0;
                if (!string.IsNullOrEmpty(LodestoneIdText) && !int.TryParse(LodestoneIdText, out lodestoneId))
                {
                    MessageBox.Show(this, "Le Lodestone ID doit être un nombre entier.", "Format invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var preset = ConfigManager.Current.Presets.FirstOrDefault(p => p.Name == SelectedPresetName);
                if (preset == null)
                {
                    MessageBox.Show(this, "Veuillez choisir un preset.", "Manquant", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _createdCharacter = new Character
                {
                    Id = NextCharacterId(),
                    Name = name,
                    LodestoneId = lodestoneId,
                    PresetId = preset.Id
                };

                EnsureLists();
                ConfigManager.Current.Characters.Add(_createdCharacter);
                ConfigManager.Save();

                // Si preset créé vide, on invitera à copier la base après "Terminé".
                // Step switch
                RefreshCharacterChoices();
                SelectedCharacterName = _createdCharacter.Name;

                StepCharacter.Visibility = Visibility.Collapsed;
                StepGroup.Visibility = Visibility.Visible;
                StepIndicator = "Étape 3/3";
            }
            catch (Exception ex)
            {
                Logger.Error("Character step failed: " + ex.Message);
                MessageBox.Show(this, "Erreur lors de la création du personnage.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToCharacter_Click(object sender, RoutedEventArgs e)
        {
            StepGroup.Visibility = Visibility.Collapsed;
            StepCharacter.Visibility = Visibility.Visible;
            StepIndicator = "Étape 2/3";
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_createdCharacter == null || _createdPreset == null)
                {
                    MessageBox.Show(this, "Veuillez d’abord créer le preset et le personnage.", "Manquant", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Créer le groupe
                var gname = (GroupName ?? "").Trim();
                if (string.IsNullOrEmpty(gname))
                {
                    MessageBox.Show(this, "Veuillez entrer un nom de groupe.", "Champ requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var group = new Group
                {
                    Id = NextGroupId(),
                    Name = gname,
                };

                EnsureLists();
                ConfigManager.Current.Groups.Add(group);

                // Si l’utilisateur a choisi un preset VIDE (UseEmptyPreset), on copie la config du personnage vers le preset
                if (UseEmptyPreset)
                {
                    TryCopyCharacterToPreset(_createdCharacter, _createdPreset);
                }

                ConfigManager.Save();

                // Step switch -> Done
                StepGroup.Visibility = Visibility.Collapsed;
                StepDone.Visibility = Visibility.Visible;
                StepIndicator = "Terminé";
            }
            catch (Exception ex)
            {
                Logger.Error("Group step failed: " + ex.Message);
                MessageBox.Show(this, "Erreur lors de la création du groupe.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LaunchCharacter_Click(object sender, RoutedEventArgs e)
        {
            if (_createdCharacter == null)
            {
                MessageBox.Show(this, "Créez d’abord le personnage.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Crochet volontairement minimal : à brancher sur ta logique existante de lancement
                // (CharactersView, service, ou appel CLI). On log pour signaler le point d’entrée.
                Logger.Info("[Onboarding] Lancement demandé pour le personnage Id=" + _createdCharacter.Id + " / Name=" + _createdCharacter.Name);

                MessageBox.Show(this,
                    "Le lancement du personnage est déclenché.\nConfigurez vos plugins, listes tiers, etc., puis revenez et cliquez sur « Terminer » pour copier la base dans le preset.",
                    "Premier lancement", MessageBoxButton.OK, MessageBoxImage.Information);

                // TODO: branche ici l’appel réel (exemples possibles selon ton codebase) :
                // LauncherService.LaunchCharacter(_createdCharacter);
                // CharactersView.RunCharacter(_createdCharacter);
                // Process.Start(... XIVLauncher.exe ..., args);
            }
            catch (Exception ex)
            {
                Logger.Error("LaunchCharacter failed: " + ex.Message);
                MessageBox.Show(this, "Impossible de lancer le personnage depuis l’assistant.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ----- Helpers -----

        private void RefreshPresetChoices()
        {
            EnsureLists();
            PresetChoices = ConfigManager.Current.Presets.Select(p => p.Name).ToList();
            if (PresetChoices.Count > 0 && string.IsNullOrEmpty(SelectedPresetName))
                SelectedPresetName = PresetChoices[0];
        }

        private void RefreshCharacterChoices()
        {
            EnsureLists();
            CharacterChoices = ConfigManager.Current.Characters.Select(c => c.Name).ToList();
            if (CharacterChoices.Count > 0 && string.IsNullOrEmpty(SelectedCharacterName))
                SelectedCharacterName = CharacterChoices[0];
        }

        private void EnsureLists()
        {
            var cfg = ConfigManager.Current;
            if (cfg.Presets == null) cfg.Presets = new List<Preset>();
            if (cfg.Characters == null) cfg.Characters = new List<Character>();
            if (cfg.Groups == null) cfg.Groups = new List<Group>();
        }

        private int NextPresetId()
        {
            EnsureLists();
            return ConfigManager.Current.Presets.Count == 0 ? 1 : ConfigManager.Current.Presets.Max(p => p.Id) + 1;
        }

        private int NextCharacterId()
        {
            EnsureLists();
            return ConfigManager.Current.Characters.Count == 0 ? 1 : ConfigManager.Current.Characters.Max(c => c.Id) + 1;
        }

        private int NextGroupId()
        {
            EnsureLists();
            return ConfigManager.Current.Groups.Count == 0 ? 1 : ConfigManager.Current.Groups.Max(g => g.Id) + 1;
        }

        /// <summary>
        /// Copie les fichiers de configuration du personnage vers le preset,
        /// selon la structure convenue : Characters\{CharacterId}\  ->  Presets\{PresetId}\
        /// Racine = dossier de l’application.
        /// </summary>
        private void TryCopyCharacterToPreset(Character character, Preset preset)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string src = Path.Combine(baseDir, "Characters", character.Id.ToString());
                string dst = Path.Combine(baseDir, "Presets", preset.Id.ToString());

                if (!Directory.Exists(src))
                {
                    Logger.Warn("[Onboarding] Répertoire personnage introuvable : " + src);
                    return;
                }

                if (Directory.Exists(dst))
                    Directory.Delete(dst, true);

                CopyDirectoryRecursive(src, dst);
                Logger.Info("[Onboarding] Copie personnage -> preset effectuée : " + src + " -> " + dst);
            }
            catch (Exception ex)
            {
                Logger.Error("Copy character to preset failed: " + ex.Message);
                MessageBox.Show(this,
                    "La copie de la configuration du personnage vers le preset a échoué. Vous pourrez la refaire depuis l’application.",
                    "Copie échouée", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CopyDirectoryRecursive(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var name = System.IO.Path.GetFileName(file);
                var dest = System.IO.Path.Combine(destDir, name);
                File.Copy(file, dest, true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var name = System.IO.Path.GetFileName(dir);
                var dest = System.IO.Path.Combine(destDir, name);
                CopyDirectoryRecursive(dir, dest);
            }
        }
    }
}
