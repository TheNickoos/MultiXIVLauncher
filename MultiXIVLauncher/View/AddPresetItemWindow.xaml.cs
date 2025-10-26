using MultiXIVLauncher.Models;
using System.Windows;
using System.Windows.Controls;

namespace MultiXIVLauncher.Views
{
    public partial class AddPresetItemWindow : Window
    {
        public PresetItem SelectedItem { get; private set; }

        public AddPresetItemWindow()
        {
            InitializeComponent();
        }

        private void Validate_Click(object sender, RoutedEventArgs e)
        {
            if (ItemList.SelectedItem is ListBoxItem item)
            {
                SelectedItem = new PresetItem { Name = item.Content.ToString() };
                DialogResult = true;
            }
        }
    }
}
