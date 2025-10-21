using System.Windows;

namespace MultiXIVLauncher
{
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();

            // Texte traduit depuis le fichier .resx
            LaunchCharacterTextBlock.Text = Properties.Resources.LaunchCharacter;
        }
    }
}
