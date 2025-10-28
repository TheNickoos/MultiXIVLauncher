using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MultiXIVLauncher.Models;

namespace MultiXIVLauncher.Views
{
    /// <summary>
    /// Interaction logic for the group editor view.
    /// Allows managing the members of a specific group (adding, removing, and displaying characters).
    /// </summary>
    public partial class GroupEditView : UserControl
    {
        /// <summary>
        /// Collection of characters that belong to the current group.
        /// </summary>
        public ObservableCollection<Character> Members { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupEditView"/> class
        /// and populates the group with sample members.
        /// </summary>
        public GroupEditView()
        {
            InitializeComponent();

            Members = new ObservableCollection<Character>
            {
                new Character { Name = "Lyna Jade" },
                new Character { Name = "Miles Brenner" },
                new Character { Name = "Tigris Paw" }
            };

            MembersList.ItemsSource = Members;
        }

        /// <summary>
        /// Opens the "Add Member" window, allowing the user to select a new character to add to the group.
        /// </summary>
        private void OpenAddMemberWindow(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddMemberWindow();
            addWindow.Owner = Application.Current.MainWindow;

            if (addWindow.ShowDialog() == true)
            {
                if (addWindow.SelectedCharacter != null)
                {
                    Members.Add(addWindow.SelectedCharacter);

                    // Attempt to retrieve the newly added item from the ListBox
                    var listBoxItem = (ListBoxItem)MembersList.ItemContainerGenerator.ContainerFromItem(addWindow.SelectedCharacter);
                    if (listBoxItem != null)
                    {
                        AnimateListItemAppearance(listBoxItem);
                    }
                    else
                    {
                        // If the item container is not yet generated, retry asynchronously
                        MembersList.Dispatcher.InvokeAsync(() =>
                        {
                            var newItem = (ListBoxItem)MembersList.ItemContainerGenerator.ContainerFromItem(addWindow.SelectedCharacter);
                            if (newItem != null)
                                AnimateListItemAppearance(newItem);
                        }, System.Windows.Threading.DispatcherPriority.Background);
                    }
                }
            }
        }

        /// <summary>
        /// Removes a member from the group when the "Remove" button is clicked.
        /// </summary>
        private void RemoveMember_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Character character)
            {
                Members.Remove(character);
            }
        }

        /// <summary>
        /// Returns to the groups list view.
        /// </summary>
        private void GoBackToGroups(object sender, RoutedEventArgs e)
        {
            var groupsView = new GroupsView();
            ((LauncherWindow)Application.Current.MainWindow).SetPage(groupsView);
        }

        /// <summary>
        /// Applies fade and slide-up animations when a new member appears in the list.
        /// </summary>
        private void AnimateListItemAppearance(ListBoxItem item)
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
