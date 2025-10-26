using MultiXIVLauncher.Views.Headers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MultiXIVLauncher.Views
{
    public partial class CharactersView : UserControl
    {
        private bool isAddingCharacter = false;

        public CharactersView()
        {
            InitializeComponent();
            ((LauncherWindow)Application.Current.MainWindow).SetHeaderContent(new SettingsHeader());
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
            if (isAddingCharacter)
            {
                string name = TxtCharacterName.Text.Trim();
                if (!string.IsNullOrEmpty(name))
                    AddCharacterCard(name);

                isAddingCharacter = false;
                HideElement(TxtCharacterName);
                HideElement(BtnValidate);
                TxtCharacterName.Text = string.Empty;
                BtnAddCharacter.Visibility = Visibility.Visible;
            }
        }

        private void AddCharacterCard(string name)
        {
            Border card = new Border
            {
                Style = (Style)FindResource("CardBorder"),
                Opacity = 0,
                RenderTransform = new TranslateTransform(0, 30),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = System.Windows.Media.Color.FromRgb(185, 147, 255),
                    BlurRadius = 25,
                    ShadowDepth = 0,
                    Opacity = 0.9
                }
            };

            Grid content = new Grid();
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel info = new StackPanel { Orientation = Orientation.Vertical };
            info.Children.Add(new TextBlock
            {
                Text = name,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("ClrTextPrimary")
            });
            info.Children.Add(new TextBlock
            {
                Text = "Spriggan — Classe inconnue",
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
                Content = "Modifier",
                Height = 30,
                Margin = new Thickness(0, 0, 8, 0),
                Style = (Style)FindResource("OutlineButton")
            };
            editButton.Click += (s, e) =>
            {
                var editView = new CharacterEditView();
                ((LauncherWindow)Application.Current.MainWindow).SetPage(editView);
            };
            actions.Children.Add(editButton);

            var deleteButton = new Button
            {
                Content = "Supprimer",
                Height = 30,
                Style = (Style)FindResource("OutlineButton")
            };
            deleteButton.Click += (s, e) => RemoveCharacterCard(card);
            actions.Children.Add(deleteButton);

            content.Children.Add(info);
            content.Children.Add(actions);
            Grid.SetColumn(actions, 1);
            card.Child = content;

            if (FindName("CharacterListPanel") is StackPanel list)
                list.Children.Add(card);

            AnimateCardAppearance(card);
        }

        private void RemoveCharacterCard(Border card)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
            var slideDown = new DoubleAnimation(0, 30, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
            card.BeginAnimation(OpacityProperty, fadeOut);
            card.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideDown);

            if (card.Effect is System.Windows.Media.Effects.DropShadowEffect shadow)
            {
                var shadowFade = new DoubleAnimation(shadow.Opacity, 0, TimeSpan.FromMilliseconds(250));
                shadow.BeginAnimation(System.Windows.Media.Effects.DropShadowEffect.OpacityProperty, shadowFade);
            }

            fadeOut.Completed += (s, _) =>
            {
                if (FindName("CharacterListPanel") is StackPanel list)
                    list.Children.Remove(card);
            };
        }

        private void AnimateCardAppearance(Border card)
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
            var slideUp = new DoubleAnimation(30, 0, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
            card.BeginAnimation(OpacityProperty, fadeIn);
            card.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);

            if (card.Effect is System.Windows.Media.Effects.DropShadowEffect shadow)
            {
                var shadowFade = new DoubleAnimation(0.9, 0.2, TimeSpan.FromMilliseconds(800))
                {
                    BeginTime = TimeSpan.FromMilliseconds(250),
                    EasingFunction = new QuadraticEase()
                };
                shadow.BeginAnimation(System.Windows.Media.Effects.DropShadowEffect.OpacityProperty, shadowFade);
            }
        }

        private void ShowElement(UIElement element)
        {
            element.Visibility = Visibility.Visible;
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
            element.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void HideElement(UIElement element)
        {
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
            fadeOut.Completed += (s, _) => element.Visibility = Visibility.Collapsed;
            element.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
