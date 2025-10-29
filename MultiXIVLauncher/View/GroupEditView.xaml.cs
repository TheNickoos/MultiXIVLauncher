using MultiXIVLauncher.Models;
using MultiXIVLauncher.Services;
using MultiXIVLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MultiXIVLauncher.Views
{
    /// <summary>
    /// View that allows editing a specific group's information and members.
    /// All modifications are stored temporarily and not saved until the user clicks "Save" in the header.
    /// </summary>
    public partial class GroupEditView : UserControl
    {
        /// <summary>
        /// The group currently being edited.
        /// </summary>
        private readonly Group CurrentGroup;

        /// <summary>
        /// Temporary list of all characters (copied from GroupsView).
        /// Used to manage membership assignment.
        /// </summary>
        private readonly List<Character> TemporaryCharacters;

        /// <summary>
        /// List of members currently in the group, bound to the ListBox.
        /// </summary>
        public ObservableCollection<Character> Members { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupEditView"/> class.
        /// </summary>
        /// <param name="group">The group being edited.</param>
        /// <param name="temporaryCharacters">A reference to the temporary character list.</param>
        private readonly GroupsView ParentView;

        public GroupEditView(Group group, List<Character> temporaryCharacters, GroupsView parentView)
        {
            InitializeComponent();

            CurrentGroup = group;
            TemporaryCharacters = temporaryCharacters;
            ParentView = parentView;

            TxtGroupName.Text = group.Name;

            var groupMembers = TemporaryCharacters
                .Where(c => c.GroupIds.Contains(group.Id))
                .ToList();

            Members = new ObservableCollection<Character>(groupMembers);
            MembersList.ItemsSource = Members;
        }
        /// <summary>
        /// Opens the "Add Member" window to add a new character to the group.
        /// Displays only characters that are not already members.
        /// </summary>
        private void OpenAddMemberWindow(object sender, RoutedEventArgs e)
        {
            var availableCharacters = TemporaryCharacters
                .Where(c => !c.GroupIds.Contains(CurrentGroup.Id))
                .Select(c => c.Name)
                .ToList();

            if (availableCharacters.Count == 0)
            {
                MessageBox.Show(
                    "All available characters are already part of this group.",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var addWindow = new AddMemberWindow(availableCharacters);
            addWindow.Owner = Application.Current.MainWindow;

            if (addWindow.ShowDialog() == true && addWindow.SelectedCharacter != null)
            {
                var selected = TemporaryCharacters.FirstOrDefault(c => c.Name == addWindow.SelectedCharacter.Name);
                if (selected != null)
                {
                    selected.GroupIds.Add(CurrentGroup.Id);
                    Members.Add(selected);

                    // Play appearance animation
                    var listBoxItem = (ListBoxItem)MembersList.ItemContainerGenerator.ContainerFromItem(selected);
                    if (listBoxItem != null)
                        AnimateListItemAppearance(listBoxItem);
                    else
                        MembersList.Dispatcher.InvokeAsync(() =>
                        {
                            var newItem = (ListBoxItem)MembersList.ItemContainerGenerator.ContainerFromItem(selected);
                            if (newItem != null)
                                AnimateListItemAppearance(newItem);
                        }, System.Windows.Threading.DispatcherPriority.Background);
                }
            }
            ParentView?.RefreshGroupCounts();
        }

        /// <summary>
        /// Removes a member from the group when the "Remove" button is clicked.
        /// Updates both the temporary group and character data.
        /// </summary>
        private void RemoveMember_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Character character)
            {
                // Remove from local observable collection
                Members.Remove(character);

                // Remove group from the character in the temp collection
                var tempChar = TemporaryCharacters.FirstOrDefault(c => c.Id == character.Id);
                tempChar?.GroupIds.Remove(CurrentGroup.Id);
                ParentView?.RefreshGroupCounts();

            }
        }

        /// <summary>
        /// Handles text changes in the group name TextBox.
        /// Updates the temporary group instance in real-time.
        /// </summary>
        private void TxtGroupName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CurrentGroup.Name = TxtGroupName.Text.Trim();
        }

        /// <summary>
        /// Returns to the Groups view when the user clicks the "Back" button.
        /// </summary>
        private void GoBackToGroups(object sender, RoutedEventArgs e)
        {
            ((LauncherWindow)Application.Current.MainWindow).SetPage(ParentView);
        }




        /// <summary>
        /// Animates the appearance of a ListBox item (fade-in and slide-up effect).
        /// </summary>
        /// <param name="item">The ListBoxItem to animate.</param>
        private static void AnimateListItemAppearance(ListBoxItem item)
        {
            item.Opacity = 0;
            item.RenderTransform = new TranslateTransform(0, 25);

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase()
            };
            var slideUp = new DoubleAnimation(25, 0, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase()
            };

            item.BeginAnimation(OpacityProperty, fadeIn);
            item.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);
        }
    }
}
