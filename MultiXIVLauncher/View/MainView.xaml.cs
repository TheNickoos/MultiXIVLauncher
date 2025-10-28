using MultiXIVLauncher.Models;
using MultiXIVLauncher.Services;
using MultiXIVLauncher.Views.Headers;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace MultiXIVLauncher.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            ((LauncherWindow)Application.Current.MainWindow).SetHeaderContent(new MainHeader());
            LoadCharacterCards();
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




        private void LoadCharacterCards()
        {
            CharacterContainer.Children.Clear();

            if (ConfigManager.Current?.Characters == null)
                return;

            foreach (var c in ConfigManager.Current.Characters)
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
            card.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);
        }

        private void AddCharacter_Click(object sender, RoutedEventArgs e)
        {
            ((LauncherWindow)Application.Current.MainWindow).SetPage(new CharacterEditView());
        }

        private void LaunchCharacter(Character character)
        {
            MessageBox.Show(
                $"Launching FFXIV for {character.Name}\nServer: {character.Server}\nJob: {character.Class}\nLevel: {character.Level}",
                "Launch",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
