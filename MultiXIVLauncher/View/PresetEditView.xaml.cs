using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MultiXIVLauncher.Views
{
    public partial class PresetEditView : UserControl
    {
        public PresetEditView()
        {
            InitializeComponent();
        }

        private void CopyFromCharacter_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Fonction à venir : copier la configuration d’un personnage !");
        }

        private void OpenPresetFolder_Click(object sender, RoutedEventArgs e)
        {
            string presetPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets", TxtPresetName.Text);
            if (System.IO.Directory.Exists(presetPath))
                Process.Start("explorer.exe", presetPath);
            else
                MessageBox.Show("Le dossier du preset n’existe pas encore.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SavePreset_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Preset \"{TxtPresetName.Text}\" sauvegardé avec succès !");
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            ((LauncherWindow)Application.Current.MainWindow).SetPage(new PresetsView());
        }
    }
}
