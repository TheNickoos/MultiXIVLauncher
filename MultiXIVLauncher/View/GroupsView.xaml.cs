using MultiXIVLauncher.Models;
using MultiXIVLauncher.Services;
using MultiXIVLauncher.Utils;
using MultiXIVLauncher.Utils.Interfaces;
using MultiXIVLauncher.Views.Headers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace MultiXIVLauncher.Views
{
    /// <summary>
    /// View responsible for displaying and managing user groups.
    /// Groups and characters are edited in temporary memory and saved only when requested.
    /// </summary>
    public partial class GroupsView : UserControl, ISavableView
    {
        /// <summary>
        /// Temporary in-memory list of groups being edited.
        /// </summary>
        private ObservableCollection<Group>? TemporaryGroups { get; set; }

        /// <summary>
        /// Temporary in-memory list of characters reflecting live group membership edits.
        /// </summary>
        private List<Character>? TemporaryCharacters { get; set; }

        private bool isAddingGroup = false;

        /// <summary>
        /// Default constructor. Initializes the Groups view with fresh temporary data from configuration.
        /// </summary>
        public GroupsView()
        {
            InitializeComponent();
            ((LauncherWindow)Application.Current.MainWindow).SetHeaderContent(new SettingsHeader());

            LoadTemporaryData();
            RefreshGroupList();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="GroupsView"/> class.
        /// Loads temporary groups and characters from configuration and sets the header.
        /// </summary>
        public GroupsView(ObservableCollection<Group> existingGroups, List<Character> existingCharacters)
        {
            InitializeComponent();
            ((LauncherWindow)Application.Current.MainWindow).SetHeaderContent(new SettingsHeader());

            TemporaryGroups = existingGroups;
            TemporaryCharacters = existingCharacters;

            RefreshGroupList();
        }


        /// <summary>
        /// Loads all groups and characters into temporary in-memory collections.
        /// </summary>
        private void LoadTemporaryData()
        {
            // Clone groups
            TemporaryGroups = new ObservableCollection<Group>(
                ConfigManager.Current.Groups.Select(g => new Group
                {
                    Id = g.Id,
                    Name = g.Name
                })
            );

            // Clone characters with their current group assignments
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

        /// <summary>
        /// Refreshes the visual list of groups displayed in the interface.
        /// </summary>
        private void RefreshGroupList()
        {
            GroupListPanel.Children.Clear();

            foreach (var group in TemporaryGroups)
            {
                AddGroupCard(group);
            }
        }

        /// <summary>
        /// Handles the "Add Group" button click.
        /// Displays the input field for the new group name.
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
        /// Handles the "Validate" button click.
        /// Adds a new group to the temporary collection.
        /// </summary>
        private void BtnValidate_Click(object sender, RoutedEventArgs e)
        {
            if (isAddingGroup)
            {
                string newGroupName = TxtGroupName.Text.Trim();
                if (!string.IsNullOrEmpty(newGroupName))
                {
                    var newGroup = new Group
                    {
                        Id = GenerateNextGroupId(),
                        Name = newGroupName
                    };

                    TemporaryGroups.Add(newGroup);
                    AddGroupCard(newGroup);
                }

                isAddingGroup = false;
                HideElement(TxtGroupName);
                HideElement(BtnValidate);
                TxtGroupName.Text = string.Empty;
                BtnAddGroup.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Updates the member count display for all visible group cards.
        /// </summary>
        public void RefreshGroupCounts()
        {
            foreach (var child in GroupListPanel.Children.OfType<Border>())
            {
                if (child.Tag is Tuple<Group, TextBlock> data)
                {
                    var group = data.Item1;
                    var textBlock = data.Item2;
                    textBlock.Text = $"{GetCharacterCount(group.Id)} characters";
                }
            }
        }


        /// <summary>
        /// Creates a visual card representing a group and adds it to the UI panel.
        /// </summary>
        /// <param name="group">The group to visually represent.</param>
        private void AddGroupCard(Group group)
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

            // === LEFT PANEL ===
            StackPanel info = new StackPanel { Orientation = Orientation.Vertical };

            var nameText = new TextBlock
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("ClrTextPrimary")
            };

            nameText.SetBinding(TextBlock.TextProperty, new Binding("Name")
            {
                Source = group,
                Mode = BindingMode.OneWay
            });


            var membersCount = new TextBlock
            {
                Style = (Style)FindResource("BodyText"),
                Opacity = 0.8
            };

            // we’ll update it manually when returning from edit
            membersCount.Text = $"{GetCharacterCount(group.Id)} characters";

            // store a tag for easier updates later
            card.Tag = new Tuple<Group, TextBlock>(group, membersCount);


            info.Children.Add(nameText);
            info.Children.Add(membersCount);

            // === RIGHT PANEL (ACTIONS) ===
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
                var editView = new GroupEditView(group, TemporaryCharacters, this);
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
                UIAnimationHelper.AnimateRemoval(card, () =>
                {
                    TemporaryGroups.Remove(group);

                    // Also remove group from all characters
                    foreach (var c in TemporaryCharacters)
                        c.GroupIds.Remove(group.Id);

                    GroupListPanel.Children.Remove(card);
                });
            };
            actions.Children.Add(deleteButton);

            content.Children.Add(info);
            content.Children.Add(actions);
            Grid.SetColumn(actions, 1);
            card.Child = content;

            GroupListPanel.Children.Add(card);
            UIAnimationHelper.AnimateAppearance(card);
        }

        /// <summary>
        /// Generates a new unique group ID based on the temporary collection.
        /// </summary>
        private int GenerateNextGroupId()
        {
            if (TemporaryGroups.Count == 0)
                return 1;
            return TemporaryGroups.Max(g => g.Id) + 1;
        }

        /// <summary>
        /// Counts how many characters currently belong to a given group.
        /// </summary>
        /// <param name="groupId">The ID of the group.</param>
        /// <returns>The number of characters in this group.</returns>
        private int GetCharacterCount(int groupId)
        {
            return TemporaryCharacters.Count(c => c.GroupIds.Contains(groupId));
        }

        /// <summary>
        /// Smoothly displays a UI element using a fade-in animation.
        /// </summary>
        private static void ShowElement(UIElement element)
        {
            element.Visibility = Visibility.Visible;
            var fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(150)));
            element.BeginAnimation(OpacityProperty, fadeIn);
        }

        /// <summary>
        /// Smoothly hides a UI element using a fade-out animation.
        /// </summary>
        private static void HideElement(UIElement element)
        {
            var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(150)));
            fadeOut.Completed += (s, _) => element.Visibility = Visibility.Collapsed;
            element.BeginAnimation(OpacityProperty, fadeOut);
        }

        /// <summary>
        /// Saves the temporary groups and characters to the configuration file.
        /// Called when the "Save" button in the header is clicked.
        /// </summary>
        public void Save()
        {
            // Apply groups
            ConfigManager.Current.Groups.Clear();
            ConfigManager.Current.Groups.AddRange(TemporaryGroups);

            // Apply characters
            ConfigManager.Current.Characters.Clear();
            ConfigManager.Current.Characters.AddRange(TemporaryCharacters);

            // Persist configuration
            ConfigManager.Save();
            Logger.Info("Groups and characters saved successfully from GroupsView.");
        }
    }
}
