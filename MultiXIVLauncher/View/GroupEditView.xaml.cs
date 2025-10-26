using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MultiXIVLauncher.Views
{
    public partial class GroupEditView : UserControl
    {
        public ObservableCollection<Character> Members { get; set; }

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

        private void OpenAddMemberWindow(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddMemberWindow();
            addWindow.Owner = Application.Current.MainWindow;

            if (addWindow.ShowDialog() == true)
            {
                if (addWindow.SelectedCharacter != null)
                {
                    Members.Add(addWindow.SelectedCharacter);

                    var listBoxItem = (ListBoxItem)MembersList.ItemContainerGenerator.ContainerFromItem(addWindow.SelectedCharacter);
                    if (listBoxItem != null)
                    {
                        listBoxItem.Opacity = 0;
                        listBoxItem.RenderTransform = new TranslateTransform(0, 25);

                        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
                        {
                            EasingFunction = new QuadraticEase()
                        };
                        var slideUp = new DoubleAnimation(25, 0, TimeSpan.FromMilliseconds(250))
                        {
                            EasingFunction = new QuadraticEase()
                        };

                        listBoxItem.BeginAnimation(OpacityProperty, fadeIn);
                        listBoxItem.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);
                    }
                    else
                    {
                        MembersList.Dispatcher.InvokeAsync(() =>
                        {
                            var newItem = (ListBoxItem)MembersList.ItemContainerGenerator.ContainerFromItem(addWindow.SelectedCharacter);
                            if (newItem != null)
                            {
                                newItem.Opacity = 0;
                                newItem.RenderTransform = new TranslateTransform(0, 25);

                                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };
                                var slideUp = new DoubleAnimation(25, 0, TimeSpan.FromMilliseconds(250)) { EasingFunction = new QuadraticEase() };

                                newItem.BeginAnimation(OpacityProperty, fadeIn);
                                newItem.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);
                            }
                        }, System.Windows.Threading.DispatcherPriority.Background);
                    }
                }
            }
        }

        private void RemoveMember_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Character character)
            {
                Members.Remove(character);
            }
        }
        private void GoBackToGroups(object sender, RoutedEventArgs e)
        {
            var groupsView = new GroupsView();
            ((LauncherWindow)Application.Current.MainWindow).SetPage(groupsView);
        }

    }

    public class Character
    {
        public string Name { get; set; }
    }
}
