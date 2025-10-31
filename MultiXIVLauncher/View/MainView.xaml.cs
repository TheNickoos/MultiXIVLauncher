using MultiXIVLauncher.Models;
using MultiXIVLauncher.Services;
using MultiXIVLauncher.View.Headers;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace MultiXIVLauncher.View
{
    public partial class MainView : UserControl
    {
        private MainHeader _header;
        private readonly string _allKey;

        public MainView()
        {
            InitializeComponent();
            _allKey = LanguageManager.T("Main_Header_AllCharacters");
            _header = new MainHeader();
            ((LauncherWindow)Application.Current.MainWindow).SetHeaderContent(_header);

            WireHeader();
            PopulateGroups();
            RefreshCharacterCardsForSelection(); // affiche selon la sélection courante
        }

        private void WireHeader()
        {
            if (_header == null) return;

            if (_header.BtnLaunchGroup != null)
                _header.BtnLaunchGroup.Click += async (s, e) => await LaunchSelectedGroupAsync();

            if (_header.CmbGroups != null)
                _header.CmbGroups.SelectionChanged += (s, e) => RefreshCharacterCardsForSelection();
        }

        private void PopulateGroups()
        {
            if (_header?.CmbGroups == null) return;

            _header.CmbGroups.Items.Clear();
            _header.CmbGroups.Items.Add(_allKey);

            var groups = ConfigManager.Current?.Groups;
            if (groups != null)
            {
                foreach (var g in groups)
                {
                    if (g != null && !string.IsNullOrWhiteSpace(g.Name))
                        _header.CmbGroups.Items.Add(g.Name);
                }
            }

            if (_header.CmbGroups.Items.Count > 0)
                _header.CmbGroups.SelectedIndex = 0;
        }

        // ------- LANCEMENT DE GROUPE (séquentiel strict) -------

        private async Task LaunchSelectedGroupAsync()
        {
            if (_header?.CmbGroups == null) return;

            var selectedName = _header.CmbGroups.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(selectedName)) return;

            var allChars = ConfigManager.Current?.Characters;
            if (allChars == null || allChars.Count == 0)
            {
                MessageBox.Show("Aucun personnage configuré.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _header.BtnLaunchGroup.IsEnabled = false;

            try
            {
                // Tous les personnages -> lance tout (séquentiel)
                if (string.Equals(selectedName, _allKey, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var ch in allChars)
                    {
                        await CharacterLauncher.LaunchCharacterAsync(
                            ch,
                            ConfigManager.Current,
                            Application.Current.MainWindow,
                            timeout: TimeSpan.FromSeconds(90),
                            cancellationToken: CancellationToken.None);
                    }
                    return;
                }

                // Trouver le groupe par nom
                var group = FindGroupByName(selectedName);
                if (group == null)
                {
                    MessageBox.Show($"Groupe introuvable : {selectedName}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Filtrer les personnages appartenant au groupe (via Character.GroupIds)
                var targetChars = GetCharactersByGroupId(group.Id, allChars);
                if (targetChars.Count == 0)
                {
                    MessageBox.Show($"Le groupe \"{selectedName}\" ne contient aucun personnage.", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Lancement séquentiel strict
                foreach (var ch in targetChars)
                {
                    await CharacterLauncher.LaunchCharacterAsync(
                        ch,
                        ConfigManager.Current,
                        Application.Current.MainWindow,
                        timeout: TimeSpan.FromSeconds(90),
                        cancellationToken: CancellationToken.None);
                }
            }
            finally
            {
                _header.BtnLaunchGroup.IsEnabled = true;
            }
        }

        // ------- AFFICHAGE FILTRÉ SELON LA COMBO -------

        private void RefreshCharacterCardsForSelection()
        {
            var allChars = ConfigManager.Current?.Characters ?? new List<Character>();

            if (_header?.CmbGroups == null)
            {
                LoadCharacterCards(allChars);
                return;
            }

            var selectedName = _header.CmbGroups.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(selectedName) || string.Equals(selectedName, _allKey, StringComparison.OrdinalIgnoreCase))
            {
                LoadCharacterCards(allChars);
                return;
            }

            var group = FindGroupByName(selectedName);
            if (group == null)
            {
                LoadCharacterCards(new List<Character>()); // groupe inconnu -> rien
                return;
            }

            var filtered = GetCharactersByGroupId(group.Id, allChars);
            LoadCharacterCards(filtered);
        }

        // ------- HELPERS GROUPES / PERSONNAGES (fortement typés) -------

        private dynamic FindGroupByName(string groupName)
        {
            var groups = ConfigManager.Current?.Groups;
            if (groups == null) return null;

            foreach (var g in groups)
            {
                if (g == null) continue;
                if (!string.IsNullOrWhiteSpace(g.Name) &&
                    string.Equals(g.Name, groupName, StringComparison.OrdinalIgnoreCase))
                {
                    return g; // on suppose g.Id (int) et g.Name (string)
                }
            }
            return null;
        }

        private static List<Character> GetCharactersByGroupId(int groupId, IList<Character> allChars)
        {
            var result = new List<Character>();
            if (allChars == null) return result;

            foreach (var c in allChars)
            {
                if (c == null || c.GroupIds == null) continue;
                if (c.GroupIds.Contains(groupId))
                    result.Add(c);
            }
            return result;
        }

        // ----------------- Rendu des cartes -----------------

        private void LoadCharacterCards(IList<Character> characters)
        {
            CharacterContainer.Children.Clear();

            if (characters == null || characters.Count == 0)
                return;

            foreach (var c in characters)
            {
                var card = CreateCharacterCard(c);
                CharacterContainer.Children.Add(card);
                AnimateCardAppearance(card);
            }
        }

        private Border CreateCharacterCard(Character character)
        {
            string portraitPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "cache", character.Id.ToString(), "portrait.jpg");

            ImageSource portraitSrc = LoadImageSafe(portraitPath)
                ?? LoadBitmapFromResources(Properties.Resources.portrait_default);

            var card = new Border
            {
                Style = (Style)FindResource("CardBorder"),
                Margin = new Thickness(0, 0, 0, 8),
                Opacity = 0,
                RenderTransform = new ScaleTransform(1, 1),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // --- Portrait ---
            var img = new Image
            {
                Source = portraitSrc,
                Width = 72,
                Height = 72,
                Stretch = Stretch.UniformToFill,
                Margin = new Thickness(0, 0, 12, 0),
                Clip = new RectangleGeometry(new Rect(0, 0, 72, 72), 8, 8)
            };
            grid.Children.Add(img);

            // --- Infos ---
            var info = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            info.Children.Add(new TextBlock
            {
                Text = character.Name,
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("ClrTextPrimary")
            });

            string server = string.IsNullOrWhiteSpace(character.Server) ? "Unknown server" : character.Server;
            string job = string.IsNullOrWhiteSpace(character.Class) ? "Unknown class" : character.Class;
            string level = character.Level > 0 ? character.Level.ToString() : "?";

            info.Children.Add(new TextBlock
            {
                Text = $"{server} - {job} {level}",
                Style = (Style)FindResource("BodyText"),
                Opacity = 0.8
            });
            Grid.SetColumn(info, 1);
            grid.Children.Add(info);

            // --- Bouton Run ---
            var runBtn = new Button
            {
                Content = LanguageManager.T("Main_View_Button_Run"),
                Width = 120,
                Height = 36,
                Style = (Style)FindResource("RoundedButton"),
                VerticalAlignment = VerticalAlignment.Center
            };
            runBtn.Click += (s, e) => LaunchCharacter(character);
            Grid.SetColumn(runBtn, 2);
            grid.Children.Add(runBtn);

            card.Child = grid;

            // === Animation de SURVOL ===
            card.MouseEnter += (s, e) =>
            {
                var hoverAnim = new DoubleAnimation(1.0, 1.03, TimeSpan.FromMilliseconds(150))
                {
                    EasingFunction = new QuadraticEase()
                };
                var shadow = new DropShadowEffect
                {
                    Color = Color.FromRgb(185, 147, 255),
                    BlurRadius = 25,
                    ShadowDepth = 0,
                    Opacity = 0.9
                };
                card.Effect = shadow;
                (card.RenderTransform as ScaleTransform)?.BeginAnimation(ScaleTransform.ScaleXProperty, hoverAnim);
                (card.RenderTransform as ScaleTransform)?.BeginAnimation(ScaleTransform.ScaleYProperty, hoverAnim);
            };

            card.MouseLeave += (s, e) =>
            {
                var leaveAnim = new DoubleAnimation(1.03, 1.0, TimeSpan.FromMilliseconds(150))
                {
                    EasingFunction = new QuadraticEase()
                };
                card.Effect = null;
                (card.RenderTransform as ScaleTransform)?.BeginAnimation(ScaleTransform.ScaleXProperty, leaveAnim);
                (card.RenderTransform as ScaleTransform)?.BeginAnimation(ScaleTransform.ScaleYProperty, leaveAnim);
            };

            return card;
        }

        private static ImageSource LoadBitmapFromResources(System.Drawing.Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = memory;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
        }

        private static ImageSource? LoadImageSafe(string path)
        {
            if (!File.Exists(path))
                return null;

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(path, UriKind.Absolute);
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch { return null; }
        }

        private static ImageSource LoadPackImage(string packUri)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(packUri, UriKind.Absolute);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }

        private void AnimateCardAppearance(Border card)
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase()
            };
            var slideUp = new DoubleAnimation(30, 0, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase()
            };

            card.BeginAnimation(OpacityProperty, fadeIn);
            // Note: si tu veux une vraie translation, mets un TranslateTransform séparé.
            card.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);
        }

        private async void LaunchCharacter(Character character)
        {
            await CharacterLauncher.LaunchCharacterAsync(
                character,
                ConfigManager.Current,
                Application.Current.MainWindow,
                timeout: TimeSpan.FromSeconds(90),
                cancellationToken: CancellationToken.None);
        }
    }
}
