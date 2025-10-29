using MultiXIVLauncher.Models;
using System.Windows;
using System.Windows.Controls;

namespace MultiXIVLauncher.Views
{
    /// <summary>
    /// Interaction logic for the window that allows users to select and add a new preset item.
    /// </summary>
    public partial class AddPresetItemWindow : Window
    {
        /// <summary>
        /// Gets the preset item selected by the user.
        /// </summary>
        public PresetItem? SelectedItem { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddPresetItemWindow"/> class.
        /// </summary>
        public AddPresetItemWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Confirms the selected item and closes the window if a valid item is chosen.
        /// </summary>
        private void Validate_Click(object sender, RoutedEventArgs e)
        {
            if (ItemList.SelectedItem is ListBoxItem item)
            {
                SelectedItem = new PresetItem { Name = item.Content?.ToString() ?? string.Empty };
                DialogResult = true;
            }
        }
    }
}
