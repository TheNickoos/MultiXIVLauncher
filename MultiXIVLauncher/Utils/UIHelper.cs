using MultiXIVLauncher.View.Headers;
using System.Windows;
using System.Windows.Controls;

namespace MultiXIVLauncher.Utils
{
    public static class UIHelper
    {
        /// <summary>
        /// Shows or hides the save/return buttons in the active SettingsHeader.
        /// </summary>
        public static void SetHeaderButtonsVisible(bool visible)
        {
            if (Application.Current.MainWindow is LauncherWindow mainWindow &&
                mainWindow.FindName("HeaderDynamicArea") is ContentControl headerArea &&
                headerArea.Content is SettingsHeader header)
            {
                header.SetButtonsVisible(visible);
            }
        }
    }
}
