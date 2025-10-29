using MultiXIVLauncher.Models;
using MultiXIVLauncher.Services;
using MultiXIVLauncher.Utils;
using MultiXIVLauncher.Utils.Interfaces;
using MultiXIVLauncher.Views.Headers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MultiXIVLauncher.Views
{
    /// <summary>
    /// Interaction logic for the presets management view.
    /// Allows the user to create, edit, and delete presets dynamically with animations.
    /// Changes are applied only to a temporary list until saved.
    /// </summary>
    public partial class PresetsView : UserControl, ISavableView
    {
        private bool isAddingPreset = false;

        /// <summary>
        /// Temporary copy of presets to edit safely before saving.
        /// </summary>
        private readonly List<Preset> tempPresets = new List<Preset>();

        public PresetsView()
        {
            InitializeComponent();
            ((LauncherWindow)Application.Current.MainWindow).SetHeaderContent(new SettingsHeader());

            // Copy presets from config to temporary list
            foreach (var preset in ConfigManager.Current.Presets)
                tempPresets.Add(new Preset
                {
                    Id = preset.Id,
                    Name = preset.Name,
                    FolderPath = preset.FolderPath
                });

            // Display all presets
            LoadPresets();
        }

        /// <summary>
        /// Reloads the UI from the temporary list (public so child views can trigger it).
        /// </summary>
        public void RefreshList() => LoadPresets();

        /// <summary>
        /// Loads all presets from the temporary list into the UI.
        /// </summary>
        private void LoadPresets()
        {
            PresetListPanel.Children.Clear();

            foreach (var preset in tempPresets)
                AddPresetCard(preset, false);
        }

        private void BtnAddPreset_Click(object sender, RoutedEventArgs e)
        {
            if (!isAddingPreset)
            {
                isAddingPreset = true;
                BtnAddPreset.Visibility = Visibility.Collapsed;
                ShowElement(TxtPresetName);
                ShowElement(BtnValidate);
            }
        }

        private void BtnValidate_Click(object sender, RoutedEventArgs e)
        {
            if (isAddingPreset)
            {
                string newPresetName = TxtPresetName.Text.Trim();
                if (!string.IsNullOrEmpty(newPresetName))
                {
                    var newPreset = new Preset
                    {
                        Id = tempPresets.Count + 1,
                        Name = newPresetName,
                        FolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets", newPresetName)
                    };

                    tempPresets.Add(newPreset);
                    AddPresetCard(newPreset);
                }

                isAddingPreset = false;
                HideElement(TxtPresetName);
                HideElement(BtnValidate);
                TxtPresetName.Text = string.Empty;
                BtnAddPreset.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Creates and adds a visual card representing a preset with action buttons.
        /// </summary>
        private void AddPresetCard(Preset preset, bool animate = true)
        {
            Border card = new Border
            {
                Style = (Style)FindResource("CardBorder"),
                Opacity = 0,
                RenderTransform = new TranslateTransform(0, 30),
                Effect = new DropShadowEffect
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

            var nameText = new TextBlock
            {
                Text = preset.Name,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("ClrTextPrimary"),
                VerticalAlignment = VerticalAlignment.Center
            };

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
                // Open editor with reference to THIS view so we can refresh on return
                var editView = new PresetEditView(preset, this);
                ((LauncherWindow)Application.Current.MainWindow).SetPage(editView);
            };

            var deleteButton = new Button
            {
                Content = "Delete",
                Height = 30,
                Style = (Style)FindResource("OutlineButton")
            };
            deleteButton.Click += (s, e) =>
            {
                tempPresets.Remove(preset);
                UIAnimationHelper.AnimateRemoval(card, () => PresetListPanel.Children.Remove(card));
            };

            actions.Children.Add(editButton);
            actions.Children.Add(deleteButton);

            content.Children.Add(nameText);
            content.Children.Add(actions);
            Grid.SetColumn(actions, 1);
            card.Child = content;

            PresetListPanel.Children.Add(card);

            if (animate)
                UIAnimationHelper.AnimateAppearance(card);
            else
                card.Opacity = 1;
        }

        public void Save()
        {
            ConfigManager.Current.Presets.Clear();
            ConfigManager.Current.Presets.AddRange(tempPresets);
            ConfigManager.Save();
        }

        private static void ShowElement(UIElement element)
        {
            element.Visibility = Visibility.Visible;
            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(150)));
            element.BeginAnimation(OpacityProperty, fadeIn);
        }

        private static void HideElement(UIElement element)
        {
            var fadeOut = new System.Windows.Media.Animation.DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(150)));
            fadeOut.Completed += (s, _) => element.Visibility = Visibility.Collapsed;
            element.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
