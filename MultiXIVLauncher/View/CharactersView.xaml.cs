using MultiXIVLauncher.Views.Headers;
using MultiXIVLauncher.Utils.Interfaces;
using MultiXIVLauncher.Services;
using MultiXIVLauncher.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;

namespace MultiXIVLauncher.Views
{
    /// <summary>
    /// Interaction logic for the view that manages and displays the list of characters.
    /// Supports adding, editing, and deleting character cards dynamically.
    /// </summary>
    public partial class CharactersView : UserControl, ISavableView
    {
        private bool isAddingCharacter = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharactersView"/> class and sets the header.
        /// </summary>
        public CharactersView()
        {
            InitializeComponent();
            ((LauncherWindow)Application.Current.MainWindow).SetHeaderContent(new SettingsHeader());

            LoadCharacters();
        }

        /// <summary>
        /// Loads characters from configuration and displays them.
        /// </summary>
        private void LoadCharacters()
        {
            CharacterListPanel.Children.Clear();

            foreach (var character in ConfigManager.Current.Characters)
                AddCharacterCard(character);
        }


        /// <summary>
        /// Displays the input field to add a new character.
        /// </summary>
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

        /// <summary>
        /// Validates the character creation and adds a new card to the list.
        /// </summary>
        private void BtnValidate_Click(object sender, RoutedEventArgs e)
        {
            if (isAddingCharacter)
            {
                string name = TxtCharacterName.Text.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    var newCharacter = Character.Create(name);
                    ConfigManager.Current.Characters.Add(newCharacter);
                    AddCharacterCard(newCharacter);

                }

                isAddingCharacter = false;
                HideElement(TxtCharacterName);
                HideElement(BtnValidate);
                TxtCharacterName.Text = string.Empty;
                BtnAddCharacter.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Creates and adds a new character card to the list with animation.
        /// </summary>
        private void AddCharacterCard(Character character, bool animate = true)
        {
            Border card = new Border
            {
                Style = (Style)FindResource("CardBorder"),
                Opacity = animate ? 0 : 1,
                RenderTransform = new TranslateTransform(0, animate ? 30 : 0),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Color.FromRgb(185, 147, 255),
                    BlurRadius = 25,
                    ShadowDepth = 0,
                    Opacity = 0.9
                }
            };

            Grid content = new Grid();
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // 🧩 Infos du personnage
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
                Text = $"{character.Server ?? "Unknown server"} — {character.Class ?? "Unknown class"}",
                Style = (Style)FindResource("BodyText"),
                Opacity = 0.8
            });

            // 🧩 Actions
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
                var editView = new CharacterEditView();
                editView.LoadCharacter(character);
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
                RemoveCharacterCard(card);
                ConfigManager.Current.Characters.Remove(character);
                ConfigManager.Save();
            };
            actions.Children.Add(deleteButton);

            content.Children.Add(info);
            content.Children.Add(actions);
            Grid.SetColumn(actions, 1);
            card.Child = content;

            CharacterListPanel.Children.Add(card);

            if (animate)
                AnimateCardAppearance(card);
        }


        /// <summary>
        /// Removes a character card from the list with fade and slide animations.
        /// </summary>
        private void RemoveCharacterCard(Border card)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
            var slideDown = new DoubleAnimation(0, 30, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
            card.BeginAnimation(OpacityProperty, fadeOut);
            card.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideDown);

            fadeOut.Completed += (s, _) => CharacterListPanel.Children.Remove(card);
        }

        /// <summary>
        /// Saves all characters currently loaded in the list.
        /// </summary>
        public void Save()
        {
            ConfigManager.Save();
            Logger.Info("Characters saved successfully.");
        }

        /// <summary>
        /// Applies entry animations when a new card appears.
        /// </summary>
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
