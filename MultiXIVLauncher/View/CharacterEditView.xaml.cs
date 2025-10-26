using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MultiXIVLauncher.Views
{
    public partial class CharacterEditView : UserControl
    {
        public CharacterEditView()
        {
            InitializeComponent();
        }

        private void ChangePortrait_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Sélectionner une image de portrait",
                Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (dialog.ShowDialog() == true)
            {
                (VisualTreeHelper.GetChild(this, 0) as Grid)
                    ?.FindName("PortraitImage");
            }
        }

        private void OpenCharacterFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", @"C:\Users\Public\Documents\MultiXIVLauncher\Characters");
        }

        private void SaveCharacter_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Modifications enregistrées pour " + TxtCharacterName.Text + " !");
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            ((LauncherWindow)Application.Current.MainWindow).SetPage(new CharactersView());
        }
    }
}
