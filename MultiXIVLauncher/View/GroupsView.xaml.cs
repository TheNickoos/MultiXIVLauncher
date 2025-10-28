using MultiXIVLauncher.Utils;
using MultiXIVLauncher.Views.Headers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace MultiXIVLauncher.Views
{
    /// <summary>
    /// Interaction logic for the view that manages and displays user groups.
    /// Allows adding, editing, and deleting groups dynamically with animations.
    /// </summary>
    public partial class GroupsView : UserControl
    {
        private bool isAddingGroup = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupsView"/> class and sets the header content.
        /// </summary>
        public GroupsView()
        {
            InitializeComponent();
            ((LauncherWindow)Application.Current.MainWindow).SetHeaderContent(new SettingsHeader());
        }

        /// <summary>
        /// Displays input fields for adding a new group.
        /// </summary>
        private void BtnAddGroup_Click(object sender, RoutedEventArgs e)
        {
            if (!isAddingGroup)
            {
                isAddingGroup = true;
                BtnAddGroup.Visibility = Visibility.Collapsed;
                ShowElement(TxtGroupName);
                ShowElement(BtnValidate);
            }
        }

        /// <summary>
        /// Validates and adds the new group to the list when the user confirms.
        /// </summary>
        private void BtnValidate_Click(object sender, RoutedEventArgs e)
        {
            if (isAddingGroup)
            {
                string newGroupName = TxtGroupName.Text.Trim();
                if (!string.IsNullOrEmpty(newGroupName))
                {
                    AddGroupCard(newGroupName);
                }

                isAddingGroup = false;
                HideElement(TxtGroupName);
                HideElement(BtnValidate);
                TxtGroupName.Text = string.Empty;
                BtnAddGroup.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Creates a visual card representing a group and adds it to the panel.
        /// </summary>
        private void AddGroupCard(string groupName)
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

            StackPanel info = new StackPanel { Orientation = Orientation.Vertical };
            info.Children.Add(new TextBlock
            {
                Text = groupName,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("ClrTextPrimary")
            });
            info.Children.Add(new TextBlock
            {
                Text = "0 characters",
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
                var editView = new GroupEditView();
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
                UIAnimationHelper.AnimateRemoval(card, () => GroupListPanel.Children.Remove(card));
            actions.Children.Add(deleteButton);

            content.Children.Add(info);
            content.Children.Add(actions);
            Grid.SetColumn(actions, 1);
            card.Child = content;

            GroupListPanel.Children.Add(card);

            UIAnimationHelper.AnimateAppearance(card);
        }

        /// <summary>
        /// Smoothly displays a UI element using a fade-in animation.
        /// </summary>
        private void ShowElement(UIElement element)
        {
            element.Visibility = Visibility.Visible;
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(150)));
            element.BeginAnimation(OpacityProperty, fadeIn);
        }

        /// <summary>
        /// Smoothly hides a UI element using a fade-out animation.
        /// </summary>
        private void HideElement(UIElement element)
        {
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(150)));
            fadeOut.Completed += (s, _) => element.Visibility = Visibility.Collapsed;
            element.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
