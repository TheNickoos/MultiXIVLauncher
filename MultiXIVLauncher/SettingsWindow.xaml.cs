using System.Windows;
using System.Windows.Controls;

namespace MultiXIVLauncher
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            InitializePresetView();
        }

        private void InitializePresetView()
        {
            PresetAddButton.Visibility = Visibility.Visible;
            PresetGrid.Visibility = Visibility.Collapsed;
        }

        private void PresetAddButton_Click(object sender, RoutedEventArgs e)
        {
            PresetAddButton.Visibility = Visibility.Collapsed;
            PresetGrid.Visibility = Visibility.Visible;
            PresetNameLabel.Visibility = Visibility.Visible;
            PresetNameTextBox.Visibility = Visibility.Visible;
            RectangleEdit.Visibility = Visibility.Visible;
            PresetSaveButton.Visibility = Visibility.Visible;
            PresetDeleteButton.Visibility = Visibility.Collapsed;
            PresetExploreButton.Visibility = Visibility.Collapsed;
        }
    }
}
