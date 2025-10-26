using MultiXIVLauncher.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MultiXIVLauncher.Views
{
    public partial class PresetsView : UserControl
    {
        private bool isAddingPreset = false;

        public PresetsView()
        {
            InitializeComponent();
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
                    AddPresetCard(newPresetName);
                }

                isAddingPreset = false;
                HideElement(TxtPresetName);
                HideElement(BtnValidate);
                TxtPresetName.Text = string.Empty;
                BtnAddPreset.Visibility = Visibility.Visible;
            }
        }

        private void AddPresetCard(string presetName)
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
                Text = presetName,
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
                Content = "Modifier",
                Height = 30,
                Margin = new Thickness(0, 0, 8, 0),
                Style = (Style)FindResource("OutlineButton")
            };
            editButton.Click += (s, e) =>
            {
                var editView = new PresetEditView();
                ((LauncherWindow)Application.Current.MainWindow).SetPage(editView);
            };

            var deleteButton = new Button
            {
                Content = "Supprimer",
                Height = 30,
                Style = (Style)FindResource("OutlineButton")
            };
            deleteButton.Click += (s, e) => UIAnimationHelper.AnimateRemoval(card, () => PresetListPanel.Children.Remove(card));

            actions.Children.Add(editButton);
            actions.Children.Add(deleteButton);

            content.Children.Add(nameText);
            content.Children.Add(actions);
            Grid.SetColumn(actions, 1);
            card.Child = content;

            PresetListPanel.Children.Add(card);

            UIAnimationHelper.AnimateAppearance(card);
        }

        private void ShowElement(UIElement element)
        {
            element.Visibility = Visibility.Visible;
            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(150)));
            element.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void HideElement(UIElement element)
        {
            var fadeOut = new System.Windows.Media.Animation.DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(150)));
            fadeOut.Completed += (s, _) => element.Visibility = Visibility.Collapsed;
            element.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
