using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiXIVLauncher
{
    public partial class MainWindow : Window
    {
        private int currentPage = 0;
        private const int CharactersPerPage = 7;
        private List<Character> allCharacters = new List<Character>();
        private List<Character> filteredCharacters = new List<Character>();

        public MainWindow()
        {
            InitializeComponent();

            RefreshUIFromConfig();

            GroupComboBox.SelectionChanged += (s, e) => ApplyGroupFilterAndRender();
            LaunchGroupButton.Click += LaunchGroupButton_Click;
        }

        private void MogstationButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://secure.square-enix.com/account/app/svc/mogstation/");
        }

        private void StatusButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://is.xivup.com/");
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(this);
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        public void RefreshUIFromConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (!File.Exists(configPath)) return;

            var config = Config.Load(configPath);
            if (config == null) return;

            GroupComboBox.Items.Clear();

            var allItem = new ComboBoxItem { Content = Properties.Resources.AllGroup, Tag = -1 };
            GroupComboBox.Items.Add(allItem);

            if (config.Groups != null)
            {
                foreach (var group in config.Groups)
                {
                    var item = new ComboBoxItem { Content = group.Name, Tag = group.Id };
                    GroupComboBox.Items.Add(item);
                }
            }

            GroupComboBox.SelectedIndex = 0;

            allCharacters = config.Characters ?? new List<Character>();

            foreach (var ch in allCharacters)
                if (ch.GroupIds == null) ch.GroupIds = new List<int>();

            ApplyGroupFilterAndRender();
        }

        private void ApplyGroupFilterAndRender()
        {
            int selectedGroupId = -1;
            if (GroupComboBox.SelectedItem is ComboBoxItem sel && sel.Tag is int)
                selectedGroupId = (int)sel.Tag;

            if (selectedGroupId == -1)
            {
                filteredCharacters = new List<Character>(allCharacters);
            }
            else
            {
                filteredCharacters = allCharacters.FindAll(
                    c => c.GroupIds != null && c.GroupIds.Contains(selectedGroupId));
            }

            currentPage = 0;
            DisplayCurrentPage();
        }

        private void DisplayCurrentPage()
        {
            var groupBox = (GroupBox)FindName("CharacterGroupBox");
            if (groupBox == null)
                return;

            groupBox.Content = null;

            if (filteredCharacters == null || filteredCharacters.Count == 0)
            {
                var tb = new TextBlock
                {
                    Text = Properties.Resources.NoCharactersAvailable,
                    Foreground = Brushes.White,
                    Margin = new Thickness(10)
                };
                groupBox.Content = tb;
                return;
            }

            var dock = new DockPanel
            {
                LastChildFill = false
            };
            groupBox.Content = dock;

            if (filteredCharacters.Count > CharactersPerPage)
            {
                var navPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                var prev = new Button
                {
                    Content = "<",
                    Width = 36,
                    Height = 28,
                    Margin = new Thickness(5),
                    FontSize = 12,
                    Background = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                    Foreground = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(20, 90, 200)),
                    BorderThickness = new Thickness(1),
                    Cursor = Cursors.Hand
                };

                var next = new Button
                {
                    Content = ">",
                    Width = 36,
                    Height = 28,
                    Margin = new Thickness(5),
                    FontSize = 12,
                    Background = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                    Foreground = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(20, 90, 200)),
                    BorderThickness = new Thickness(1),
                    Cursor = Cursors.Hand
                };

                prev.Click += (s, e) =>
                {
                    if (currentPage > 0)
                    {
                        currentPage--;
                        DisplayCurrentPage();
                    }
                };

                next.Click += (s, e) =>
                {
                    if ((currentPage + 1) * CharactersPerPage < filteredCharacters.Count)
                    {
                        currentPage++;
                        DisplayCurrentPage();
                    }
                };

                var pageText = new TextBlock
                {
                    Text = $"Page {currentPage + 1}/{Math.Ceiling((double)filteredCharacters.Count / CharactersPerPage)}",
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 10, 0),
                    FontSize = 11,
                    FontWeight = FontWeights.Bold
                };

                navPanel.Children.Add(prev);
                navPanel.Children.Add(pageText);
                navPanel.Children.Add(next);

                DockPanel.SetDock(navPanel, Dock.Bottom);
                dock.Children.Add(navPanel);
            }

            var grid = new UniformGrid
            {
                Columns = 1,
                Rows = CharactersPerPage,
                Margin = new Thickness(10, 10, 10, 0)
            };

            int start = currentPage * CharactersPerPage;
            int end = Math.Min(start + CharactersPerPage, filteredCharacters.Count);

            for (int i = start; i < end; i++)
            {
                var character = filteredCharacters[i];

                var btn = new Button
                {
                    Content = character.Name,
                    Height = 26,
                    Margin = new Thickness(4, 2, 4, 2),
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Background = new SolidColorBrush(Color.FromRgb(44, 44, 44)),
                    Foreground = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(90, 90, 90)),
                    BorderThickness = new Thickness(1),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Tag = character.Id
                };

                var style = new Style(typeof(Button));
                style.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(44, 44, 44))));
                style.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
                style.Triggers.Add(new Trigger
                {
                    Property = Button.IsMouseOverProperty,
                    Value = true,
                    Setters = {
                        new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(70, 70, 70)))
                    }
                });
                btn.Style = style;

                btn.Click += CharacterButton_Click;
                grid.Children.Add(btn);
            }

            DockPanel.SetDock(grid, Dock.Top);
            dock.Children.Add(grid);
        }

        private void CharacterButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsLaunchingCharacter)
                return;

            var btn = sender as Button;
            int id = (int)btn.Tag;

            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            var config = Config.Load(configPath);
            var character = config.Characters.Find(c => c.Id == id);

            if (character != null)
                CharacterLauncher.LaunchCharacter(character, config, this);
        }

        public bool IsLaunchingCharacter { get; private set; } = false;

        public void SetLauncherInteractivity(bool enabled)
        {
            IsLaunchingCharacter = !enabled;

            foreach (var ctrl in FindVisualChildren<Button>(this))
            {
                ctrl.IsEnabled = enabled;
            }

            GroupComboBox.IsEnabled = enabled;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

        private async void LaunchGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsLaunchingCharacter)
                return;

            if (GroupComboBox.SelectedItem is ComboBoxItem selected)
            {
                int groupId = (int)selected.Tag;

                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                var config = Config.Load(configPath);

                if (config != null)
                    await GroupLauncher.LaunchGroupAsync(groupId, config, this);
            }
            else
            {
                MessageBox.Show(
                    Properties.Resources.SelectAGroupFirst,
                    Properties.Resources.NoGroupSelected,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }
}
