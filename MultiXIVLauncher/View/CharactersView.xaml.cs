using MultiXIVLauncher.Models;
using MultiXIVLauncher.Services;
using MultiXIVLauncher.Utils;
using MultiXIVLauncher.Utils.Interfaces;
using MultiXIVLauncher.View.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MultiXIVLauncher.View.Components;
using System.Threading.Tasks;

namespace MultiXIVLauncher.View
{
    /// <summary>
    /// Displays and manages the list of characters.
    /// All edits happen in temporary memory and are only persisted when the header Save is clicked.
    /// </summary>
    public partial class CharactersView : UserControl, ISavableView
    {
        private List<Character>? TemporaryCharacters { get; set; }
        private bool isAddingCharacter = false;

        public CharactersView()
        {
            InitializeComponent();
            ((LauncherWindow)Application.Current.MainWindow).SetHeaderContent(new SettingsHeader());
            LoadTemporaryData();
            RefreshCharacterList();
        }

        private void LoadTemporaryData()
        {
            TemporaryCharacters = ConfigManager.Current.Characters
                .Select(c => new Character
                {
                    Id = c.Id,
                    Name = c.Name,
                    LodestoneId = c.LodestoneId,
                    PresetId = c.PresetId,
                    GroupIds = new List<int>(c.GroupIds),
                    Class = c.Class,
                    Server = c.Server,
                    Level = c.Level
                })
                .ToList();
        }

        public void RefreshCharacterList()
        {
            CharacterListPanel.Children.Clear();
            if (TemporaryCharacters == null) return;
            foreach (var ch in TemporaryCharacters)
                AddCharacterCard(ch, animate: false);
        }

        private void BtnAddCharacter_Click(object sender, RoutedEventArgs e)
        {
            if (!isAddingCharacter)
            {
                isAddingCharacter = true;
                BtnAddCharacter.Visibility = Visibility.Collapsed;
                ShowElement(TxtCharacterName);
                ShowElement(BtnValidate);
            }
        }

        private void BtnValidate_Click(object sender, RoutedEventArgs e)
        {
            if (!isAddingCharacter) return;

            string name = TxtCharacterName.Text.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                var newCharacter = new Character
                {
                    Id = GenerateNextCharacterId(),
                    Name = name,
                    LodestoneId = 0,
                    PresetId = 0,
                    GroupIds = new List<int>(),
                    Class = null,
                    Server = null,
                    Level = 0
                };

                TemporaryCharacters!.Add(newCharacter);
                AddCharacterCard(newCharacter);
            }

            isAddingCharacter = false;
            HideElement(TxtCharacterName);
            HideElement(BtnValidate);
            TxtCharacterName.Text = string.Empty;
            BtnAddCharacter.Visibility = Visibility.Visible;
        }

        private void AddCharacterCard(Character character, bool animate = true)
        {
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(1.0, 1.0));
            transformGroup.Children.Add(new TranslateTransform(0, animate ? 30 : 0));

            Border card = new Border
            {
                Style = (Style)FindResource("CardBorder"),
                Opacity = animate ? 0 : 1,
                RenderTransform = transformGroup,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Tag = character.Id
            };

            Grid content = new Grid();
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel info = new StackPanel { Orientation = Orientation.Vertical };
            info.Children.Add(new TextBlock
            {
                Text = character.Name,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("ClrTextPrimary")
            });
            info.Children.Add(new TextBlock
            {
                Text = $"{(string.IsNullOrWhiteSpace(character.Server) ? "Unknown server" : character.Server)} — {(string.IsNullOrWhiteSpace(character.Class) ? "Unknown class" : character.Class)}",
                Style = (Style)FindResource("BodyText"),
                Opacity = 0.8
            });

            StackPanel actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var editButton = new Button
            {
                Content = "Edit",
                Height = 30,
                Margin = new Thickness(0, 0, 8, 0),
                Style = (Style)FindResource("OutlineButton")
            };
            editButton.Click += (s, e) =>
            {
                var id = (int)card.Tag;
                var tempChar = TemporaryCharacters!.First(c => c.Id == id);
                var editView = new CharacterEditView(tempChar, this);
                ((LauncherWindow)Application.Current.MainWindow).SetPage(editView);
            };
            actions.Children.Add(editButton);

            var deleteButton = new Button
            {
                Content = "Delete",
                Height = 30,
                Style = (Style)FindResource("OutlineButton")
            };
            deleteButton.Click += (s, e) =>
            {
                var id = (int)card.Tag;
                var tempChar = TemporaryCharacters!.FirstOrDefault(c => c.Id == id);
                if (tempChar != null)
                {
                    TemporaryCharacters!.Remove(tempChar);
                    try
                    {
                        string cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache", id.ToString());
                        if (Directory.Exists(cacheDir))
                        {
                            Directory.Delete(cacheDir, true);
                            Logger.Info($"Deleted cache for character '{tempChar.Name}' (ID: {id})");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"Failed to delete cache for character '{tempChar.Name}': {ex.Message}");
                    }
                }

                RemoveCharacterCard(card);
            };
            actions.Children.Add(deleteButton);

            content.Children.Add(info);
            content.Children.Add(actions);
            Grid.SetColumn(actions, 1);
            card.Child = content;
            CharacterListPanel.Children.Add(card);

            var hoverShadow = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Color.FromRgb(185, 147, 255),
                BlurRadius = 25,
                ShadowDepth = 0,
                Opacity = 0.9
            };

            card.MouseEnter += (s, e) =>
            {
                var hoverAnim = new DoubleAnimation(1.0, 1.03, TimeSpan.FromMilliseconds(150))
                {
                    EasingFunction = new QuadraticEase()
                };
                card.Effect = hoverShadow;
                ((ScaleTransform)transformGroup.Children[0]).BeginAnimation(ScaleTransform.ScaleXProperty, hoverAnim);
                ((ScaleTransform)transformGroup.Children[0]).BeginAnimation(ScaleTransform.ScaleYProperty, hoverAnim);
            };

            card.MouseLeave += (s, e) =>
            {
                var leaveAnim = new DoubleAnimation(1.03, 1.0, TimeSpan.FromMilliseconds(150))
                {
                    EasingFunction = new QuadraticEase()
                };
                card.Effect = null;
                ((ScaleTransform)transformGroup.Children[0]).BeginAnimation(ScaleTransform.ScaleXProperty, leaveAnim);
                ((ScaleTransform)transformGroup.Children[0]).BeginAnimation(ScaleTransform.ScaleYProperty, leaveAnim);
            };

            if (animate)
                AnimateCardAppearance(card);
        }

        private int GenerateNextCharacterId()
        {
            if (TemporaryCharacters == null || TemporaryCharacters.Count == 0)
                return 1;
            return TemporaryCharacters.Max(c => c.Id) + 1;
        }

        public async void Save()
        {
            var mainWindow = (LauncherWindow)Application.Current.MainWindow;
            var overlay = new LoadingOverlay();
            Panel.SetZIndex(overlay, 9999);
            mainWindow.OverlayContainer.Children.Add(overlay);
            overlay.UpdateStatus("Saving characters...");

            try
            {
                if (ConfigManager.Current == null)
                {
                    Logger.Error("Cannot save characters: ConfigManager.Current is null.");
                    mainWindow.OverlayContainer.Children.Remove(overlay);
                    return;
                }

                ConfigManager.Current.Characters.Clear();
                ConfigManager.Current.Characters.AddRange(TemporaryCharacters);

                int total = ConfigManager.Current.Characters.Count;
                int index = 0;

                overlay.Progress.IsIndeterminate = false;
                overlay.Progress.Maximum = total;

                // ✅ Correction : base sur le dossier racine de l’application
                string charactersRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Characters");
                Directory.CreateDirectory(charactersRoot);

                foreach (var character in ConfigManager.Current.Characters)
                {
                    index++;
                    overlay.UpdateStatus($"Updating Lodestone ({index}/{total}) for {character.Name}...");
                    overlay.Progress.Value = index - 1;

                    try
                    {
                        bool success = await LodestoneFetcher.UpdateCharacterFromLodestoneAsync(character);
                        if (success)
                            Logger.Info($"Lodestone data successfully updated for '{character.Name}'.");
                        else
                            Logger.Warn($"Lodestone data update failed for '{character.Name}'.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error updating Lodestone for '{character.Name}': {ex.Message}");
                    }

                    try
                    {
                        if (character.PresetId > 0)
                        {
                            var preset = ConfigManager.Current.Presets.FirstOrDefault(p => p.Id == character.PresetId);
                            if (preset != null)
                            {
                                string presetDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets", preset.Name);
                                if (Directory.Exists(presetDir))
                                {
                                    string characterDir = Path.Combine(charactersRoot, character.Id.ToString());
                                    Directory.CreateDirectory(characterDir);
                                    overlay.UpdateStatus($"Applying preset '{preset.Name}' to {character.Name}...");
                                    await CopyDirectoryAsync(presetDir, characterDir, overwrite: true);
                                }
                                else
                                {
                                    Logger.Warn($"Preset directory not found for '{preset.Name}' at '{presetDir}'.");
                                }
                            }
                            else
                            {
                                Logger.Warn($"No preset found for ID {character.PresetId} (Character '{character.Name}').");
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to copy preset (Id={character.PresetId}) for '{character.Name}': {ex.Message}");
                    }
                }

                overlay.UpdateStatus("Saving configuration...");
                ConfigManager.Save();
                overlay.UpdateStatus("Done!");
                await Task.Delay(600);
            }
            finally
            {
                mainWindow.OverlayContainer.Children.Remove(overlay);
            }
        }

        private static Task CopyDirectoryAsync(string sourceDir, string destDir, bool overwrite)
        {
            return Task.Run(() =>
            {
                foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(Path.Combine(destDir, Path.GetRelativePath(sourceDir, dir)));

                foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
                {
                    var relative = Path.GetRelativePath(sourceDir, file);
                    var destFile = Path.Combine(destDir, relative);
                    Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                    File.Copy(file, destFile, overwrite);
                }
            });
        }

        private void AnimateCardAppearance(Border card)
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
            var slideUp = new DoubleAnimation(30, 0, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
            card.BeginAnimation(OpacityProperty, fadeIn);
            card.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);
        }

        private void ShowElement(UIElement element)
        {
            element.Visibility = Visibility.Visible;
            element.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150)));
        }

        private void HideElement(UIElement element)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
            fadeOut.Completed += (s, _) => element.Visibility = Visibility.Collapsed;
            element.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void RemoveCharacterCard(Border card)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
            var slideDown = new DoubleAnimation(0, 30, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
            card.BeginAnimation(OpacityProperty, fadeOut);
            card.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideDown);
            fadeOut.Completed += (s, _) => CharacterListPanel.Children.Remove(card);
        }
    }
}
