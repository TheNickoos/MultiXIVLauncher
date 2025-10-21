using System.Windows;

namespace MultiXIVLauncher
{
    public partial class GroupLoadingWindow : Window
    {
        public GroupLoadingWindow()
        {
            InitializeComponent();
        }

        public void UpdateProgress(string groupName, string characterName, int current, int total)
        {
            Dispatcher.Invoke(() =>
            {
                GroupNameText.Text = string.Format(Properties.Resources.LaunchGroupName, groupName);
                CharacterProgressText.Text = $"({current}/{total}) {characterName}";
                ProgressBarLaunch.Value = ((double)current / total) * 100;
            });
        }
    }
}
